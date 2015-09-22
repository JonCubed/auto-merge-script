// include Fake lib
#r @"./packages/FAKE/tools/FakeLib.dll"

open Fake
open System
open System.Text.RegularExpressions

let getBoolBuildParamOrDefault name defaultValue =
    bool.Parse(getBuildParamOrDefault name defaultValue)

let (=?) s1 s2 = 
    System.String.Equals(s1, s2, System.StringComparison.CurrentCultureIgnoreCase) 


let mutable sourceBranchName = null
let mutable repoDirectory = "."

////////////////////////////////
//// Read Command Line Params
///////////////////////////////
let remoteName = getBuildParamOrDefault "remote" "origin"
let validSourceRegex = getBuildParamOrDefault "validSource" "(hotfix|release)/.*"
let validDestinationRegex = getBuildParamOrDefault "destination" ("^"+remoteName+"/(master|develop)")
let customTag = getBuildParamOrDefault "tag" null
let useBranchNameAsTag = getBoolBuildParamOrDefault "tagWithBranch" "true"
let branchToTag = getBuildParamOrDefault "branchToTag" "master"
let deleteBranches = getBoolBuildParamOrDefault "delete" "true"

////////////////////////////////
//// Functions
///////////////////////////////
let IsValidSourceBranch name =
   let m = Regex.Match(name,validSourceRegex) 
   m.Success

let IsValidRemoteBranch name =
   let m = Regex.Match(name,validDestinationRegex) 
   m.Success
    
let BranchExists branches branch = 
    branches |> List.exists( (=) branch)

let LocalBranchExists repoDir branch = 
    BranchExists (Git.Branches.getLocalBranches repoDir) branch

let RemoteBranchExists repoDir branch = 
    BranchExists (Git.Branches.getRemoteBranches repoDir) branch

let GetLocalBranchFromRemote branch = 
    Regex.Replace(branch, @"^"+remoteName+"/(\s*)", "$1")

let GetTagFromBranchName branch =
    Regex.Replace(branch, @"^(?:\w*/)?(.*)$", "$1")    

let MergeInto sourceBranch remoteBranch =
    let localBranch = GetLocalBranchFromRemote remoteBranch
        
    // Check if remote branch exists
    if not (RemoteBranchExists repoDirectory remoteBranch) then failwith ("Remote branch " + remoteBranch + " does not exist" )

    if LocalBranchExists repoDirectory localBranch then  
        // branch already checked out so update it
        trace ("Switching to " + localBranch + " branch")
        Git.Branches.checkoutBranch repoDirectory localBranch
        trace "Pulling latest from tracking branch"
        Git.Branches.pull repoDirectory remoteName localBranch
    else      
        // branch not checked out yet
        trace ("Checking out remote branch " + remoteBranch + " to " + localBranch)
        Git.Branches.checkoutTracked repoDirectory remoteBranch localBranch

    // We are ready to merge
    let HeadBeforeMerge = Git.Information.getCurrentSHA1 repoDirectory
    trace ("Merging " + sourceBranch + " into " + localBranch)
    Git.Merge.merge repoDirectory Git.Merge.FastForwardFlag sourceBranch 

    // if conflicts reset branch and fail
    if Git.FileStatus.isInTheMiddleOfConflictedMerge repoDirectory then 
        Git.Reset.hard repoDirectory remoteBranch null
        failwith ("Failed to merge branch " + sourceBranch + " into " +  localBranch)

    // make sure there are changes to push
    let headAfterMerge = Git.Information.getCurrentSHA1 repoDirectory   
    if not (Git.Information.isAheadOf repoDirectory headAfterMerge HeadBeforeMerge) then
        trace "Info: Current branch is not ahead of remote"
    else
        trace ("Pushing changes to remote")
        Git.Branches.pushBranch repoDirectory remoteName localBranch

let TagCurrentBranchWith tag = 
    Git.Branches.tag repoDirectory tag
    Git.Branches.pushTag repoDirectory remoteName tag
    
////////////////////////////////
//// Targets
///////////////////////////////
Target "FindGitFolder" (fun _ ->
    let gitFolder = Git.CommandHelper.findGitDir repoDirectory
    let repoDirectory = gitFolder.FullName
    trace ("Git folder is " + repoDirectory)
)

Target "TestCurrentBranch" (fun _ ->
    sourceBranchName <- Git.Information.getBranchName repoDirectory
    trace ("Current branch is " + sourceBranchName)
        
    if not (IsValidSourceBranch sourceBranchName) then failwith ("Current branch " + sourceBranchName + " is invalid")       
)

Target "Merge" (fun _ ->    
    // get all branches that match the destination regex
    let remoteBranches = Git.Branches.getRemoteBranches repoDirectory |> List.where(IsValidRemoteBranch)

    for branch in remoteBranches do    
        let localBranch = GetLocalBranchFromRemote branch
        let tagThisBranch = (branchToTag =? localBranch)
        if tagThisBranch then
            trace "Tag this branch"

        MergeInto sourceBranchName branch 
    
        // tag branch with source branch name
        if tagThisBranch && useBranchNameAsTag then
            TagCurrentBranchWith (GetTagFromBranchName sourceBranchName)

        // tag branch with given tag (could be a build no.)
        if tagThisBranch && not (customTag = null) then
            TagCurrentBranchWith customTag
                        
)

Target "CloseBranch" (fun _ ->
    if not deleteBranches then
        trace "Closing of source branch skipped"
    else
        // close the current branch
        Git.Branches.deleteBranch repoDirectory true sourceBranchName
        Git.Branches.pushBranch repoDirectory remoteName (":"+sourceBranchName)
)

Target "GetBranches" (fun _ ->
    // list all remote branches
    List.iter( trace ) (Git.Branches.getRemoteBranches repoDirectory)
)

Target "Debug" (fun _ ->
    trace ("Current Directory = " + Environment.CurrentDirectory)
    trace ("sourceBranchName = " + sourceBranchName)
    trace ("repoDirectory = " + repoDirectory)
    trace ("remoteName [remote] = " + remoteName)
    trace ("validSourceRegex [validSource] = " + validSourceRegex)
    trace ("customTag [tag] = " + customTag)
    trace ("useBranchAsTag [tagWithBranch] = " + useBranchNameAsTag.ToString())
    trace ("deleteBranches [delete] = " + deleteBranches.ToString())
    trace ("branchToTag [branchToTag] = " + branchToTag)
    trace ("destinationRegex [destination] = " + validDestinationRegex)
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

