// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using LibGit2Sharp;

namespace Holo.Services;

public class GitService : IGitService
{
    [MemberNotNullWhen(true, nameof(WorkingDirectory))]
    private bool Ready { get; set; }
    private Uri? WorkingDirectory { get; set; }

    /// <inheritdoc />
    public void SetWorkingDirectory(Uri path)
    {
        WorkingDirectory = path;
        Ready = true;
    }

    /// <inheritdoc />
    public bool IsRepository()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");

        return Repository.IsValid(WorkingDirectory.LocalPath);
    }

    /// <inheritdoc />
    public void InitializeRepository()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");

        Repository.Init(WorkingDirectory.LocalPath);
    }

    /// <inheritdoc />
    public IEnumerable<Commit> GetRecentCommits(int count = 15)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo.Commits.Take(count);
    }

    /// <inheritdoc />
    public IEnumerable<StatusEntry> GetStagedFiles()
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
            );
    }

    /// <inheritdoc />
    public IEnumerable<StatusEntry> GetUnstagedFiles()
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
            );
    }

    /// <inheritdoc />
    public void StageFiles(IEnumerable<Uri> files)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        Commands.Stage(repo, files.Select(f => f.LocalPath));
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
    public void Pull(Signature merger)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        var options = new PullOptions { FetchOptions = new FetchOptions() };

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
    public IEnumerable<Branch> GetLocalBranches()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo.Branches.Where(b => !b.IsRemote);
    }

    /// <inheritdoc />
    public IEnumerable<Branch> GetRemoteBranches()
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo.Branches.Where(b => b.IsRemote);
    }

    /// <inheritdoc />
    public Branch CreateBranch(string name)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo.CreateBranch(name);
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
    public Commit Commit(string message, Signature author, Signature committer)
    {
        if (!Ready)
            throw new InvalidOperationException("Working directory not set");
        if (!IsRepository())
            throw new InvalidOperationException("Not in a repository");

        if (!GetStagedFiles().Any())
            throw new InvalidOperationException("No files staged for commit");

        using var repo = new Repository(WorkingDirectory.LocalPath);
        return repo.Commit(message, author, committer);
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
