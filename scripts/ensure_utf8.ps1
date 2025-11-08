<#
.SYNOPSIS
  Ensures the current PowerShell session uses UTF-8 consistently.

.DESCRIPTION
  Windows PowerShell 5.x still defaults to code page 932 on Japanese
  systems, which makes UTF-8 files *look* broken and can reintroduce
  mojibake when saving. Run this script (preferably at the start of
  every session or via your profile) to flip the console, Get-Content,
  and Set-Content defaults to UTF-8. Optionally execute a script block
  once the environment is configured.

.PARAMETER Command
  Optional script block to run after UTF-8 settings are applied.
#>

[CmdletBinding()]
param(
  [ScriptBlock]$Command
)

$utf8 = [System.Text.Encoding]::UTF8

if ($PSVersionTable.PSEdition -eq 'Desktop') {
  # Windows PowerShell (5.x) still obeys the active code page.
  chcp 65001 > $null
}

# PowerShell Core defaults to UTF-8 already, but we set both to be explicit.
[Console]::OutputEncoding = $utf8
[Console]::InputEncoding = $utf8

# Encourage downstream tools (dotnet, git) to stay in UTF-8 land.
$env:DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = '0'
$env:LC_ALL = 'en_US.UTF-8'

Write-Host "UTF-8 console configured (Input/OutputEncoding set to UTF-8)." -ForegroundColor Green

if ($Command) {
  & $Command
}
