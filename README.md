# Auto-Merge Script

F# FAKE script for merging git repos for use in automation. By default setup for GitFlow.

# Usage

You can run the script from the command prompt by passing AutoMerge.fsx to Fake.exe 
with any of the command line parameters below. It expects FAKE to be at *./packages/FAKE*

```PowerShell
#PowerShell
. ./packages/FAKE/tools/Fake.exe AutoMerge.fsx
```

A PowerShell script has been provided that will first download FAKE to the packages folder before 
executing the script. It requires nuget.
 
```PowerShell
#PowerShell
. .\AutoMerge.ps1
```

## Commandline parameters

| Parameter     | Default                  | Description |
| ------------- | ------------------------ | ----------- |
| nugetExe      | .\\.nuget\\NuGet.exe     | path to NuGet exe (only for PowerShell) |
| remote        | origin                   | name of the remote for the repo |
| validSource   | (hotfix\|release)/.*      | regex pattern for what a valid local source branch is for merging |
| destination   | ^origin/(master\|develop) | regex pattern for find which remote branched the source branch should be merged into |
| tag           |                          | a tag value to tag a destination branch with |
| tagWithBranch | true                     | uses the source branch name as a tag value |
| branchToTag   | master                   | local destination branch that should be tagged |
| delete        | true                     | delete the source branch locally and remotely once the merge is compelete |
