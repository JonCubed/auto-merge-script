param (
    [string]$nugetExe = ".\.nuget\NuGet.exe",
    [string]$remote,
    [string]$validSource,
    [string]$destination,
    [string]$tag,
    [string]$tagWithBranch,
    [string]$branchToTag,
    [string]$delete
)

$params = @()

if ($remote) {
     $params += "remote=`"$remote`""
}

if ($validSource) {
     $params += "validSource=`"$validSource`""
}

if ($destination) {
     $params += "destination=`"$destination`""
}

if ($tag) {
     $params += "tag=`"$tag`""
}

if ($tagWithBranch) {
     $params += "tagWithBranch=`"$tagWithBranch`""
}

if ($branchToTag) {
     $params += "branchToTag=`"$branchToTag`""
}

if ($delete) {
     $params += "delete=`"$delete`""
}

Write-Output "Command line parameters passed in:"
$params | Out-String | Write-Output


$mergeScriptPath = Join-Path $PSScriptRoot AutoMerge.fsx
$location = Get-Location
if (-not($PSScriptRoot -eq $location))
{
    Write-Output "Copying script to current location ($location)"
    Copy-Item $mergeScriptPath .
}

. $nugetExe Install "FAKE" -OutputDirectory "packages" -ExcludeVersion
Invoke-Expression ".\packages\FAKE\tools\Fake.exe AutoMerge.fsx $params"

