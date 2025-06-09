// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Ameko.Services;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using DynamicData;
using Holo;
using Holo.Providers;
using NLog;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IServiceProvider _serviceProvider;
    private readonly IoService _ioService;

    #region Interactions
    // File
    public Interaction<Unit, Uri[]> OpenSubtitle { get; }
    public Interaction<string, Uri?> SaveSubtitleAs { get; }
    public Interaction<string, Uri?> ExportSubtitle { get; }
    public Interaction<Unit, Uri?> OpenSolution { get; }
    public Interaction<string, Uri?> SaveSolutionAs { get; }

    // Subtitle
    public Interaction<StylesManagerWindowViewModel, Unit> ShowStylesManager { get; }

    // Scripts
    public Interaction<DepCtrlWindowViewModel, Unit> ShowDependencyControl { get; }

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
    public ICommand OpenSolutionCommand { get; }
    public ICommand SaveSolutionCommand { get; }
    public ICommand CloseTabCommand { get; }
    public ICommand QuitCommand { get; }

    // Subtitle
    public ICommand ShowStylesManagerCommand { get; }

    // Scripts
    public ICommand ExecuteScriptCommand { get; }
    public ICommand ReloadScriptsCommand { get; }
    public ICommand ShowDependencyControlCommand { get; }

    // Help
    public ICommand ShowLogWindowCommand { get; }
    public ICommand ShowAboutWindowCommand { get; }

    // Other
    public ICommand RemoveDocumentFromSolutionCommand { get; }
    public ICommand RemoveDirectoryFromSolutionCommand { get; }
    public ICommand RenameDocumentCommand { get; }
    public ICommand RenameDirectoryCommand { get; }
    #endregion

    public ISolutionProvider SolutionProvider { get; }

    public ObservableCollection<TemplatedControl> ScriptMenuItems { get; }

    /// <summary>
    /// Window title
    /// </summary>
    public string WindowTitle { get; } = $"Ameko {VersionService.FullLabel}";

    /// <summary>
    /// Set the <see cref="Solution.WorkingSpace"/> to the selected workspace, opening it if needed
    /// </summary>
    /// <param name="workspaceId">ID to open</param>
    public void TryLoadReferenced(int workspaceId)
    {
        var wsp = SolutionProvider.Current.LoadedWorkspaces.FirstOrDefault(w =>
            w.Id == workspaceId
        );
        if (wsp is not null)
        {
            SolutionProvider.Current.WorkingSpace = wsp;
            return;
        }

        SolutionProvider.Current.OpenDocument(workspaceId);
    }

    private void GenerateScriptsMenu()
    {
        ScriptMenuItems.Clear();
        ScriptMenuItems.AddRange(
            ScriptMenuService.GenerateMenuItemSource(
                ScriptService.Instance.Scripts,
                ExecuteScriptCommand
            )
        );

        ScriptMenuItems.Add(new Separator());
        ScriptMenuItems.Add(ScriptMenuService.GenerateReloadMenuItem(ReloadScriptsCommand));
        ScriptMenuItems.Add(ScriptMenuService.GenerateDepCtlMenuItem(ShowDependencyControlCommand));
    }

    public MainWindowViewModel(
        IServiceProvider serviceProvider,
        IoService ioService,
        ISolutionProvider solutionProvider
    )
    {
        _serviceProvider = serviceProvider;
        _ioService = ioService;
        SolutionProvider = solutionProvider;

        #region Interactions
        // File
        OpenSubtitle = new Interaction<Unit, Uri[]>();
        SaveSubtitleAs = new Interaction<string, Uri?>();
        ExportSubtitle = new Interaction<string, Uri?>();
        OpenSolution = new Interaction<Unit, Uri?>();
        SaveSolutionAs = new Interaction<string, Uri?>();
        // Subtitle
        ShowStylesManager = new Interaction<StylesManagerWindowViewModel, Unit>();
        // Scripts
        ShowDependencyControl = new Interaction<DepCtrlWindowViewModel, Unit>();
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
        OpenSolutionCommand = CreateOpenSolutionCommand();
        SaveSolutionCommand = CreateSaveSolutionCommand();
        CloseTabCommand = CreateCloseTabCommand();
        QuitCommand = CreateQuitCommand();
        // Subtitle
        ShowStylesManagerCommand = CreateShowStylesManagerCommand();
        // Scripts
        ExecuteScriptCommand = CreateExecuteScriptCommand();
        ReloadScriptsCommand = CreateReloadScriptsCommand();
        ShowDependencyControlCommand = CreateShowDependencyControlCommand();
        // Help
        ShowLogWindowCommand = CreateShowLogWindowCommand();
        ShowAboutWindowCommand = CreateShowAboutWindowCommand();
        // Other
        RemoveDocumentFromSolutionCommand = CreateRemoveDocumentFromSolutionCommand();
        RemoveDirectoryFromSolutionCommand = CreateRemoveDirectoryFromSolutionCommand();
        RenameDocumentCommand = CreateRenameDocumentCommand();
        RenameDirectoryCommand = CreateRenameDirectoryCommand();
        #endregion

        ScriptMenuItems = [];
        GenerateScriptsMenu();
        ScriptService.Instance.Scripts.CollectionChanged += (_, _) => GenerateScriptsMenu();
    }
}
