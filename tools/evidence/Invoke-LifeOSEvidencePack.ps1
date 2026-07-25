[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $GroupDirectory,

    [Parameter(Mandatory)]
    [string] $OutputPath,

    [string] $Title = 'LifeOS evidence pack',

    [string] $Python = 'python'
)

$ErrorActionPreference = 'Stop'
$group = Resolve-Path -LiteralPath $GroupDirectory
$script = Join-Path $PSScriptRoot 'Build-LifeOSEvidencePdf.py'

& $Python -c 'import reportlab'
if ($LASTEXITCODE -ne 0) {
    $requirements = Join-Path $PSScriptRoot 'requirements.txt'
    throw "Evidence PDF dependency missing. Install it with: $Python -m pip install -r `"$requirements`""
}

& $Python $script $group $OutputPath --title $Title
if ($LASTEXITCODE -ne 0) {
    throw 'Evidence PDF generation failed.'
}

Write-Host "Evidence PDF created: $OutputPath"
