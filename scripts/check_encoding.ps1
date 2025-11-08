<#
.SYNOPSIS
  Scans text files for common mojibake sequences (繧/縺/蜑/...).

.DESCRIPTION
  Some editors / shell commands still default to Shift-JIS on Japanese
  Windows, which can lead to "繧ｪ縺｡" style strings being written back
  into UTF-8 files. This script walks the repository, looks for risky
  extensions, and flags the first offending line so the change can be
  reverted before commit.

.PARAMETER Path
  Root paths to scan. Defaults to Docs/ and Mods/QudJP/.

.PARAMETER Extensions
  File extensions (with leading dot) to include in the scan.

.PARAMETER FailOnIssues
  When provided, the script exits with code 1 if issues are found.
  (Useful in CI or a pre-commit hook.)
#>
param(
  [string[]]$Path = @('Docs', 'Mods/QudJP'),
  [string[]]$Extensions = @('.md', '.xml', '.txt', '.csv'),
  [switch]$FailOnIssues
)

$badPattern = '[繧縺蜑菴譛螳鬘讖蝨驛遘莨蟾驕髢遉鬮遯霑霆鬟閧閻]'
$issues = @()

foreach ($root in $Path) {
  if (-not (Test-Path $root)) {
    Write-Warning "Skipping missing path: $root"
    continue
  }

  Get-ChildItem -Path $root -Recurse -File |
    Where-Object { $Extensions -contains $_.Extension.ToLowerInvariant() } |
    ForEach-Object {
      $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
      if ($bytes.Length -eq 0) { return }

      $text = [System.Text.Encoding]::UTF8.GetString($bytes)
      if ($text -notmatch $badPattern) { return }

      $line = ($text -split "`r?`n" | Where-Object { $_ -match $badPattern } | Select-Object -First 1).Trim()
      $issues += [pscustomobject]@{
        File    = $_.FullName
        Snippet = $line.Substring(0, [Math]::Min($line.Length, 120))
      }
    }
}

if ($issues.Count -eq 0) {
  Write-Host "✅ No mojibake-style sequences detected."
  exit 0
}

Write-Host "⚠️  Detected $($issues.Count) file(s) with suspicious sequences:`n"
$issues | Format-Table -AutoSize

if ($FailOnIssues) {
  Write-Error "Mojibake candidate(s) detected."
  exit 1
}
