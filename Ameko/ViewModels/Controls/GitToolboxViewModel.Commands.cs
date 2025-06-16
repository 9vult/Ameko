// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using Holo.Models;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

public partial class GitToolboxViewModel : ViewModelBase
{
    private ReactiveCommand<Unit, Unit> CreateRefreshCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            _gitService.SetWorkingDirectory(_solutionProvider.Current.SavePath);
            RaisePropertyChanges();
        });
    }

    private ReactiveCommand<GitStatusEntry, Unit> CreateStageCommand()
    {
        return ReactiveCommand.Create(
            (GitStatusEntry entry) =>
            {
                _gitService.StageFiles([new Uri(entry.FilePath)]);
                RaisePropertyChanges();
            }
        );
    }

    private ReactiveCommand<GitStatusEntry, Unit> CreateUnstageCommand()
    {
        return ReactiveCommand.Create(
            (GitStatusEntry entry) =>
            {
                _gitService.UnstageFiles([new Uri(entry.FilePath)]);
                RaisePropertyChanges();
            }
        );
    }

    private ReactiveCommand<GitStatusEntry, Unit> CreateCommitCommand()
    {
        return ReactiveCommand.Create(
            (GitStatusEntry entry) =>
            {
                _gitService.Commit(CommitMessage);
                CommitMessage = string.Empty;
                RaisePropertyChanges();
            }
        );
    }

    private void RaisePropertyChanges()
    {
        this.RaisePropertyChanged(nameof(IsInRepo));
        this.RaisePropertyChanged(nameof(StagedFiles));
        this.RaisePropertyChanged(nameof(UnstagedFiles));
        this.RaisePropertyChanged(nameof(CommitButtonToolTip));
        this.RaisePropertyChanged(nameof(AnythingToCommit));
    }
}
