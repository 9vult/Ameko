// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Holo.Models;
using Holo.Providers;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

public partial class GitToolboxViewModel : ViewModelBase
{
    private readonly IGitService _gitService;
    private readonly IProjectProvider _iProjectProvider;

    private string _commitMessage = string.Empty;

    public ICommand RefreshCommand { get; }
    public ICommand StageCommand { get; }
    public ICommand UnstageCommand { get; }
    public ICommand CommitCommand { get; }
    public ICommand PullCommand { get; }
    public ICommand PushCommand { get; }

    public bool IsPotentialOwnershipIssue =>
        !_gitService.IsRepository() && _gitService.HasGitDirectory();

    public bool IsInRepo => _gitService.IsRepository();
    public bool HasStagedChanges => StagedFiles.Count != 0;
    public bool HasUnstagedChanges => UnstagedFiles.Count != 0;
    public bool AnythingToCommit => IsInRepo && (HasStagedChanges || HasUnstagedChanges);
    public bool CanCommit => !string.IsNullOrWhiteSpace(_commitMessage);
    public bool CanPush => IsInRepo && _gitService.CanPush();

    public string CommitMessage
    {
        get => _commitMessage;
        set
        {
            this.RaiseAndSetIfChanged(ref _commitMessage, value);
            this.RaisePropertyChanged(nameof(CanCommit));
        }
    }

    public string CommitButtonToolTip =>
        string.Format(
            I18N.Git.Git_Button_Commit_ToolTip,
            IsInRepo ? _gitService.GetCurrentBranch().Name : "master"
        );

    public string LatestCommitsHeader =>
        string.Format(
            I18N.Git.Git_LatestCommits,
            IsInRepo ? _gitService.GetCurrentBranch().Name : "master"
        );

    public List<GitStatusEntry> StagedFiles =>
        IsInRepo ? _gitService.GetStagedFiles().ToList() : [];
    public List<GitStatusEntry> UnstagedFiles =>
        IsInRepo ? _gitService.GetUnstagedFiles().ToList() : [];

    public List<GitCommit> LatestCommits => IsInRepo ? _gitService.GetRecentCommits().ToList() : [];

    public GitToolboxViewModel(IGitService gitService, IProjectProvider iProjectProvider)
    {
        _gitService = gitService;
        _iProjectProvider = iProjectProvider;

        RefreshCommand = CreateRefreshCommand();
        StageCommand = CreateStageCommand();
        UnstageCommand = CreateUnstageCommand();
        CommitCommand = CreateCommitCommand();
        PullCommand = CreatePullCommand();
        PushCommand = CreatePushCommand();
    }
}
