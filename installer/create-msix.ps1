param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$SourceDir,

    [Parameter(Mandatory = $true)]
    [string]$IdentityName,

    [Parameter(Mandatory = $true)]
    [string]$Publisher,

    [string]$OutputDir = "installer/Output",
    [string]$DisplayName = "PTREE Gold",
    [string]$PublisherDisplayName = "Todd Whitehead",
    [string]$Description = "PTREE Gold",
    [string]$CertificateBase64,
    [string]$CertificatePassword
)

$ErrorActionPreference = 'Stop'

if (-not (Get-Command makeappx.exe -ErrorAction SilentlyContinue)) {
    throw "makeappx.exe was not found on PATH. Install the Windows SDK and ensure makeappx.exe is available."
}

if (-not (Test-Path $SourceDir)) {
    throw "Source directory not found: $SourceDir"
}

$sourceRoot = (Resolve-Path $SourceDir).Path
$templatePath = Join-Path $PSScriptRoot 'msix/AppxManifest.xml.template'
if (-not (Test-Path $templatePath)) {
    throw "AppxManifest template not found at $templatePath"
}

if (-not (Test-Path (Join-Path $sourceRoot 'PTG.exe'))) {
    throw "PTG.exe not found in source directory: $sourceRoot"
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
$outputRoot = (Resolve-Path $OutputDir).Path

$stagingPath = Join-Path $outputRoot 'msix-staging'
if (Test-Path $stagingPath) {
    Remove-Item -Recurse -Force $stagingPath
}
New-Item -ItemType Directory -Path $stagingPath | Out-Null
Copy-Item -Path (Join-Path $sourceRoot '*') -Destination $stagingPath -Recurse -Force

$assetsPath = Join-Path $stagingPath 'Assets'
New-Item -ItemType Directory -Path $assetsPath -Force | Out-Null

Add-Type -AssemblyName System.Drawing

function New-Logo {
    param(
        [int]$Width,
        [int]$Height,
        [string]$Path
    )

    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear([System.Drawing.Color]::FromArgb(255, 8, 32, 48))
        $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 215, 0))
        try {
            $fontSize = [Math]::Max(12, [Math]::Round($Height * 0.45))
            $font = New-Object System.Drawing.Font('Consolas', $fontSize, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
            try {
                $text = 'PTG'
                $size = $graphics.MeasureString($text, $font)
                $x = ($Width - $size.Width) / 2
                $y = ($Height - $size.Height) / 2
                $graphics.DrawString($text, $font, $brush, $x, $y)
            }
            finally {
                $font.Dispose()
            }
        }
        finally {
            $brush.Dispose()
        }
    }
    finally {
        $graphics.Dispose()
    }

    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

New-Logo -Width 44 -Height 44 -Path (Join-Path $assetsPath 'Square44x44Logo.png')
New-Logo -Width 50 -Height 50 -Path (Join-Path $assetsPath 'StoreLogo.png')
New-Logo -Width 150 -Height 150 -Path (Join-Path $assetsPath 'Square150x150Logo.png')
New-Logo -Width 310 -Height 150 -Path (Join-Path $assetsPath 'Wide310x150Logo.png')

$coreVersion = ($Version -split '-', 2)[0]
$versionParts = @($coreVersion -split '\.')
if ($versionParts.Count -gt 4) {
    throw "Version '$Version' has more than 4 numeric parts and cannot be used for MSIX packaging."
}

foreach ($part in $versionParts) {
    if ($part -notmatch '^\d+$') {
        throw "Version '$Version' must contain only numeric parts before any prerelease suffix."
    }
}

while ($versionParts.Count -lt 4) {
    $versionParts += '0'
}

$msixVersion = ($versionParts -join '.')

$template = Get-Content -Path $templatePath -Raw
$manifest = $template
    .Replace('__IDENTITY_NAME__', $IdentityName)
    .Replace('__PUBLISHER__', $Publisher)
    .Replace('__VERSION__', $msixVersion)
    .Replace('__DISPLAY_NAME__', $DisplayName)
    .Replace('__PUBLISHER_DISPLAY_NAME__', $PublisherDisplayName)
    .Replace('__DESCRIPTION__', $Description)

$manifestPath = Join-Path $stagingPath 'AppxManifest.xml'
Set-Content -Path $manifestPath -Value $manifest -Encoding UTF8

$msixPath = Join-Path $outputRoot "ptg-$Version-win-x64.msix"
if (Test-Path $msixPath) {
    Remove-Item $msixPath -Force
}

makeappx.exe pack /o /d $stagingPath /p $msixPath | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "MSIX packaging failed with exit code $LASTEXITCODE."
}

if ([string]::IsNullOrWhiteSpace($CertificateBase64) -or [string]::IsNullOrWhiteSpace($CertificatePassword)) {
    Write-Host "Created unsigned MSIX package: $msixPath"
    Write-Warning 'CertificateBase64/CertificatePassword not provided, so package signing was skipped.'
    return
}

if (-not (Get-Command signtool.exe -ErrorAction SilentlyContinue)) {
    throw "signtool.exe was not found on PATH. Install the Windows SDK and ensure signtool.exe is available."
}

$certificatePath = Join-Path ([System.IO.Path]::GetTempPath()) ("ptg-msix-signing-{0}.pfx" -f ([System.Guid]::NewGuid().ToString('N')))
$importedCertificate = $null

try {
    [System.IO.File]::WriteAllBytes($certificatePath, [System.Convert]::FromBase64String($CertificateBase64))

    $certFlags = [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::PersistKeySet
    $importedCertificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $importedCertificate.Import($certificatePath, $CertificatePassword, $certFlags)

    $certificateStore = New-Object System.Security.Cryptography.X509Certificates.X509Store('My', 'CurrentUser')
    try {
        $certificateStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        $certificateStore.Add($importedCertificate)
    }
    finally {
        $certificateStore.Close()
    }

    signtool.exe sign /fd SHA256 /sha1 $importedCertificate.Thumbprint /s My $msixPath | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "MSIX signing failed with exit code $LASTEXITCODE."
    }
}
finally {
    if ($importedCertificate -and (Test-Path "Cert:\CurrentUser\My\$($importedCertificate.Thumbprint)")) {
        Remove-Item -Path "Cert:\CurrentUser\My\$($importedCertificate.Thumbprint)" -Force
    }
    if (Test-Path $certificatePath) {
        Remove-Item -Path $certificatePath -Force
    }
}

Write-Host "Created signed MSIX package: $msixPath"
