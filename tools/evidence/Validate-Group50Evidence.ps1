param(
    [string]$RepositoryPath = "C:\Projects\LifeOS"
)

$ErrorActionPreference = "Stop"

$EvidencePath = Join-Path $RepositoryPath "docs\screenshot-groups\group-50-teams-foundation"
$Expected = @(
    "01_Teams_Capability_Enabled.png",
    "02_Team_Channel_Selection.png",
    "03_Teams_Channel_Message_Candidate.png",
    "04_Thread_Reply_Provenance.png",
    "05_Meeting_Context_Linked.png",
    "06_Reviewable_Action_Suggestion.png",
    "07_Teams_Access_Lost_Recovery.png",
    "08_Group50_Validation_Clean_Sync.png"
)

if (-not (Test-Path -LiteralPath $EvidencePath)) {
    throw "Group 50 evidence directory not found: $EvidencePath"
}

$PngFiles = @(Get-ChildItem -LiteralPath $EvidencePath -Filter "*.png" -File)

if ($PngFiles.Count -ne 8) {
    throw "Expected exactly 8 Group 50 PNG files; found $($PngFiles.Count)."
}

$ActualNames = @($PngFiles.Name | Sort-Object)
$ExpectedNames = @($Expected | Sort-Object)

if (($ActualNames -join "`n") -ne ($ExpectedNames -join "`n")) {
    throw "Group 50 evidence filenames do not match the approved manifest."
}

foreach ($File in $PngFiles) {
    if ($File.Length -lt 10000) {
        throw "Evidence file appears too small: $($File.Name)"
    }

    $Bytes = [System.IO.File]::ReadAllBytes($File.FullName)
    if ($Bytes.Length -lt 8 -or
        $Bytes[0] -ne 0x89 -or
        $Bytes[1] -ne 0x50 -or
        $Bytes[2] -ne 0x4E -or
        $Bytes[3] -ne 0x47) {
        throw "Evidence file is not a valid PNG signature: $($File.Name)"
    }
}

Write-Host "Group 50 evidence validated: exactly 8 approved PNG files." -ForegroundColor Green
