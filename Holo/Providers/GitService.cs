// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Holo.Models;
using LibGit2Sharp;

namespace Holo.Providers;

public class GitService(IFileSystem fileSystem) : IGitService
{
    [MemberNotNullWhen(true, nameof(WorkingDirectory))]
    private bool Ready { get; set; }
    private Uri? WorkingDirectory { get; set; }

    /// <inheritdoc />
    public void SetWorkingDirectory(Uri? path)
    {
        if (path is null)
        {
            WorkingDirectory = null;
            Ready = false;
            return;
        }
        WorkingDirectory = new Uri(fileSystem.Path.GetDirectoryName(path.LocalPath) ?? "/");
        Ready = true;
    }

    /// <inheritdoc />
    public bool IsRepository()
    {
        return Ready && Repository.IsValid(WorkingDirectory.LocalPath);
    }

    public bool HasGitDirectory()
    {
        if (!Ready)
            return false;

        return fileSystem.Directory.GetDirectories(WorkingDirectory.LocalPath, ".git").Length != 0;
    }

    /// <inheritdoc />
    public void InitializeRepository()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");

        Repository.Init(WorkingDirectory.LocalPath);
    }

    /// <inheritdoc />
    public IEnumerable<GitCommit> GetRecentCommits(int count = 15)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var latest = repo.Commits.Take(count).ToList();
        return (
            from commit in latest
            let signature = commit.Author ?? commit.Committer
            let name = signature?.Name ?? "-"
            let email = signature?.Email ?? "-"
            let message = commit.MessageShort ?? "-"
            let date = commit.Committer?.When ?? DateTimeOffset.MinValue
            select new GitCommit(name, email, message, date)
        ).ToList();
    }

    /// <inheritdoc />
    public IEnumerable<GitStatusEntry> GetStagedFiles()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo.RetrieveStatus()
            .Where(se =>
                se.State.HasFlag(FileStatus.ModifiedInIndex)
                || se.State.HasFlag(FileStatus.NewInIndex)
            )
            .Select(se => new GitStatusEntry(se.FilePath, true));
    }

    /// <inheritdoc />
    public IEnumerable<GitStatusEntry> GetUnstagedFiles()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo.RetrieveStatus()
            .Where(se =>
                se.State.HasFlag(FileStatus.ModifiedInWorkdir)
                || se.State.HasFlag(FileStatus.NewInWorkdir)
            )
            .Select(se => new GitStatusEntry(se.FilePath, false));
    }

    /// <inheritdoc />
    public void StageFiles(IEnumerable<string> files)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        Commands.Stage(repo, files);
    }

    /// <inheritdoc />
    public void UnstageFiles(IEnumerable<string> files)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        Commands.Unstage(repo, files);
    }

    /// <inheritdoc />
    public void Fetch()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var remoteName = repo.Head.TrackedBranch?.RemoteName ?? "origin";
        var remote = repo.Network.Remotes[remoteName];

        Commands.Fetch(
            repo,
            remote.Name,
            remote.FetchRefSpecs.Select(r => r.Specification),
            null,
            null
        );
    }

    /// <inheritdoc />
    public void Pull()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var options = new PullOptions { FetchOptions = new FetchOptions() };

        var merger = repo.Config.BuildSignature(DateTimeOffset.Now);
        Commands.Pull(repo, merger, options);
    }

    /// <inheritdoc />
    public void Push()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var branch = repo.Head;
        var remote = repo.Network.Remotes[
            branch.TrackedBranch?.RemoteName
                ?? throw new InvalidOperationException("Local branch has no tracking branch")
        ];

        var refSpec = $"{branch.CanonicalName}:{branch.UpstreamBranchCanonicalName}";
        repo.Network.Push(remote, refSpec, new PushOptions());
    }

    /// <inheritdoc />
    public GitBranch GetCurrentBranch()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var branch = repo.Head;
        return new GitBranch(branch.FriendlyName, branch.IsRemote, branch.IsTracking);
    }

    /// <inheritdoc />
    public IEnumerable<GitBranch> GetLocalBranches()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo
            .Branches.Where(b => !b.IsRemote)
            .Select(b => new GitBranch(b.FriendlyName, false, b.IsTracking));
    }

    /// <inheritdoc />
    public IEnumerable<GitBranch> GetRemoteBranches()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo
            .Branches.Where(b => b.IsRemote)
            .Select(b => new GitBranch(b.FriendlyName, true, false));
    }

    /// <inheritdoc />
    public GitBranch CreateBranch(string name)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var branch = repo.CreateBranch(name);
        return new GitBranch(branch.FriendlyName, false, branch.IsTracking);
    }

    /// <inheritdoc />
    public void SwitchBranches(string name)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var branch = repo.Branches[name];
        if (branch is null)
            throw new ArgumentException($"Branch {name} not found", nameof(name));

        Commands.Checkout(repo, branch);
    }

    /// <inheritdoc />
    public GitCommit Commit(string message)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        if (!GetStagedFiles().Any())
            throw new InvalidOperationException("No files staged for commit");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
        var commit = repo.Commit(message, signature, signature);
        return new GitCommit(
            commit.Author.Name,
            commit.Author.Email,
            commit.MessageShort,
            commit.Author.When
        );
    }

    /// <inheritdoc />
    public void Reset(ResetMode mode, string commitSha)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var commit = repo.Lookup<Commit>(commitSha);
        if (commit is null)
            throw new ArgumentException($"Commit {commitSha} not found", nameof(commitSha));

        repo.Reset(mode, commit);
    }

    /// <inheritdoc />
    public void StashSave(string? message = null)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var author = repo.Config.BuildSignature(DateTimeOffset.Now);
        repo.Stashes.Add(author, message ?? "Stashed Changes", StashModifiers.Default);
    }

    /// <inheritdoc />
    public void StashPop()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        if (!repo.Stashes.Any())
            return;

        repo.Stashes.Pop(0);
    }
}
