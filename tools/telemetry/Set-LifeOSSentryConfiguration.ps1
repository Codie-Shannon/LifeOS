[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Read-SentryDsn {
    param(
        [Parameter(Mandatory)]
        [string] $ProjectName
    )

    $secureValue = Read-Host "Paste the Sentry DSN for $ProjectName" -AsSecureString
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureValue)

    try {
        $value = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }

    $uri = $null
    if (-not [Uri]::TryCreate($value, [UriKind]::Absolute, [ref] $uri) -or
        $uri.Scheme -ne 'https' -or
        [string]::IsNullOrWhiteSpace($uri.UserInfo) -or
        [string]::IsNullOrWhiteSpace($uri.Host)) {
        throw "The $ProjectName value is not a valid HTTPS Sentry DSN."
    }

    if ($uri.Host -notmatch '(^|\.)sentry\.io$') {
        throw "The $ProjectName DSN does not use a sentry.io ingestion host."
    }

    return @{
        Value = $value
        Host = $uri.Host
    }
}

if ([string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) {
    throw 'LOCALAPPDATA is unavailable.'
}

$desktop = Read-SentryDsn -ProjectName 'lifeos-desktop'
$android = Read-SentryDsn -ProjectName 'lifeos-android'

$configurationDirectory = Join-Path $env:LOCALAPPDATA 'LifeOS\telemetry'
$configurationPath = Join-Path $configurationDirectory 'sentry.json'

New-Item -ItemType Directory -Path $configurationDirectory -Force | Out-Null

$configuration = [ordered]@{
    SchemaVersion = 1
    EnabledByDefault = $false
    RequiresExplicitOptIn = $true
    SendDefaultPii = $false
    DesktopDsn = $desktop.Value
    AndroidDsn = $android.Value
}

$configuration |
    ConvertTo-Json |
    Set-Content -LiteralPath $configurationPath -Encoding utf8

Write-Host ''
Write-Host 'LifeOS Sentry configuration saved locally.'
Write-Host "Path: $configurationPath"
Write-Host "Desktop ingestion host: $($desktop.Host)"
Write-Host "Android ingestion host: $($android.Host)"
Write-Host 'Crash reporting remains disabled by default and requires explicit user opt-in.'
