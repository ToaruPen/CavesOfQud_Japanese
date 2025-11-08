param(
    [string]$GamePath = "C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud",
    [string]$OutputPath = "$PSScriptRoot\..\references\Base"
)

$ErrorActionPreference = "Stop"

function Resolve-PathSafe {
    param([string]$Base, [string]$Relative)
    return Join-Path -Path $Base -ChildPath $Relative
}

function Copy-WithStructure {
    param([string]$Source, [string]$Destination)
    $destDir = Split-Path -Parent $Destination
    if (!(Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir | Out-Null
    }
    Copy-Item -Path $Source -Destination $Destination -Recurse -Force
}

Write-Host "GamePath   : $GamePath"
Write-Host "OutputPath : $OutputPath"

$baseRoot = Resolve-PathSafe -Base $GamePath -Relative "CoQ_Data\StreamingAssets\Base"
$targets = @(
    "Conversations.xml",
    "Books.xml",
    "Commands.xml",
    "EmbarkModules.xml",
    "Corpus",
    "ObjectBlueprints"
)

foreach ($target in $targets) {
    $source = Resolve-PathSafe -Base $baseRoot -Relative $target
    if (!(Test-Path $source)) {
        Write-Warning "Missing $source"
        continue
    }
    $destination = Resolve-PathSafe -Base (Resolve-Path $OutputPath) -Relative $target
    Write-Host "Copying $target"
    Copy-WithStructure -Source $source -Destination $destination
}

Write-Host "Base extraction completed."
