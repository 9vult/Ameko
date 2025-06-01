// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Ameko.Services;
using Holo;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Interactions
    public Interaction<LogWindowViewModel, Unit> ShowLogWindow { get; }
    #endregion

    #region Commands
    public ICommand ShowLogWindowCommand { get; }
    #endregion

    public Solution Solution
    {
        get => HoloContext.Instance.Solution;
        set
        {
            HoloContext.Instance.Solution = value;
            this.RaisePropertyChanged();
        }
    }

    public string Greeting { get; } = $"Welcome to Ameko {VersionService.FullLabel}!";

    public MainWindowViewModel()
    {
        #region Interactions
        ShowLogWindow = new Interaction<LogWindowViewModel, Unit>();
        #endregion

        #region Commands
        ShowLogWindowCommand = CreateShowLogWindowCommand();
        #endregion
    }
}
