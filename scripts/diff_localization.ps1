param(
    [string]$BasePath = "$PSScriptRoot\..\references\Base",
    [string]$LocalizationPath = "$PSScriptRoot\..\Mods\QudJP\Localization",
    [switch]$MissingOnly,
    [string]$JsonPath
)

$ErrorActionPreference = "Stop"

function Get-RelativePath {
    param(
        [string]$Base,
        [string]$Target
    )

    $baseFull = [System.IO.Path]::GetFullPath($Base).TrimEnd("\", "/")
    $targetFull = [System.IO.Path]::GetFullPath($Target)

    $baseUri = New-Object System.Uri($baseFull + [System.IO.Path]::DirectorySeparatorChar)
    $targetUri = New-Object System.Uri($targetFull)

    if (-not $baseUri.IsBaseOf($targetUri)) {
        return ""
    }

    $relative = $baseUri.MakeRelativeUri($targetUri).ToString()
    return [System.Uri]::UnescapeDataString($relative).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

function Get-LocalizedPath {
    param([IO.FileInfo]$BaseFile)
    $dir = Split-Path -Path $BaseFile.FullName -Parent
    $relative = Get-RelativePath -Base $BasePath -Target $dir
    $baseName = $BaseFile.BaseName
    $ext = $BaseFile.Extension
    $localizedName = "{0}.jp{1}" -f $baseName, $ext
    $targetDir = if ($relative) { Join-Path $LocalizationPath $relative } else { $LocalizationPath }
    return Join-Path $targetDir $localizedName
}

function Get-ObjectNames {
    param([string]$Path)
    try {
        $raw = Get-Content -Path $Path -Raw -ErrorAction Stop
        $xml = New-Object System.Xml.XmlDocument
        $xml.PreserveWhitespace = $true
        $xml.LoadXml($raw)
    }
    catch {
        Write-Warning "Failed to parse XML: $Path ($_)"
        return @()
    }

    $nodes = $xml.SelectNodes("//object[@Name]")
    if ($null -eq $nodes) {
        return @()
    }

    $names = @()
    foreach ($node in $nodes) {
        $attribute = $node.Attributes["Name"]
        if ($attribute -and -not [string]::IsNullOrWhiteSpace($attribute.Value)) {
            $names += $attribute.Value
        }
    }

    return $names
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
    $relativeBase = Get-RelativePath -Base $BasePath -Target $file.FullName
    $relativeLocalized = Get-RelativePath -Base $LocalizationPath -Target $localized

    if (!(Test-Path $localized)) {
        $report += [PSCustomObject]@{
            BaseFile   = $relativeBase
            Localized  = $relativeLocalized
            ObjectName = $null
            Status     = "file-missing"
        }
        continue
    }

    if ($file.Extension -ne ".xml") {
        $report += [PSCustomObject]@{
            BaseFile   = $relativeBase
            Localized  = $relativeLocalized
            ObjectName = $null
            Status     = "ok"
        }
        continue
    }

    $baseObjectNames = Get-ObjectNames -Path $file.FullName
    if ($baseObjectNames.Count -eq 0) {
        $report += [PSCustomObject]@{
            BaseFile   = $relativeBase
            Localized  = $relativeLocalized
            ObjectName = $null
            Status     = "ok"
        }
        continue
    }

    $localizedObjectNames = Get-ObjectNames -Path $localized
    $localizedSet = New-Object System.Collections.Generic.HashSet[string]([StringComparer]::OrdinalIgnoreCase)
    foreach ($name in $localizedObjectNames) {
        [void]$localizedSet.Add($name)
    }

    $missingObjects = @()
    foreach ($name in $baseObjectNames) {
        if (-not $localizedSet.Contains($name)) {
            $missingObjects += $name
        }
    }

    if ($missingObjects.Count -eq 0) {
        $report += [PSCustomObject]@{
            BaseFile   = $relativeBase
            Localized  = $relativeLocalized
            ObjectName = $null
            Status     = "ok"
        }
    }
    else {
        foreach ($name in $missingObjects) {
            $report += [PSCustomObject]@{
                BaseFile   = $relativeBase
                Localized  = $relativeLocalized
                ObjectName = $name
                Status     = "object-missing"
            }
        }
    }
}

$filtered = if ($MissingOnly) { $report | Where-Object { $_.Status -ne "ok" } } else { $report }

if ($filtered.Count -gt 0) {
    $filtered | Sort-Object Status, BaseFile, ObjectName | Format-Table -AutoSize
}
else {
    Write-Host "No entries."
}

Write-Host ""
foreach ($group in ($report | Group-Object Status | Sort-Object Name)) {
    Write-Host ("{0,-16}: {1,4}" -f $group.Name, $group.Count)
}

if ($JsonPath) {
    $json = $report | ConvertTo-Json -Depth 4
    $utf8 = New-Object System.Text.UTF8Encoding($false)
    $resolved = [System.IO.Path]::GetFullPath($JsonPath)
    [System.IO.File]::WriteAllText($resolved, $json, $utf8)
    Write-Host ""
    Write-Host "Report written to $resolved"
}
