// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

public partial class GitToolboxViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> CreateRefreshCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            _gitService.SetWorkingDirectory(_solutionProvider.Current.SavePath);
            this.RaisePropertyChanged(nameof(IsInRepo));
            this.RaisePropertyChanged(nameof(StagedFiles));
            this.RaisePropertyChanged(nameof(UnstagedFiles));
            this.RaisePropertyChanged(nameof(CommitButtonToolTip));
        });
    }
}
