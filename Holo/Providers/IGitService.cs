// SPDX-License-Identifier: MPL-2.0

using Holo.Models;
using LibGit2Sharp;

namespace Holo.Providers;

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
    void SetWorkingDirectory(Uri? path);

    /// <summary>
    /// Check if the working directory is a Git repository
    /// </summary>
    /// <returns><see langword="true"/> if it is a Git repository</returns>
    bool IsRepository();

    /// <summary>
    /// Check if the working directory contains a .git directory
    /// </summary>
    /// <returns><see langword="true"/> if a .git directory exists</returns>
    bool HasGitDirectory();

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
    IEnumerable<GitStatusEntry> GetStagedFiles();

    /// <summary>
    /// Get the currently-unstaged files
    /// </summary>
    /// <returns>Unstaged files</returns>
    IEnumerable<GitStatusEntry> GetUnstagedFiles();

    /// <summary>
    /// Stage files
    /// </summary>
    /// <param name="files">Paths to the files to stage</param>
    void StageFiles(IEnumerable<Uri> files);

    /// <summary>
    /// Stage files
    /// </summary>
    /// <param name="files">Paths to the files to unstage</param>
    void UnstageFiles(IEnumerable<Uri> files);

    /// <summary>
    /// Fetch data from the remote
    /// </summary>
    void Fetch();

    /// <summary>
    /// Pull changes from the remote branch into the local branch
    /// </summary>
    /// <param name="merger">Signature to use if a merge happens</param>
    void Pull(Signature merger);

    /// <summary>
    /// Push from the local branch to the remote branch
    /// </summary>
    void Push();

    /// <summary>
    /// Get the current branch
    /// </summary>
    /// <returns>The current branch</returns>
    GitBranch GetCurrentBranch();

    /// <summary>
    /// Get the local branches
    /// </summary>
    /// <returns>Local branches</returns>
    IEnumerable<GitBranch> GetLocalBranches();

    /// <summary>
    /// Get the remote branches
    /// </summary>
    /// <returns>Remote branches</returns>
    IEnumerable<GitBranch> GetRemoteBranches();

    /// <summary>
    /// Create a branch
    /// </summary>
    /// <param name="name">Name of the new branch</param>
    /// <returns>The newly-created branch</returns>
    GitBranch CreateBranch(string name);

    /// <summary>
    /// Switch to a different branch
    /// </summary>
    /// <param name="name">Name of the branch to switch to</param>
    void SwitchBranches(string name);

    /// <summary>
    /// Make a commit
    /// </summary>
    /// <param name="message">Commit message</param>
    /// <returns>The newly-created commit</returns>
    Commit Commit(string message);

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
