[CmdletBinding()]
param([string]$ModRoot = (Join-Path $PSScriptRoot '..\..'))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

#将现成内衬与顶盖按原画布合成建造图标和蓝图，不改变游戏中的分层贴图。
function Build-Preview {
    param([string]$Directory, [string]$Stem, [string]$Facing)

    $textureDirectory = Join-Path $ModRoot "Textures\Building\MihoPhase2\$Directory"
    $interior = [System.Drawing.Bitmap]::new((Join-Path $textureDirectory "${Stem}Interior_${Facing}.png"))
    $cover = [System.Drawing.Bitmap]::new((Join-Path $textureDirectory "${Stem}Cover_${Facing}.png"))
    try {
        if ($interior.Size -ne $cover.Size) { throw "内衬和顶盖的画布尺寸不同：$Stem" }
        $preview = [System.Drawing.Bitmap]::new($cover.Width, $cover.Height)
        $graphics = [System.Drawing.Graphics]::FromImage($preview)
        try {
            $rectangle = [System.Drawing.Rectangle]::new(0, 0, $cover.Width, $cover.Height)
            $graphics.DrawImage($interior, $rectangle, 0, 0, $interior.Width, $interior.Height, [System.Drawing.GraphicsUnit]::Pixel)
            $graphics.DrawImage($cover, $rectangle, 0, 0, $cover.Width, $cover.Height, [System.Drawing.GraphicsUnit]::Pixel)
            $target = Join-Path $textureDirectory "${Stem}Complete_${Facing}.png"
            $preview.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
            Write-Output "已合成：$target"
        }
        finally {
            $graphics.Dispose()
            $preview.Dispose()
        }
    }
    finally {
        $interior.Dispose()
        $cover.Dispose()
    }
}

Build-Preview 'MedicalPod' 'MihoPhase2_SupernovaMedicalPod' 'south'
Build-Preview 'IndoorCultivator' 'MihoPhase2_IndoorCultivator' 'east'
