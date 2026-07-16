param(
    [string]$RepositoryPath = "C:\Projects\LifeOS"
)

$ErrorActionPreference = "Stop"

$EvidencePath = Join-Path $RepositoryPath "docs\screenshot-groups\group-49-microsoft-files"
$Expected = @(
    "01_Microsoft_Files_Capability.png",
    "02_OneDrive_Bounded_Selection.png",
    "03_OneDrive_Integration_Candidates.png",
    "04_File_Provenance_And_Freshness.png",
    "05_SharePoint_Site_Library_Selection.png",
    "06_SharePoint_Candidate_Project_Link.png",
    "07_Source_Removed_Permission_Lost.png",
    "08_Group49_Validation_Clean_Sync.png"
)

if (-not (Test-Path -LiteralPath $EvidencePath)) {
    throw "Group 49 evidence directory not found: $EvidencePath"
}

$PngFiles = @(Get-ChildItem -LiteralPath $EvidencePath -Filter "*.png" -File)

if ($PngFiles.Count -ne 8) {
    throw "Expected exactly 8 Group 49 PNG files; found $($PngFiles.Count)."
}

$ActualNames = @($PngFiles.Name | Sort-Object)
$ExpectedNames = @($Expected | Sort-Object)

if (($ActualNames -join "`n") -ne ($ExpectedNames -join "`n")) {
    throw "Group 49 evidence filenames do not match the approved manifest."
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

Write-Host "Group 49 evidence validated: exactly 8 approved PNG files." -ForegroundColor Green
