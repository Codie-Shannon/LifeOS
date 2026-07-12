param(
    [string]$RepoPath = "C:\Projects\LifeOS"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$failed = $false

function Fail([string]$Message) {
    Write-Host $Message -ForegroundColor Red
    $script:failed = $true
}

$tracked = @(& git -C $RepoPath ls-files)
if ($LASTEXITCODE -ne 0) { throw "git ls-files failed." }

# Handoff files must not exist anywhere.
$handoffs = @(
    Get-ChildItem $RepoPath -Recurse -Force -File -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name -match "(?i)handoff" -or
        $_.Name -match "(?i)pre[_ -]?screenshot" -or $_.Name -match "(?i)context[_ -]?drop"
    }
)

if ($handoffs.Count -gt 0) {
    Fail "Forbidden handoff files exist:"
    $handoffs | ForEach-Object { Write-Host "  $($_.FullName)" }
}

# Only actual manual-test documents at repo root/docs root are violations.
$misplacedManual = @(
    $tracked | Where-Object {
        $name = [System.IO.Path]::GetFileName($_)
        $rootOrDocsRoot =
            ($_ -notmatch "/") -or
            ($_ -match "^docs/[^/]+$")

        $manualDocument =
            $name -match "(?i)(manual.*(test|verification)|test.*checklist|release.*check)"

        $rootOrDocsRoot -and
        $manualDocument -and
        $_ -notmatch "^docs/manual-tests/"
    }
)

if ($misplacedManual.Count -gt 0) {
    Fail "Manual-test documents are outside docs/manual-tests/:"
    $misplacedManual | ForEach-Object { Write-Host "  $_" }
}

# Only actual private runtime directories are forbidden.
$privateRuntime = @(
    $tracked | Where-Object {
        $_ -match "(?i)(^|/)(connector-config|connector-cache|connector-data|private-evidence|oauth-cache|token-cache|lifecycle-cache)(/|$)"
    }
)

if ($privateRuntime.Count -gt 0) {
    Fail "Private runtime directories are tracked:"
    $privateRuntime | ForEach-Object { Write-Host "  $_" }
}

$sensitiveNames = @(
    $tracked | Where-Object {
        $_ -match "(?i)(^|/)(\.env(\..*)?|credentials.*\.json|client_secret.*\.json|oauth.*\.json|token.*\.json|secrets\.json)$" -or
        $_ -match "(?i)\.(pfx|p12|pem|key)$"
    }
)

if ($sensitiveNames.Count -gt 0) {
    Fail "Sensitive filenames are tracked:"
    $sensitiveNames | ForEach-Object { Write-Host "  $_" }
}

$temporary = @(
    $tracked | Where-Object {
        $_ -match "(^|/)(bin|obj|TestResults|artifacts)/" -or
        $_ -match "(?i)\.(tmp|temp|bak|old|orig|rej|log|zip|7z|rar)$"
    }
)

if ($temporary.Count -gt 0) {
    Fail "Temporary/build files are tracked:"
    $temporary | ForEach-Object { Write-Host "  $_" }
}

if ($failed) { exit 1 }

Write-Host "Repository hygiene validation passed." -ForegroundColor Green
exit 0

