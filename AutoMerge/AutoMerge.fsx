// include Fake lib
#r @"../packages/FAKE.4.0.3/tools/FakeLib.dll"

open Fake
open System.Text.RegularExpressions

let mutable sourceBranchName = null
let remoteName = "origin"

////////////////////////////////
//// Functions
///////////////////////////////
let IsValidSourceBranch name =
   let m = Regex.Match(name,@"(hotfix|release)/.*") 
   m.Success

let IsReleaseBranch name =
   let m = Regex.Match(name,@"release/.*") 
   m.Success
    
let BranchExists branches branch = 
    branches |> List.exists( (=) branch)

let LocalBranchExists repoDir branch = 
    BranchExists (Git.Branches.getLocalBranches repoDir) branch

let RemoteBranchExists repoDir branch = 
    BranchExists (Git.Branches.getRemoteBranches repoDir) (remoteName+"/"+branch)

let MergeInto sourceBranch destinationBranch =
    let remoteBranch = remoteName + "/" + destinationBranch

    // Check if remote branch exists
    if not (RemoteBranchExists "." destinationBranch) then failwith ("Remote branch " + remoteBranch + " does not exist" )

    if LocalBranchExists "." destinationBranch then  
        // branch already checked out so update it
        trace ("Switching to " + destinationBranch + " branch")
        Git.Branches.checkoutBranch "." destinationBranch
        trace "Pulling latest from tracking branch"
        Git.Branches.pull "." remoteName destinationBranch
    else      
        // branch not checked out yet
        trace ("Checking out remote branch " + remoteBranch + " to " + destinationBranch)
        Git.Branches.checkoutTracked "." remoteBranch destinationBranch

    // We are ready to merge
    let HeadBeforeMerge = Git.Information.getCurrentSHA1 "."
    trace ("Merging " + sourceBranch + " into " + destinationBranch)
    Git.Merge.merge "." Git.Merge.FastForwardFlag sourceBranch 

    // if conflicts reset branch and fail
    if Git.FileStatus.isInTheMiddleOfConflictedMerge "." then 
        Git.Reset.hard "." remoteBranch null
        failwith ("Failed to merge branch " + sourceBranch + " into " +  destinationBranch)

    // make sure there are changes to push
    let headAfterMerge = Git.Information.getCurrentSHA1 "."    
    if not (Git.Information.isAheadOf "." headAfterMerge HeadBeforeMerge) then
        trace "Info: Current branch is not ahead of remote"
    else
        trace ("Pushing changes to remote")
        Git.Branches.pushBranch "." remoteName destinationBranch
    
////////////////////////////////
//// Targets
///////////////////////////////
Target "FindGitFolder" (fun _ ->
    let gitFolder = Git.CommandHelper.findGitDir "."
    trace ("Git folder is " + gitFolder.FullName)
)

Target "TestCurrentBranch" (fun _ ->
    sourceBranchName <- Git.Information.getBranchName "."
    trace ("Current branch is " + sourceBranchName)
        
    if not (IsValidSourceBranch sourceBranchName) then failwith ("Current branch " + sourceBranchName + " is invalid")       
)

Target "Merge" (fun _ ->    
    // merge current to master
    MergeInto sourceBranchName "master"

    // merge current into develop
    MergeInto sourceBranchName "develop"
)

Target "CloseBranch" (fun _ ->
    // close the current branch
    Git.Branches.deleteBranch "." true sourceBranchName
    Git.Branches.pushBranch "." remoteName (":"+sourceBranchName)
)

Target "GetBranches" (fun _ ->
    // close the current branch
    List.iter( trace ) (Git.Branches.getRemoteBranches ".")
)

////////////////////////////////
//// Dependencies
///////////////////////////////
"FindGitFolder"
    ==>"TestCurrentBranch"
    ==> "Merge"
    ==> "CloseBranch"

// start build
RunTargetOrDefault "CloseBranch"

