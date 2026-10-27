# Sets up the MSVC build environment (PATH / INCLUDE / LIB) for the current
# process, then runs `npm.cmd run build` (tauri build) in WebShell.
#
# Fully portable: auto-detects Visual Studio / Build Tools via vswhere and the
# Windows SDK via the registry (Microsoft's canonical Installed Roots key) plus
# standard well-known paths. Works on any Windows machine with "Desktop dev w/
# C++" workload installed — Community, Professional, Enterprise, or Build Tools,
# installed on any drive, SDK on any drive.
#
# Why MSVC instead of GNU?
#   MinGW's ld.exe / dlltool.exe fail on CJK / non-ASCII paths
#   (Permission denied). MSVC's link.exe / rc.exe handle Unicode natively. No
#   path rewiring, junctions, or GNU copies are required even with a CJK user
#   name. MSVC is also Rust's default Windows toolchain.

# --- 1. Locate Visual Studio via vswhere (standard location on every Windows) ---

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path -LiteralPath $vswhere)) { throw "vswhere not found at $vswhere" }

$vsInstall = & $vswhere -latest -property installationPath
if (-not $vsInstall) { throw "No Visual Studio / Build Tools installation found via vswhere" }

$msvc = Get-ChildItem "$vsInstall\VC\Tools\MSVC" -Directory -ErrorAction Stop | Sort-Object Name -Descending | Select-Object -First 1
if (-not $msvc) { throw "MSVC toolchain not found under $vsInstall\VC\Tools\MSVC" }

# --- 2. Locate Windows SDK: registry first, then standard paths ---

$SDK_ROOT = $null

# 2a. Registry — Microsoft's canonical location for SDK install path
#     HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows Kits\Installed Roots
#     Always present if any Windows SDK is installed via the official installer.
$regPaths = @(
  'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows Kits\Installed Roots',
  'HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots'
)
foreach ($regPath in $regPaths) {
  if (Test-Path -LiteralPath $regPath) {
    $kitRoot = (Get-ItemProperty $regPath -ErrorAction SilentlyContinue).KitsRoot10
    if ($kitRoot -and (Test-Path -LiteralPath "$kitRoot\10")) {
      $SDK_ROOT = "$kitRoot\10"
      break
    }
  }
}

# 2b. Standard well-known paths (fallback for SDKs not registered)
if (-not $SDK_ROOT) {
  $wellKnown = @(
    "${env:ProgramFiles(x86)}\Windows Kits\10",
    "${env:ProgramFiles}\Windows Kits\10"
  )
  foreach ($p in $wellKnown) {
    if ($p -and (Test-Path -LiteralPath $p)) {
      $SDK_ROOT = $p
      break
    }
  }
}

# 2c. Scan all local drives for "Windows Kits\10" (last resort)
if (-not $SDK_ROOT) {
  foreach ($drive in Get-PSDrive -PSProvider FileSystem) {
    $candidate = Join-Path $drive.Root 'Windows Kits\10'
    if (Test-Path -LiteralPath $candidate) {
      $SDK_ROOT = $candidate
      break
    }
  }
}

if (-not $SDK_ROOT) { throw "Windows SDK not found. Checked registry (KitsRoot10), ${env:ProgramFiles(x86)}\Windows Kits\10, ${env:ProgramFiles}\Windows Kits\10, and all local drives." }

$sdkInclude = Get-ChildItem "$SDK_ROOT\include" -Directory | Sort-Object Name -Descending | Select-Object -First 1
$sdkLib     = Get-ChildItem "$SDK_ROOT\lib"     -Directory | Sort-Object Name -Descending | Select-Object -First 1
$sdkBin     = Get-ChildItem "$SDK_ROOT\bin"     -Directory | Sort-Object Name -Descending | Select-Object -First 1
if (-not $sdkInclude -or -not $sdkLib -or -not $sdkBin) { throw "Incomplete Windows SDK under $SDK_ROOT" }

# --- 3. Assemble PATH / INCLUDE / LIB for this process ---

$msvcBin = "$($msvc.FullName)\bin\Hostx64\x64"

$env:PATH = "$msvcBin;$($sdkBin.FullName)\x64;$env:USERPROFILE\.cargo\bin;$env:PATH"
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

Write-Host "VS      : $vsInstall"
Write-Host "MSVC    : $($msvc.Name)"
Write-Host "SDK     : $($sdkInclude.Name)  ($SDK_ROOT)"
Write-Host "link    : $msvcBin\link.exe"
Write-Host "rc      : $($sdkBin.FullName)\x64\rc.exe"
Write-Host "host    : $(rustc --print host-tuple 2>$null)"
Write-Host ""

# Run npm.cmd directly. PowerShell maps native stderr to error records
# only when $ErrorActionPreference=Stop; we avoid that global setting so
# benign npm/cargo warnings ("caniuse-lite outdated", dep warnings, etc.)
# do not terminate the script. We trust $LASTEXITCODE for real failures.
Set-Location $PSScriptRoot\..
& npm.cmd run build:frontends
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& npm.cmd run build
exit $LASTEXITCODE