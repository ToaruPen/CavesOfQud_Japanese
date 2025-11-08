<#
.SYNOPSIS
  Mirrors the local Mods/QudJP folder into the live Mod instance used by the game client.

.DESCRIPTION
  This script copies the translated assets from the working repository into the
  player's Mods directory under %USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Mods\QudJP.
  It wraps robocopy with the switches we use in the workflow so that we only sync when we explicitly call it.

.PARAMETER SourcePath
  Optional path to the source Mod folder. Defaults to ..\Mods\QudJP relative to this script.

.PARAMETER TargetPath
  Optional destination path. Defaults to the QudJP folder under the LocalLow Mods directory.

.PARAMETER WhatIf
  Adds /L to robocopy so you can preview the copy without modifying files.

.PARAMETER ExcludeFonts
  Skip copying the Fonts directory (useful if the runtime already generated them).

.EXAMPLE
  ./sync_mod.ps1
  Mirrors the repository Mod into the live Mods folder.

.EXAMPLE
  ./sync_mod.ps1 -WhatIf
  Shows what would be copied without touching any files.
#>
[CmdletBinding()]
param(
    [string]$SourcePath = (Resolve-Path -Path (Join-Path $PSScriptRoot "..\Mods\QudJP")),
    [string]$TargetPath = (Join-Path $env:USERPROFILE "AppData\LocalLow\Freehold Games\CavesOfQud\Mods\QudJP"),
    [switch]$WhatIf,
    [switch]$ExcludeFonts
)

if (-not (Test-Path -Path $SourcePath)) {
    throw "Source path '$SourcePath' does not exist. Run this script from within the repository."
}

if (-not (Test-Path -Path $TargetPath)) {
    Write-Verbose "Target '$TargetPath' does not exist. Creating it."
    New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
}

$excludeDirs = @("obj", ".git", ".vs", "bin")
if ($ExcludeFonts) {
    $excludeDirs += "Fonts"
}

$robocopyArgs = @(
    "`"$SourcePath`"",
    "`"$TargetPath`"",
    "/MIR",
    "/R:2",         # retry twice on locked files
    "/W:2",         # wait 2 seconds between retries
    "/FFT",         # tolerate FAT/NTFS timestamp differences
    "/NFL",         # no file list (keeps output shorter)
    "/NDL"          # no directory list
)

foreach ($dir in $excludeDirs) {
    $robocopyArgs += "/XD"
    $robocopyArgs += "`"$dir`""
}

if ($WhatIf) {
    $robocopyArgs += "/L"
    $robocopyArgs += "/NJH"
    $robocopyArgs += "/NJS"
    Write-Host "Running in WhatIf (list only) mode..."
}

Write-Host "Syncing:" -ForegroundColor Cyan
Write-Host "  Source : $SourcePath"
Write-Host "  Target : $TargetPath"

& robocopy @robocopyArgs | Write-Host
$exitCode = $LASTEXITCODE

if ($exitCode -ge 8) {
    throw "robocopy reported a failure (exit code $exitCode)."
} else {
    Write-Host "Sync completed with robocopy exit code $exitCode."
}
