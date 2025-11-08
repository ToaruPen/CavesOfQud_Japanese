param(
    [string]$BasePath = "$PSScriptRoot\..\references\Base",
    [string]$LocalizationPath = "$PSScriptRoot\..\Mods\QudJP\Localization"
)

$ErrorActionPreference = "Stop"

function Get-LocalizedPath {
    param([IO.FileInfo]$BaseFile)
    $dir = Split-Path -Path $BaseFile.FullName -Parent
    $relative = $dir.Substring($BasePath.Length).TrimStart("\")
    $baseName = $BaseFile.BaseName
    $ext = $BaseFile.Extension
    $localizedName = "{0}.jp{1}" -f $baseName, $ext
    $targetDir = if ($relative) { Join-Path $LocalizationPath $relative } else { $LocalizationPath }
    return Join-Path $targetDir $localizedName
}

if (!(Test-Path $BasePath)) {
    throw "BasePath not found: $BasePath"
}
if (!(Test-Path $LocalizationPath)) {
    throw "LocalizationPath not found: $LocalizationPath"
}

$baseFiles = Get-ChildItem -Path $BasePath -Recurse -Include *.xml,*.txt
$report = @()

foreach ($file in $baseFiles) {
    $localized = Get-LocalizedPath -BaseFile $file
    $status = if (Test-Path $localized) { "localized" } else { "missing" }
    $report += [PSCustomObject]@{
        BaseFile    = $file.FullName.Substring($BasePath.Length + 1)
        Localized   = $localized.Substring($LocalizationPath.Length + 1)
        Status      = $status
    }
}

$report | Sort-Object Status, BaseFile | Format-Table -AutoSize

Write-Host ""
Write-Host "missing: " ($report | Where-Object Status -eq "missing").Count
Write-Host "localized: " ($report | Where-Object Status -eq "localized").Count
