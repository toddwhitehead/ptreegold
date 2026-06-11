param(
    [Parameter(Mandatory = $true)]
    [string]$SourceDir,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$IdentityName,

    [Parameter(Mandatory = $true)]
    [string]$Publisher,

    [string]$PublisherDisplayName = 'Todd Whitehead',
    [string]$ManifestTemplate = 'installer/msix/AppxManifest.xml',
    [string]$AssetsDir = 'installer/msix/Images',
    [string]$OutputDir = 'installer/Output',
    [string]$CertificatePath,
    [string]$CertificatePassword
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-WindowsSdkTool {
    param([Parameter(Mandatory = $true)][string]$ToolName)

    $tool = Get-ChildItem 'C:\Program Files (x86)\Windows Kits\10\bin' -Filter $ToolName -Recurse |
        Where-Object { $_.FullName -match '\\x64\\' } |
        Sort-Object FullName -Descending |
        Select-Object -First 1

    if (-not $tool) {
        throw "Unable to locate $ToolName in the Windows SDK."
    }

    return $tool.FullName
}

function Convert-ToMsixVersion {
    param([Parameter(Mandatory = $true)][string]$RawVersion)

    $normalized = $RawVersion.Trim().TrimStart('v')
    $normalized = $normalized -replace '-.*$', ''
    $parts = $normalized.Split('.', [System.StringSplitOptions]::RemoveEmptyEntries)

    if ($parts.Count -lt 1 -or $parts.Count -gt 3) {
        throw "Version '$RawVersion' must contain 1 to 3 numeric segments."
    }

    $numericParts = foreach ($part in $parts) {
        if ($part -notmatch '^\d+$') {
            throw "Version '$RawVersion' contains a non-numeric segment: '$part'."
        }

        [int]$part
    }

    while ($numericParts.Count -lt 3) {
        $numericParts += 0
    }

    return '{0}.{1}.{2}.0' -f $numericParts[0], $numericParts[1], $numericParts[2]
}

$resolvedSourceDir = (Resolve-Path $SourceDir).Path
$resolvedManifestTemplate = (Resolve-Path $ManifestTemplate).Path
$resolvedAssetsDir = (Resolve-Path $AssetsDir).Path
$msixVersion = Convert-ToMsixVersion -RawVersion $Version
$stageDir = Join-Path $env:RUNNER_TEMP 'ptg-msix-layout'
$outputPath = Join-Path (Resolve-Path '.').Path $OutputDir
$packagePath = Join-Path $outputPath ("ptg-$($Version.Trim().TrimStart('v'))-win-x64.msix")

if (Test-Path $stageDir) {
    Remove-Item $stageDir -Recurse -Force
}

New-Item -ItemType Directory -Path $stageDir | Out-Null
Copy-Item (Join-Path $resolvedSourceDir '*') $stageDir -Recurse -Force
New-Item -ItemType Directory -Path (Join-Path $stageDir 'Images') | Out-Null
Copy-Item (Join-Path $resolvedAssetsDir '*') (Join-Path $stageDir 'Images') -Recurse -Force

$manifestContent = Get-Content $resolvedManifestTemplate -Raw
$manifestContent = $manifestContent.Replace('__IDENTITY_NAME__', $IdentityName)
$manifestContent = $manifestContent.Replace('__PUBLISHER__', $Publisher)
$manifestContent = $manifestContent.Replace('__VERSION__', $msixVersion)
$manifestContent = $manifestContent.Replace('__PUBLISHER_DISPLAY_NAME__', $PublisherDisplayName)
Set-Content -Path (Join-Path $stageDir 'AppxManifest.xml') -Value $manifestContent -Encoding UTF8

New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

$makeAppx = Get-WindowsSdkTool -ToolName 'makeappx.exe'
& $makeAppx pack /d $stageDir /p $packagePath /o

if ($LASTEXITCODE -ne 0) {
    throw "makeappx.exe failed with exit code $LASTEXITCODE."
}

if ($CertificatePath) {
    $resolvedCertPath = (Resolve-Path $CertificatePath).Path
    $signTool = Get-WindowsSdkTool -ToolName 'signtool.exe'
    $signArgs = @('sign', '/fd', 'SHA256', '/f', $resolvedCertPath)

    if ($CertificatePassword) {
        $signArgs += @('/p', $CertificatePassword)
    }

    $signArgs += $packagePath
    & $signTool @signArgs

    if ($LASTEXITCODE -ne 0) {
        throw "signtool.exe failed with exit code $LASTEXITCODE."
    }
}
else {
    Write-Host 'No certificate configured; created an unsigned MSIX package.'
}

Write-Host "Created MSIX package at $packagePath"
