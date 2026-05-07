param(
    [switch]$RunSelfTest
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$outDir = Join-Path $root 'bin'
$buildDir = Join-Path $root 'build'
$source = Join-Path $root 'src\AudioTray.cs'
$manifest = Join-Path $root 'app.manifest'
$iconPng = Join-Path $root 'icon.png'
$iconIco = Join-Path $buildDir 'AudioTray.ico'
$exe = Join-Path $outDir 'AudioTray.exe'
$compiler = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not (Test-Path $compiler)) {
    $compiler = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

if (-not (Test-Path $compiler)) {
    throw 'Cannot find .NET Framework csc.exe compiler.'
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null
New-Item -ItemType Directory -Force -Path $buildDir | Out-Null

function Convert-PngToIco {
    param(
        [Parameter(Mandatory = $true)][string]$PngPath,
        [Parameter(Mandatory = $true)][string]$IcoPath
    )

    Add-Type -AssemblyName System.Drawing

    $sizes = @(256, 64, 48, 32, 16)
    $sourceImage = [System.Drawing.Image]::FromFile($PngPath)
    $images = New-Object System.Collections.Generic.List[object]

    try {
        foreach ($size in $sizes) {
            $bitmap = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
            try {
                $graphics.Clear([System.Drawing.Color]::Transparent)
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.DrawImage($sourceImage, 0, 0, $size, $size)

                $stream = New-Object System.IO.MemoryStream
                $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
                $images.Add([pscustomobject]@{
                    Size = $size
                    Bytes = $stream.ToArray()
                })
                $stream.Dispose()
            } finally {
                $graphics.Dispose()
                $bitmap.Dispose()
            }
        }
    } finally {
        $sourceImage.Dispose()
    }

    $file = [System.IO.File]::Create($IcoPath)
    $writer = New-Object System.IO.BinaryWriter $file
    try {
        $writer.Write([UInt16]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]$images.Count)

        $offset = 6 + (16 * $images.Count)
        foreach ($image in $images) {
            $writer.Write([byte]$(if ($image.Size -eq 256) { 0 } else { $image.Size }))
            $writer.Write([byte]$(if ($image.Size -eq 256) { 0 } else { $image.Size }))
            $writer.Write([byte]0)
            $writer.Write([byte]0)
            $writer.Write([UInt16]1)
            $writer.Write([UInt16]32)
            $writer.Write([UInt32]$image.Bytes.Length)
            $writer.Write([UInt32]$offset)
            $offset += $image.Bytes.Length
        }

        foreach ($image in $images) {
            $writer.Write($image.Bytes)
        }
    } finally {
        $writer.Dispose()
        $file.Dispose()
    }
}

$compilerArgs = @(
    '/nologo',
    '/target:winexe',
    '/platform:anycpu',
    '/optimize+',
    "/win32manifest:$manifest",
    "/out:$exe",
    '/reference:System.dll',
    '/reference:System.Core.dll',
    '/reference:System.Drawing.dll',
    '/reference:System.Windows.Forms.dll',
    '/reference:System.Xml.dll',
    '/reference:System.Xml.Serialization.dll'
)

if (Test-Path $iconPng) {
    Convert-PngToIco -PngPath $iconPng -IcoPath $iconIco
    Copy-Item -LiteralPath $iconIco -Destination (Join-Path $outDir 'AudioTray.ico') -Force
    $compilerArgs += "/win32icon:$iconIco"
}

$compilerArgs += $source

& $compiler @compilerArgs

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}

Write-Host "Built $exe"

if ($RunSelfTest) {
    & $exe --self-test
}
