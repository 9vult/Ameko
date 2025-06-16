// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Holo.Models;
using Holo.Providers;

namespace Ameko.ViewModels.Controls;

public partial class GitToolboxViewModel : ViewModelBase
{
    private readonly IGitService _gitService;
    private readonly ISolutionProvider _solutionProvider;

    public bool IsInRepo => _gitService.IsRepository();
    public bool HasStagedChanges => StagedFiles.Count != 0;
    public bool HasUnstagedChanges => UnstagedFiles.Count != 0;

    public string CommitMessage { get; set; } = string.Empty;

    public ICommand RefreshCommand { get; }

    public string CommitButtonToolTip =>
        string.Format(
            I18N.Git.Git_Button_Commit_ToolTip,
            IsInRepo ? _gitService.GetCurrentBranch().Name : "master"
        );

    public List<GitStatusEntry> StagedFiles =>
        IsInRepo ? _gitService.GetStagedFiles().ToList() : [];
    public List<GitStatusEntry> UnstagedFiles =>
        IsInRepo ? _gitService.GetUnstagedFiles().ToList() : [];

    public GitToolboxViewModel(IGitService gitService, ISolutionProvider solutionProvider)
    {
        _gitService = gitService;
        _solutionProvider = solutionProvider;

        RefreshCommand = CreateRefreshCommand();
    }
}
