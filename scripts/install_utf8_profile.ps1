<#
.SYNOPSIS
  Adds UTF-8 safety bootstrap to the user's PowerShell profile.

.DESCRIPTION
  This script appends (or updates) a short block in the current user's
  PowerShell profile so that every new console automatically runs
  `scripts/ensure_utf8.ps1` from this repository. That guarantees the
  console code page, input/output encodings, and related env vars are
  locked to UTF-8 before any translation commands run.

.PARAMETER RepoPath
  Path to the root of CavesOfQud_Japanese. Defaults to the script's
  parent directory.

.PARAMETER Remove
  Removes the previously inserted bootstrap block.
#>

[CmdletBinding(SupportsShouldProcess=$true)]
param(
  [string]$RepoPath = (Split-Path -Parent $PSScriptRoot),
  [switch]$Remove
)

$profilePath = $PROFILE.CurrentUserCurrentHost
$ensurePath  = Join-Path $RepoPath 'scripts/ensure_utf8.ps1'
$tagBegin    = '# BEGIN CavesOfQud_Japanese UTF8 Bootstrap'
$tagEnd      = '# END CavesOfQud_Japanese UTF8 Bootstrap'

if (-not (Test-Path $profilePath)) {
  New-Item -Path (Split-Path $profilePath) -ItemType Directory -Force | Out-Null
  New-Item -Path $profilePath -ItemType File -Force | Out-Null
}

$profileContent = Get-Content -Path $profilePath -Raw
$blockPattern = [regex]::Escape($tagBegin) + '.*?' + [regex]::Escape($tagEnd)

if ($Remove) {
  if ($profileContent -match $blockPattern) {
    $updated = [regex]::Replace($profileContent, $blockPattern, '', 'Singleline')
    Set-Content -Path $profilePath -Value $updated -Encoding utf8
    Write-Host "Removed UTF-8 bootstrap block from profile: $profilePath" -ForegroundColor Yellow
  }
  else {
    Write-Host "No UTF-8 bootstrap block found in profile." -ForegroundColor Yellow
  }
  return
}

if (-not (Test-Path $ensurePath)) {
  throw "Could not find ensure_utf8.ps1 at '$ensurePath'. Please pass -RepoPath if the repo is elsewhere."
}

$ensureFull = (Resolve-Path $ensurePath).ProviderPath
$bootstrap = @"
$tagBegin
if (Test-Path '$ensureFull') {
    & '$ensureFull'
}
$tagEnd
"@

if ($profileContent -match $blockPattern) {
  $updated = [regex]::Replace($profileContent, $blockPattern, $bootstrap, 'Singleline')
  Set-Content -Path $profilePath -Value $updated -Encoding utf8
  Write-Host "Updated existing UTF-8 bootstrap block in profile: $profilePath" -ForegroundColor Green
} else {
  Add-Content -Path $profilePath -Value "`r`n$bootstrap" -Encoding utf8
  Write-Host "Added UTF-8 bootstrap block to profile: $profilePath" -ForegroundColor Green
}
