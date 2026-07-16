[CmdletBinding()]
param(
    [Parameter()]
    [string]$Repository = 'C:\Projects\LifeOS',

    [Parameter()]
    [switch]$RequireImages
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScenarioPath = Join-Path $Repository 'tools\evidence\group-48-scenarios.json'
$EvidencePath = Join-Path $Repository 'docs\screenshot-groups\group-48-microsoft-mail-calendar'
$VersionPath = Join-Path $Repository 'docs\lifeos-version-state.json'

if (-not (Test-Path -LiteralPath $ScenarioPath -PathType Leaf)) {
    throw "Group 48 scenario manifest is missing: $ScenarioPath"
}

$Manifest = [System.IO.File]::ReadAllText(
    $ScenarioPath,
    [System.Text.Encoding]::UTF8
) | ConvertFrom-Json

$Scenarios = @($Manifest.scenarios)
if ($Scenarios.Count -ne 8) {
    throw "Group 48 requires exactly 8 scenarios; found $($Scenarios.Count)."
}

$Orders = @($Scenarios | Select-Object -ExpandProperty order)
if (($Orders -join ',') -ne '1,2,3,4,5,6,7,8') {
    throw 'Group 48 scenario order must be exactly 1 through 8.'
}

$Names = @($Scenarios | Select-Object -ExpandProperty file)
if (($Names | Select-Object -Unique).Count -ne 8) {
    throw 'Group 48 screenshot filenames must be unique.'
}

if (-not (Test-Path -LiteralPath $VersionPath -PathType Leaf)) {
    throw "Version state is missing: $VersionPath"
}

$Version = [System.IO.File]::ReadAllText(
    $VersionPath,
    [System.Text.Encoding]::UTF8
) | ConvertFrom-Json

if ([string]$Version.desktopVersion -ne '9.0.0-alpha.3' -or
    [int]$Version.desktopGroup -ne 48) {
    throw 'Unexpected Group 48 release identity.'
}

$Images = @()
if (Test-Path -LiteralPath $EvidencePath -PathType Container) {
    $Images = @(
        Get-ChildItem -LiteralPath $EvidencePath -Filter '*.png' -File |
            Sort-Object Name
    )
}

if ($RequireImages) {
    if ($Images.Count -ne 8) {
        throw "Group 48 requires exactly 8 PNG files; found $($Images.Count)."
    }

    $ActualNames = @($Images | Select-Object -ExpandProperty Name)
    if (($ActualNames -join "`n") -ne (($Names | Sort-Object) -join "`n")) {
        throw "Group 48 image names do not match the scenario manifest."
    }
}
elseif ($Images.Count -ne 0) {
    throw "Pack 1 image boundary failed: expected zero screenshots; found $($Images.Count)."
}

Write-Host 'GROUP 48 EVIDENCE CONTRACT PASS' -ForegroundColor Green
Write-Host 'Scenarios: 8/8 unique and ordered' -ForegroundColor Green
Write-Host 'Release: v9.0.0-alpha.3' -ForegroundColor Green
Write-Host ("Images: " + $(if ($RequireImages) { 'exactly 8 approved PNG files present' } else { 'zero screenshots present for Pack 1' })) -ForegroundColor Green
