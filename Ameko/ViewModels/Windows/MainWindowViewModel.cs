// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Ameko.Services;
using Ameko.Utilities;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using DynamicData;
using Holo;
using Holo.Configuration.Keybinds;
using Holo.Providers;
using NLog;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

[KeybindContext(KeybindContext.Global)]
public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IServiceProvider _serviceProvider;
    private readonly IStylesManagerFactory _stylesManagerFactory;
    private readonly IIoService _ioService;
    private readonly IScriptService _scriptService;
    private readonly IFileSystem _fileSystem;

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
    [KeybindTarget("ameko.document.new", "Ctrl+N")]
    public ICommand NewCommand { get; }

    [KeybindTarget("ameko.document.open", "Ctrl+O")]
    public ICommand OpenSubtitleCommand { get; }

    [KeybindTarget("ameko.document.save", "Ctrl+S")]
    public ICommand SaveSubtitleCommand { get; }

    [KeybindTarget("ameko.document.saveAs", "Ctrl+Shift+S")]
    public ICommand SaveSubtitleAsCommand { get; }

    [KeybindTarget("ameko.document.export")]
    public ICommand ExportSubtitleCommand { get; }

    [KeybindTarget("ameko.solution.open")]
    public ICommand OpenSolutionCommand { get; }

    [KeybindTarget("ameko.solution.save")]
    public ICommand SaveSolutionCommand { get; }

    [KeybindTarget("ameko.workspace.close", "Ctrl+W")]
    public ICommand CloseTabCommand { get; }

    [KeybindTarget("ameko.application.quit", "Ctrl+Q")]
    public ICommand QuitCommand { get; }

    // Subtitle
    [KeybindTarget("ameko.stylesManager.show")]
    public ICommand ShowStylesManagerCommand { get; }

    // Scripts
    // Command execution doesn't get a keybind. So sad :(
    public ICommand ExecuteScriptCommand { get; }

    [KeybindTarget("ameko.scripts.reload")]
    public ICommand ReloadScriptsCommand { get; }

    [KeybindTarget("ameko.depCtrl.show")]
    public ICommand ShowDependencyControlCommand { get; }

    // Help
    [KeybindTarget("ameko.logs.show", "Ctrl+L")]
    public ICommand ShowLogWindowCommand { get; }

    [KeybindTarget("ameko.about.show", "Shift+F1")]
    public ICommand ShowAboutWindowCommand { get; }

    // Other
    // These commands are specific to the solution explorer and don't need keybinds
    public ICommand RemoveDocumentFromSolutionCommand { get; }
    public ICommand RemoveDirectoryFromSolutionCommand { get; }
    public ICommand RenameDocumentCommand { get; }
    public ICommand RenameDirectoryCommand { get; }
    #endregion

    public ISolutionProvider SolutionProvider { get; }
    public KeybindService KeybindService { get; }

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
        Log.Trace("Regenerating scripts menu...");
        ScriptMenuItems.Clear();
        ScriptMenuItems.AddRange(
            ScriptMenuService.GenerateMenuItemSource(_scriptService.Scripts, ExecuteScriptCommand)
        );

        ScriptMenuItems.Add(new Separator());
        ScriptMenuItems.Add(ScriptMenuService.GenerateReloadMenuItem(ReloadScriptsCommand));
        ScriptMenuItems.Add(ScriptMenuService.GenerateDepCtlMenuItem(ShowDependencyControlCommand));
        Log.Trace("Done!");
    }

    public MainWindowViewModel(
        IServiceProvider serviceProvider,
        ISolutionProvider solutionProvider,
        IStylesManagerFactory stylesManagerFactory,
        IIoService ioService,
        IScriptService scriptService,
        IFileSystem fileSystem,
        KeybindService keybindService
    )
    {
        _serviceProvider = serviceProvider;
        SolutionProvider = solutionProvider;
        _stylesManagerFactory = stylesManagerFactory;
        _ioService = ioService;
        _scriptService = scriptService;
        _fileSystem = fileSystem;
        KeybindService = keybindService;

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
        _scriptService.OnReload += (_, _) => GenerateScriptsMenu();
    }
}
