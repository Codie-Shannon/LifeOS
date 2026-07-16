[CmdletBinding()]
param(
    [Parameter()]
    [string]$Repository = 'C:\Projects\LifeOS',

    [Parameter()]
    [switch]$RequireImages
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ScenarioPath = Join-Path $Repository 'tools\evidence\group-47-scenarios.json'
$EvidenceFolder = Join-Path $Repository 'docs\screenshot-groups\group-47-integration-inbox'

if (-not (Test-Path -LiteralPath $ScenarioPath -PathType Leaf)) {
    throw "Group 47 scenario manifest not found: $ScenarioPath"
}

$Manifest = Get-Content -LiteralPath $ScenarioPath -Raw | ConvertFrom-Json

if ([int]$Manifest.group -ne 47) {
    throw "Unexpected evidence group: $($Manifest.group)"
}

if ([string]$Manifest.release -ne 'v9.0.0-alpha.2') {
    throw "Unexpected release identity: $($Manifest.release)"
}

if ([int]$Manifest.expectedCount -ne 8) {
    throw "Expected exactly 8 scenarios but manifest says $($Manifest.expectedCount)."
}

$Scenarios = @($Manifest.scenarios)
if ($Scenarios.Count -ne 8) {
    throw "Expected 8 scenario records but found $($Scenarios.Count)."
}

$Ids = @($Scenarios | ForEach-Object { [string]$_.id })
$Files = @($Scenarios | ForEach-Object { [string]$_.fileName })

if (@($Ids | Sort-Object -Unique).Count -ne 8) {
    throw 'Group 47 scenario IDs are not unique.'
}

if (@($Files | Sort-Object -Unique).Count -ne 8) {
    throw 'Group 47 screenshot names are not unique.'
}

for ($Index = 1; $Index -le 8; $Index++) {
    $ExpectedPrefix = '{0:D2}_' -f $Index

    if (-not $Files[$Index - 1].StartsWith(
            $ExpectedPrefix,
            [System.StringComparison]::Ordinal)) {
        throw "Scenario $Index does not use the expected ordered filename prefix $ExpectedPrefix"
    }

    if (-not $Files[$Index - 1].EndsWith(
            '.png',
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Scenario $Index is not a PNG filename."
    }
}

$ActualPng = @(
    Get-ChildItem -LiteralPath $EvidenceFolder -Filter '*.png' -File -ErrorAction SilentlyContinue
)

if ($RequireImages) {
    $Missing = @(
        foreach ($FileName in $Files) {
            $Path = Join-Path $EvidenceFolder $FileName
            if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
                $Path
            }
        }
    )

    if ($Missing.Count -gt 0) {
        throw "Missing Group 47 screenshots:`n$($Missing -join "`n")"
    }

    $UnexpectedPng = @(
        $ActualPng | Where-Object { $_.Name -notin $Files }
    )

    if ($UnexpectedPng.Count -gt 0) {
        throw "Unexpected Group 47 PNG files:`n$($UnexpectedPng.FullName -join "`n")"
    }
}
elseif ($ActualPng.Count -ne 0) {
    throw "Group 47 Pack 1 must not contain screenshots:`n$($ActualPng.FullName -join "`n")"
}

Write-Host 'GROUP 47 EVIDENCE CONTRACT PASS' -ForegroundColor Green
Write-Host 'Scenarios: 8/8 unique and ordered' -ForegroundColor Green
Write-Host 'Release: v9.0.0-alpha.2' -ForegroundColor Green

if ($RequireImages) {
    Write-Host 'Images: exactly 8 approved PNG files present' -ForegroundColor Green
}
else {
    Write-Host 'Pack 1 image boundary: zero screenshots present' -ForegroundColor Green
}
