// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using System.Windows.Input;
using Ameko.Services;
using Holo;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Interactions
    // File
    public Interaction<Unit, Uri[]> OpenSubtitle { get; }

    // Help
    public Interaction<LogWindowViewModel, Unit> ShowLogWindow { get; }
    public Interaction<AboutWindowViewModel, Unit> ShowAboutWindow { get; }
    #endregion

    #region Commands
    // File
    public ICommand NewCommand { get; }
    public ICommand OpenSubtitleCommand { get; }

    // Help
    public ICommand ShowLogWindowCommand { get; }
    public ICommand ShowAboutWindowCommand { get; }
    #endregion

    /// <summary>
    /// Window title
    /// </summary>
    public string WindowTitle { get; } = $"Ameko {VersionService.FullLabel}";

    /// <summary>
    /// Currently-open <see cref="Solution"/>
    /// </summary>
    public Solution Solution
    {
        get => HoloContext.Instance.Solution;
        set
        {
            HoloContext.Instance.Solution = value;
            this.RaisePropertyChanged();
        }
    }

    public MainWindowViewModel()
    {
        #region Interactions
        // File
        OpenSubtitle = new Interaction<Unit, Uri[]>();
        // Help
        ShowLogWindow = new Interaction<LogWindowViewModel, Unit>();
        ShowAboutWindow = new Interaction<AboutWindowViewModel, Unit>();
        #endregion

        #region Commands
        // File
        NewCommand = CreateNewCommand();
        OpenSubtitleCommand = CreateOpenSubtitleCommand();
        // Help
        ShowLogWindowCommand = CreateShowLogWindowCommand();
        ShowAboutWindowCommand = CreateShowAboutWindowCommand();
        #endregion
    }
}
