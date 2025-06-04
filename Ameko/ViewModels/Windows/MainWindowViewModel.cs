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
    public Interaction<string, Uri?> SaveSubtitleAs { get; }
    public Interaction<string, Uri?> ExportSubtitle { get; }

    // Help
    public Interaction<LogWindowViewModel, Unit> ShowLogWindow { get; }
    public Interaction<AboutWindowViewModel, Unit> ShowAboutWindow { get; }
    #endregion

    #region Commands
    // File
    public ICommand NewCommand { get; }
    public ICommand OpenSubtitleCommand { get; }
    public ICommand SaveSubtitleCommand { get; }
    public ICommand SaveSubtitleAsCommand { get; }
    public ICommand ExportSubtitleCommand { get; }
    public ICommand CloseTabCommand { get; }
    public ICommand QuitCommand { get; }

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
        SaveSubtitleAs = new Interaction<string, Uri?>();
        ExportSubtitle = new Interaction<string, Uri?>();
        // Help
        ShowLogWindow = new Interaction<LogWindowViewModel, Unit>();
        ShowAboutWindow = new Interaction<AboutWindowViewModel, Unit>();
        #endregion

        #region Commands
        // File
        NewCommand = CreateNewCommand();
        OpenSubtitleCommand = CreateOpenSubtitleCommand();
        SaveSubtitleCommand = CreateSaveSubtitleCommand();
        SaveSubtitleAsCommand = CreateSaveSubtitleAsCommand();
        ExportSubtitleCommand = CreateExportSubtitleCommand();
        CloseTabCommand = CreateCloseTabCommand();
        QuitCommand = CreateQuitCommand();
        // Help
        ShowLogWindowCommand = CreateShowLogWindowCommand();
        ShowAboutWindowCommand = CreateShowAboutWindowCommand();
        #endregion
    }
}
