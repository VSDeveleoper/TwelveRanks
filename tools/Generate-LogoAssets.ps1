[CmdletBinding()]
param(
    [string]$MasterLogoPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$taskRepositoryRoot = Split-Path -Parent $PSScriptRoot
$taskAppAssetsDirectory = Join-Path $taskRepositoryRoot 'TwelveRanks.App\Assets'
$taskStoreAssetsDirectory = Join-Path $taskRepositoryRoot 'StoreSubmissionAssets\StoreLogos'

if ([string]::IsNullOrWhiteSpace($MasterLogoPath)) {
    $MasterLogoPath = Join-Path $taskAppAssetsDirectory 'Branding\TwelveRanks_Logo_Master_v2.png'
}

$resolvedMasterLogoPath = (Resolve-Path -LiteralPath $MasterLogoPath).Path
if (-not $resolvedMasterLogoPath.StartsWith($taskRepositoryRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "The master logo must be inside the repository: $resolvedMasterLogoPath"
}

New-Item -ItemType Directory -Path $taskStoreAssetsDirectory -Force | Out-Null

$taskBackgroundColor = [System.Drawing.Color]::FromArgb(255, 20, 107, 67)
$taskEmblemCrop = [System.Drawing.Rectangle]::new(177, 60, 900, 900)
$taskWordmarkCrop = [System.Drawing.Rectangle]::new(100, 950, 1054, 150)

function Set-TaskHighQualityGraphics {
    param([System.Drawing.Graphics]$Graphics)

    $Graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $Graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $Graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $Graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
}

function New-TaskCroppedBitmap {
    param(
        [System.Drawing.Image]$Source,
        [System.Drawing.Rectangle]$SourceCrop,
        [int]$Width,
        [int]$Height
    )

    $result = [System.Drawing.Bitmap]::new(
        $Width,
        $Height,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($result)

    try {
        Set-TaskHighQualityGraphics -Graphics $graphics
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.DrawImage(
            $Source,
            [System.Drawing.Rectangle]::new(0, 0, $Width, $Height),
            $SourceCrop,
            [System.Drawing.GraphicsUnit]::Pixel)
    }
    finally {
        $graphics.Dispose()
    }

    return $result
}

function Save-TaskCroppedPng {
    param(
        [System.Drawing.Image]$Source,
        [System.Drawing.Rectangle]$SourceCrop,
        [int]$Width,
        [int]$Height,
        [string]$Destination
    )

    $bitmap = New-TaskCroppedBitmap `
        -Source $Source `
        -SourceCrop $SourceCrop `
        -Width $Width `
        -Height $Height

    try {
        $bitmap.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $bitmap.Dispose()
    }
}

function Save-TaskFullPng {
    param(
        [System.Drawing.Image]$Source,
        [int]$Width,
        [int]$Height,
        [string]$Destination
    )

    $fullCrop = [System.Drawing.Rectangle]::new(0, 0, $Source.Width, $Source.Height)
    Save-TaskCroppedPng `
        -Source $Source `
        -SourceCrop $fullCrop `
        -Width $Width `
        -Height $Height `
        -Destination $Destination
}

function Convert-TaskGreenToTransparent {
    param([System.Drawing.Bitmap]$Bitmap)

    for ($taskY = 0; $taskY -lt $Bitmap.Height; $taskY++) {
        for ($taskX = 0; $taskX -lt $Bitmap.Width; $taskX++) {
            $pixel = $Bitmap.GetPixel($taskX, $taskY)
            $greenDominance = [Math]::Min(
                [int]$pixel.G - [int]$pixel.R,
                [int]$pixel.G - [int]$pixel.B)

            if ($pixel.G -ge 45 -and $greenDominance -gt 2) {
                $alpha = if ($greenDominance -ge 38) {
                    0
                }
                else {
                    [int][Math]::Round(255 * (38 - $greenDominance) / 36)
                }

                $Bitmap.SetPixel(
                    $taskX,
                    $taskY,
                    [System.Drawing.Color]::FromArgb(
                        [Math]::Clamp($alpha, 0, 255),
                        $pixel.R,
                        $pixel.G,
                        $pixel.B))
            }
        }
    }
}

function Save-TaskHorizontalLockup {
    param(
        [System.Drawing.Image]$Source,
        [int]$Width,
        [int]$Height,
        [string]$Destination
    )

    $canvas = [System.Drawing.Bitmap]::new(
        $Width,
        $Height,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($canvas)

    $outerMargin = [int][Math]::Round($Height * 0.05)
    $emblemSize = [int][Math]::Round($Height * 0.88)
    $emblemY = [int][Math]::Round(($Height - $emblemSize) / 2)
    $wordmarkX = $outerMargin + $emblemSize + [int][Math]::Round($Height * 0.04)
    $wordmarkWidth = $Width - $wordmarkX - $outerMargin
    $wordmarkHeight = [int][Math]::Round($wordmarkWidth * $taskWordmarkCrop.Height / $taskWordmarkCrop.Width)
    $wordmarkY = [int][Math]::Round(($Height - $wordmarkHeight) / 2)

    $emblem = New-TaskCroppedBitmap `
        -Source $Source `
        -SourceCrop $taskEmblemCrop `
        -Width $emblemSize `
        -Height $emblemSize
    $wordmark = New-TaskCroppedBitmap `
        -Source $Source `
        -SourceCrop $taskWordmarkCrop `
        -Width $wordmarkWidth `
        -Height $wordmarkHeight

    try {
        Convert-TaskGreenToTransparent -Bitmap $emblem
        Convert-TaskGreenToTransparent -Bitmap $wordmark

        Set-TaskHighQualityGraphics -Graphics $graphics
        $graphics.Clear($taskBackgroundColor)
        $graphics.DrawImage($emblem, $outerMargin, $emblemY, $emblemSize, $emblemSize)
        $graphics.DrawImage($wordmark, $wordmarkX, $wordmarkY, $wordmarkWidth, $wordmarkHeight)

        $canvas.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $emblem.Dispose()
        $wordmark.Dispose()
        $graphics.Dispose()
        $canvas.Dispose()
    }
}

function Get-TaskPngBytes {
    param(
        [System.Drawing.Image]$Source,
        [System.Drawing.Rectangle]$SourceCrop,
        [int]$Size
    )

    $bitmap = New-TaskCroppedBitmap `
        -Source $Source `
        -SourceCrop $SourceCrop `
        -Width $Size `
        -Height $Size
    $stream = [System.IO.MemoryStream]::new()

    try {
        $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
        return $stream.ToArray()
    }
    finally {
        $stream.Dispose()
        $bitmap.Dispose()
    }
}

function Save-TaskWindowsIcon {
    param(
        [System.Drawing.Image]$Source,
        [System.Drawing.Rectangle]$SourceCrop,
        [string]$Destination
    )

    $iconSizes = @(16, 24, 32, 48, 64, 128, 256)
    $pngImages = foreach ($iconSize in $iconSizes) {
        ,(Get-TaskPngBytes -Source $Source -SourceCrop $SourceCrop -Size $iconSize)
    }

    $stream = [System.IO.File]::Create($Destination)
    $writer = [System.IO.BinaryWriter]::new($stream)

    try {
        $writer.Write([uint16]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]$iconSizes.Count)

        $imageOffset = 6 + (16 * $iconSizes.Count)
        for ($taskIndex = 0; $taskIndex -lt $iconSizes.Count; $taskIndex++) {
            $iconSize = $iconSizes[$taskIndex]
            $pngImage = $pngImages[$taskIndex]
            $dimensionByte = if ($iconSize -eq 256) { 0 } else { $iconSize }

            $writer.Write([byte]$dimensionByte)
            $writer.Write([byte]$dimensionByte)
            $writer.Write([byte]0)
            $writer.Write([byte]0)
            $writer.Write([uint16]1)
            $writer.Write([uint16]32)
            $writer.Write([uint32]$pngImage.Length)
            $writer.Write([uint32]$imageOffset)

            $imageOffset += $pngImage.Length
        }

        foreach ($pngImage in $pngImages) {
            $writer.Write([byte[]]$pngImage)
        }
    }
    finally {
        $writer.Dispose()
        $stream.Dispose()
    }
}

$masterLogo = [System.Drawing.Image]::FromFile($resolvedMasterLogoPath)

try {
    Save-TaskCroppedPng -Source $masterLogo -SourceCrop $taskEmblemCrop -Width 900 -Height 900 `
        -Destination (Join-Path $taskAppAssetsDirectory 'Branding\TwelveRanks_Emblem_Master.png')

    $packageSquareAssets = @(
        @{ File = 'LockScreenLogo.scale-200.png'; Size = 48 },
        @{ File = 'Square150x150Logo.scale-200.png'; Size = 300 },
        @{ File = 'Square44x44Logo.scale-200.png'; Size = 88 },
        @{ File = 'Square44x44Logo.targetsize-24_altform-unplated.png'; Size = 24 },
        @{ File = 'StoreLogo.png'; Size = 50 },
        @{ File = 'PackageIcon.png'; Size = 128 }
    )

    foreach ($packageAsset in $packageSquareAssets) {
        Save-TaskCroppedPng `
            -Source $masterLogo `
            -SourceCrop $taskEmblemCrop `
            -Width $packageAsset.Size `
            -Height $packageAsset.Size `
            -Destination (Join-Path $taskAppAssetsDirectory $packageAsset.File)
    }

    Save-TaskHorizontalLockup `
        -Source $masterLogo `
        -Width 620 `
        -Height 300 `
        -Destination (Join-Path $taskAppAssetsDirectory 'Wide310x150Logo.scale-200.png')
    Save-TaskHorizontalLockup `
        -Source $masterLogo `
        -Width 1240 `
        -Height 600 `
        -Destination (Join-Path $taskAppAssetsDirectory 'SplashScreen.scale-200.png')
    Save-TaskWindowsIcon `
        -Source $masterLogo `
        -SourceCrop $taskEmblemCrop `
        -Destination (Join-Path $taskAppAssetsDirectory 'TwelveRanks.ico')

    $storeSquareAssets = @(
        @{ File = 'TwelveRanks_AppIcon_71x71.png'; Size = 71 },
        @{ File = 'TwelveRanks_AppIcon_150x150.png'; Size = 150 },
        @{ File = 'TwelveRanks_AppIcon_300x300.png'; Size = 300 }
    )

    foreach ($storeAsset in $storeSquareAssets) {
        Save-TaskCroppedPng `
            -Source $masterLogo `
            -SourceCrop $taskEmblemCrop `
            -Width $storeAsset.Size `
            -Height $storeAsset.Size `
            -Destination (Join-Path $taskStoreAssetsDirectory $storeAsset.File)
    }

    Save-TaskFullPng `
        -Source $masterLogo `
        -Width 1080 `
        -Height 1080 `
        -Destination (Join-Path $taskStoreAssetsDirectory 'TwelveRanks_BoxArt_1080x1080.png')
}
finally {
    $masterLogo.Dispose()
}

Write-Output "Generated Twelve Ranks package and Store logo assets from $resolvedMasterLogoPath"
