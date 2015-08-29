$ErrorActionPreference = "Stop"

cls

$pluginFolder = ".\plugin"
$packingFolder = ".\.artifacts"
$agentFolder = Join-Path $packingFolder "agent"

# Create .package folder
if (-not (Test-Path $packingFolder))
{
    Write-Output "Creating $packingFolder"
    New-Item $packingFolder -ItemType Directory
}
else
{
    Write-Output "Cleaning $packingFolder"
    Remove-Item $packingFolder\* -Recurse -Force
}

Write-Output "Copying files to $packingFolder"
Copy-Item $pluginFolder\* $packingFolder -Recurse
Copy-Item ..\src\AutoMerge\AutoMerge.ps1,..\src\AutoMerge\AutoMerge.fsx $agentFolder\tools


Write-Output "Creating plugin zip in $packingFolder"
Compress-Archive -Path $agentFolder\* -DestinationPath $agentFolder\GitAutoMerge.zip
Remove-Item $agentFolder\* -Exclude GitAutoMerge.zip -Recurse -Force

Compress-Archive -Path $packingFolder\* -DestinationPath $packingFolder\GitAutoMerge-plugin.zip
Remove-Item $packingFolder\* -Recurse -Exclude GitAutoMerge-plugin.zip -Force


Write-Output "Build complete"