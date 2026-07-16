param(
    [string]$RepositoryPath = "C:\Projects\LifeOS"
)

$ErrorActionPreference = "Stop"

$EvidencePath = Join-Path $RepositoryPath "docs\screenshot-groups\group-51-google-workspace"
$Expected = @(
    "01_Google_Workspace_Provider_Connected.png",
    "02_Google_Capability_Catalogue.png",
    "03_Gmail_Google_Calendar_Candidates.png",
    "04_Gmail_Provenance_Thread_Attachments.png",
    "05_Google_Drive_Contact_Task_Candidates.png",
    "06_Google_Contacts_Tasks_Review.png",
    "07_Google_Revoked_Consent_Recovery.png",
    "08_Group51_Validation_Clean_Sync.png"
)

if (-not (Test-Path -LiteralPath $EvidencePath)) {
    throw "Group 51 evidence directory not found: $EvidencePath"
}

$PngFiles = @(Get-ChildItem -LiteralPath $EvidencePath -Filter "*.png" -File)

if ($PngFiles.Count -ne 8) {
    throw "Expected exactly 8 Group 51 PNG files; found $($PngFiles.Count)."
}

$ActualNames = @($PngFiles.Name | Sort-Object)
$ExpectedNames = @($Expected | Sort-Object)

if (($ActualNames -join "`n") -ne ($ExpectedNames -join "`n")) {
    throw "Group 51 evidence filenames do not match the approved manifest."
}

foreach ($File in $PngFiles) {
    if ($File.Length -lt 10000) {
        throw "Evidence file appears too small: $($File.Name)"
    }

    $Bytes = [System.IO.File]::ReadAllBytes($File.FullName)

    if (
        $Bytes.Length -lt 8 -or
        $Bytes[0] -ne 0x89 -or
        $Bytes[1] -ne 0x50 -or
        $Bytes[2] -ne 0x4E -or
        $Bytes[3] -ne 0x47
    ) {
        throw "Evidence file is not a valid PNG signature: $($File.Name)"
    }
}

Write-Host "Group 51 evidence validated: exactly 8 approved PNG files." -ForegroundColor Green
