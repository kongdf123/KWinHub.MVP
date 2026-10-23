# Sets up the PortableBuildTools MSVC environment (PATH / INCLUDE / LIB) for
# the current process, then runs `npm run build` (tauri build) in WebShell.
# Needed because PortableBuildTools was installed with env=none (sandbox-safe).

$ErrorActionPreference = 'Stop'
$pbtRoot = 'D:\PBTBuild'

$msvc = Get-ChildItem "$pbtRoot\VC\Tools\MSVC" -Directory | Sort-Object Name -Descending | Select-Object -First 1
if (-not $msvc) { throw "MSVC not found under $pbtRoot\VC\Tools\MSVC" }

$sdkInclude = Get-ChildItem "$pbtRoot\Windows Kits\10\include" -Directory | Sort-Object Name -Descending | Select-Object -First 1
$sdkLib = Get-ChildItem "$pbtRoot\Windows Kits\10\lib" -Directory | Sort-Object Name -Descending | Select-Object -First 1
if (-not $sdkInclude -or -not $sdkLib) { throw "Windows SDK not found under $pbtRoot\Windows Kits\10" }

$msvcBin = "$($msvc.FullName)\bin\Hostx64\x64"
$sdkBin = Get-ChildItem "$pbtRoot\Windows Kits\10\bin" -Directory | Sort-Object Name -Descending | Select-Object -First 1

$env:PATH = "$msvcBin;$($sdkBin.FullName)\x64;$env:PATH"
$env:INCLUDE = @(
  "$($msvc.FullName)\include",
  "$($sdkInclude.FullName)\ucrt",
  "$($sdkInclude.FullName)\um",
  "$($sdkInclude.FullName)\shared",
  "$($sdkInclude.FullName)\winrt"
) -join ';'
$env:LIB = @(
  "$($msvc.FullName)\lib\x64",
  "$($sdkLib.FullName)\ucrt\x64",
  "$($sdkLib.FullName)\um\x64"
) -join ';'

Write-Host "MSVC:   $($msvc.Name)"
Write-Host "SDK:    $($sdkInclude.Name)"
Write-Host "link:   $msvcBin\link.exe"

$cargoBin = "$env:USERPROFILE\.cargo\bin"
$env:PATH = "$cargoBin;$env:PATH"

Set-Location $PSScriptRoot\..
npm run build
exit $LASTEXITCODE
