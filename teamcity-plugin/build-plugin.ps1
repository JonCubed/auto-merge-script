$ErrorActionPreference = "Stop"

cls

$pluginFolder = ".\plugin"
$packingFolder = ".\.artifacts"
$agentFolder = Join-Path $packingFolder "agent"
$toolsFolder = Join-Path $agentFolder "tools"
$toolsSourceFolder = "..\src\AutoMerge"
$agentZipName = "GitAutoMerge.zip"
$pluginZipName = "GitAutoMerge-plugin.zip"

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
Copy-Item (Join-Path $toolsSourceFolder AutoMerge.ps1),(Join-Path $toolsSourceFolder AutoMerge.fsx) $toolsFolder


Write-Output "Creating plugin zip in $packingFolder"
Compress-Archive -Path $agentFolder\* -DestinationPath (Join-Path $agentFolder $agentZipName)
Remove-Item $agentFolder\* -Exclude $agentZipName -Recurse -Force

Compress-Archive -Path $packingFolder\* -DestinationPath (Join-Path $packingFolder $pluginZipName)
Remove-Item $packingFolder\* -Recurse -Exclude $pluginZipName -Force


Write-Output "Build complete"