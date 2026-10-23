# Renders a 1024x1024 KWinHub app icon (dark rounded square, teal KW monogram)
# using GDI+, then the Tauri CLI generates all platform icons from it.
Add-Type -AssemblyName System.Drawing

$size = 1024
$bmp = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.TextRenderingHint = 'AntiAliasGridFit'
$g.Clear([System.Drawing.Color]::FromArgb(15, 23, 42))   # slate-900

# Teal accent bar across the bottom
$accent = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(45, 212, 191))
$g.FillRectangle($accent, 0, ($size - 96), $size, 96)

# KW monogram
$font = New-Object System.Drawing.Font('Segoe UI', 380, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$textBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(226, 232, 240))
$format = New-Object System.Drawing.StringFormat
$format.Alignment = 'Center'
$format.LineAlignment = 'Center'
$rect = New-Object System.Drawing.RectangleF(0, -40, $size, $size)
$g.DrawString('KW', $font, $textBrush, $rect, $format)

$out = Join-Path $PSScriptRoot 'icon-source.png'
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Host "Wrote $out"
