// SPDX-License-Identifier: MPL-2.0

using LibGit2Sharp;

namespace Holo.Services;

/// <summary>
/// Basic service for interacting with Git
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Set the current working directory
    /// </summary>
    /// <param name="path">Directory path</param>
    /// <remarks>Probably the <see cref="Solution"/> directory</remarks>
    void SetWorkingDirectory(Uri path);

    /// <summary>
    /// Check if the working directory is a Git repository
    /// </summary>
    /// <returns></returns>
    bool IsRepository();

    /// <summary>
    /// Initialize a new Git repository
    /// </summary>
    void InitializeRepository();

    /// <summary>
    /// Get the most recent commits on the current branch
    /// </summary>
    /// <param name="count">Number of commits to get</param>
    /// <returns>Recent commits</returns>
    IEnumerable<Commit> GetRecentCommits(int count = 15);

    /// <summary>
    /// Get the currently-staged files
    /// </summary>
    /// <returns>Staged files</returns>
    IEnumerable<StatusEntry> GetStagedFiles();

    /// <summary>
    /// Get the currently-unstaged files
    /// </summary>
    /// <returns>Unstaged files</returns>
    IEnumerable<StatusEntry> GetUnstagedFiles();

    /// <summary>
    /// Stage files
    /// </summary>
    /// <param name="files">Paths to the files to stage</param>
    void StageFiles(IEnumerable<Uri> files);

    /// <summary>
    /// Fetch data from the remote
    /// </summary>
    /// <param name="remote">Remote to fetch from</param>
    void Fetch(string remote = "origin");

    /// <summary>
    /// Pull changes from the remote branch into the local branch
    /// </summary>
    /// <param name="merger">Signature to use if a merge happens</param>
    /// <param name="remote">Remote to pull from</param>
    void Pull(Signature merger, string remote = "origin");

    /// <summary>
    /// Push from the local branch to the remote branch
    /// </summary>
    /// <param name="remote">Remote to push to</param>
    /// <param name="branch">Branch to push</param>
    void Push(string remote = "origin", string branch = "master");

    /// <summary>
    /// Get the local branches
    /// </summary>
    /// <returns>Local branches</returns>
    IEnumerable<Branch> GetLocalBranches();

    /// <summary>
    /// Get the remote branches
    /// </summary>
    /// <returns>Remote branches</returns>
    IEnumerable<Branch> GetRemoteBranches();

    /// <summary>
    /// Create a branch
    /// </summary>
    /// <param name="name">Name of the new branch</param>
    /// <returns>The newly-created branch</returns>
    Branch CreateBranch(string name);

    /// <summary>
    /// Switch to a different branch
    /// </summary>
    /// <param name="name">Name of the branch to switch to</param>
    void SwitchBranches(string name);

    /// <summary>
    /// Make a commit
    /// </summary>
    /// <param name="message">Commit message</param>
    /// <param name="author">Person who authored the changes</param>
    /// <param name="committer">Person committing the changes</param>
    /// <returns>The newly-created commit</returns>
    Commit Commit(string message, Signature author, Signature committer);

    /// <summary>
    /// Reset the working state to a previous commit
    /// </summary>
    /// <param name="mode">Reset mode to use</param>
    /// <param name="commitSha">Commit to reset to</param>
    void Reset(ResetMode mode, string commitSha);

    /// <summary>
    /// Stash changes in the stash
    /// </summary>
    /// <param name="message">Optional message</param>
    void StashSave(string? message = null);

    /// <summary>
    /// Pop the latest stash
    /// </summary>
    void StashPop();
}
