[CmdletBinding()]
param(
    [string] $ProjectPath = (
        Join-Path $PSScriptRoot '..\..\src\LifeOS.Mobile\LifeOS.Mobile.csproj'),
    [string] $KeystorePath = (
        Join-Path $env:LOCALAPPDATA 'LifeOS\signing\lifecontrolos-release.keystore'),
    [string] $KeyAlias = 'lifecontrolos-release',
    [switch] $SkipTests
)

$ErrorActionPreference = 'Stop'

$resolvedProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$resolvedKeystorePath = (Resolve-Path -LiteralPath $KeystorePath).Path
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

if (-not $SkipTests) {
    & dotnet test (
        Join-Path $repositoryRoot 'tests\LifeOS.Mobile.Tests\LifeOS.Mobile.Tests.csproj'
    ) -c Release --no-restore

    if ($LASTEXITCODE -ne 0) {
        throw 'Mobile tests failed. The signed package was not published.'
    }
}

$securePassword = Read-Host `
    'Enter the Android release keystore password' `
    -AsSecureString
$passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR(
    $securePassword)

try {
    $plainPassword =
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
    $env:LifeOSAndroidSigningPassword = $plainPassword

    & dotnet publish $resolvedProjectPath `
        -f net10.0-android `
        -c Release `
        --no-restore `
        -p:AndroidPackageFormats=apk `
        -p:AndroidKeyStore=true `
        "-p:AndroidSigningKeyStore=$resolvedKeystorePath" `
        "-p:AndroidSigningKeyAlias=$KeyAlias" `
        -p:AndroidSigningKeyPass=env:LifeOSAndroidSigningPassword `
        -p:AndroidSigningStorePass=env:LifeOSAndroidSigningPassword

    if ($LASTEXITCODE -ne 0) {
        throw 'Android private-beta publish failed.'
    }
}
finally {
    Remove-Item Env:\LifeOSAndroidSigningPassword -ErrorAction SilentlyContinue
    $plainPassword = $null
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
}

$publishDirectory = Join-Path (
    Split-Path $resolvedProjectPath) 'bin\Release\net10.0-android\publish'
$signedApk = Get-ChildItem -LiteralPath $publishDirectory `
        -Filter '*-Signed.apk' `
        -File |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $signedApk) {
    throw "No signed APK was found in $publishDirectory."
}

Write-Host ''
Write-Host 'Private-beta APK published successfully.' -ForegroundColor Green
Write-Host "APK: $($signedApk.FullName)"
