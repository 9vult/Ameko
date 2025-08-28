// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Ameko.Messages;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Controls;
using Ameko.ViewModels.Dialogs;
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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IServiceProvider _serviceProvider;
    private readonly IStylesManagerFactory _stylesManagerFactory;
    private readonly IIoService _ioService;
    private readonly IScriptService _scriptService;
    private readonly IFileSystem _fileSystem;

    public ILayoutProvider LayoutProvider { get; }

    #region Interactions
    // File
    public Interaction<Unit, Uri[]> OpenSubtitle { get; }
    public Interaction<string, Uri?> SaveSubtitleAs { get; }
    public Interaction<string, Uri?> ExportSubtitle { get; }
    public Interaction<Unit, Uri?> OpenSolution { get; }
    public Interaction<Unit, Uri?> OpenFolderAsSolution { get; }
    public Interaction<string, Uri?> SaveSolutionAs { get; }
    public Interaction<Unit, Uri?> AttachReferenceFile { get; }

    // Subtitle
    public Interaction<StylesManagerWindowViewModel, Unit> ShowStylesManager { get; }

    // Solution
    // Timing
    public Interaction<ShiftTimesDialogViewModel, Unit> ShowShiftTimesDialog { get; }

    // Video
    public Interaction<Unit, Uri?> OpenVideo { get; }
    public Interaction<JumpDialogViewModel, JumpDialogClosedMessage?> ShowJumpDialog { get; }

    // Scripts
    public Interaction<PkgManWindowViewModel, Unit> ShowPackageManager { get; }

    // Help
    public Interaction<LogWindowViewModel, Unit> ShowLogWindow { get; }
    public Interaction<AboutWindowViewModel, Unit> ShowAboutWindow { get; }
    public Interaction<KeybindsWindowViewModel, Unit> ShowKeybindsWindow { get; }
    #endregion

    #region Commands
    // File
    [KeybindTarget("ameko.document.new", "Ctrl+N")]
    public ICommand NewCommand { get; }

    [KeybindTarget("ameko.document.open", "Ctrl+O")]
    public ICommand OpenSubtitleCommand { get; }
    public ICommand OpenSubtitleNoGuiCommand { get; }

    [KeybindTarget("ameko.document.save", "Ctrl+S")]
    public ICommand SaveSubtitleCommand { get; }

    [KeybindTarget("ameko.document.saveAs", "Ctrl+Shift+S")]
    public ICommand SaveSubtitleAsCommand { get; }

    [KeybindTarget("ameko.document.export")]
    public ICommand ExportSubtitleCommand { get; }

    [KeybindTarget("ameko.solution.open")]
    public ICommand OpenSolutionCommand { get; }
    public ICommand OpenSolutionNoGuiCommand { get; }

    [KeybindTarget("ameko.solution.openFolder")]
    public ICommand OpenFolderAsSolutionCommand { get; }

    [KeybindTarget("ameko.solution.save")]
    public ICommand SaveSolutionCommand { get; }

    [KeybindTarget("ameko.workspace.close", "Ctrl+W")]
    public ICommand CloseTabCommand { get; }

    [KeybindTarget("ameko.application.quit", "Ctrl+Q")]
    public ICommand QuitCommand { get; }

    // Subtitle
    [KeybindTarget("ameko.stylesManager.show")]
    public ICommand ShowStylesManagerCommand { get; }

    [KeybindTarget("ameko.reference.attach")]
    public ICommand AttachReferenceFileCommand { get; }

    // Solution
    // Timing
    [KeybindTarget("ameko.document.shiftTimes", "Ctrl+I")]
    public ICommand ShowShiftTimesDialogCommand { get; }

    // Video
    [KeybindTarget("ameko.video.open")]
    public ICommand OpenVideoCommand { get; }
    public ICommand OpenVideoNoGuiCommand { get; }

    [KeybindTarget("ameko.video.jump", "Ctrl+G")]
    public ICommand ShowJumpDialogCommand { get; }

    // Scripts
    // Command execution doesn't get a keybind. So sad :(
    public ICommand ExecuteScriptCommand { get; }

    [KeybindTarget("ameko.scripts.reload")]
    public ICommand ReloadScriptsCommand { get; }

    [KeybindTarget("ameko.pkgMan.show")]
    public ICommand ShowPackageManagerCommand { get; }

    // Layouts
    public ICommand SelectLayoutCommand { get; }
    public ICommand RefreshLayoutsCommand { get; }

    // Help
    [KeybindTarget("ameko.logs.show", "Ctrl+L")]
    public ICommand ShowLogWindowCommand { get; }

    [KeybindTarget("ameko.about.show", "Shift+F1")]
    public ICommand ShowAboutWindowCommand { get; }

    [KeybindTarget("ameko.keybinds.show")]
    public ICommand ShowKeybindsWindowCommand { get; }

    // Other
    // These commands are specific to the solution explorer and don't need keybinds
    public ICommand RemoveDocumentFromSolutionCommand { get; }
    public ICommand RemoveDirectoryFromSolutionCommand { get; }
    public ICommand RenameDocumentCommand { get; }
    public ICommand RenameDirectoryCommand { get; }
    #endregion

    public ISolutionProvider SolutionProvider { get; }
    public IKeybindService KeybindService { get; }
    public GitToolboxViewModel GitToolboxViewModel { get; }

    public ObservableCollection<TemplatedControl> ScriptMenuItems { get; }
    public ObservableCollection<TemplatedControl> LayoutMenuItems { get; }

    /// <summary>
    /// WindowSection title
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
        Logger.Debug("Regenerating scripts menu...");
        ScriptMenuItems.Clear();
        ScriptMenuItems.AddRange(
            ScriptMenuService.GenerateMenuItemSource(_scriptService.Scripts, ExecuteScriptCommand)
        );

        ScriptMenuItems.Add(new Separator());
        ScriptMenuItems.Add(ScriptMenuService.GenerateReloadMenuItem(ReloadScriptsCommand));
        ScriptMenuItems.Add(ScriptMenuService.GeneratePkgManMenuItem(ShowPackageManagerCommand));
        Logger.Debug("Done!");
    }

    private void GenerateLayoutsMenu()
    {
        Logger.Debug("Regenerating layouts menu...");
        LayoutMenuItems.Clear();
        LayoutMenuItems.AddRange(
            LayoutMenuService.GenerateMenuItemSource(LayoutProvider.Layouts, SelectLayoutCommand)
        );
        LayoutMenuItems.Add(new Separator());
        LayoutMenuItems.Add(LayoutMenuService.GenerateReloadMenuItem(RefreshLayoutsCommand));
        Logger.Debug("Done!");
    }

    public MainWindowViewModel(
        IServiceProvider serviceProvider,
        IFileSystem fileSystem,
        IIoService ioService,
        ILayoutProvider layoutProvider,
        ISolutionProvider solutionProvider,
        IStylesManagerFactory stylesManagerFactory,
        IScriptService scriptService,
        IKeybindService keybindService,
        GitToolboxViewModel gitToolboxViewModel
    )
    {
        _serviceProvider = serviceProvider;
        _fileSystem = fileSystem;
        _ioService = ioService;
        LayoutProvider = layoutProvider;
        SolutionProvider = solutionProvider;
        _stylesManagerFactory = stylesManagerFactory;
        _scriptService = scriptService;
        KeybindService = keybindService;
        GitToolboxViewModel = gitToolboxViewModel;

        #region Interactions
        // File
        OpenSubtitle = new Interaction<Unit, Uri[]>();
        SaveSubtitleAs = new Interaction<string, Uri?>();
        ExportSubtitle = new Interaction<string, Uri?>();
        OpenSolution = new Interaction<Unit, Uri?>();
        OpenFolderAsSolution = new Interaction<Unit, Uri?>();
        SaveSolutionAs = new Interaction<string, Uri?>();
        // Subtitle
        ShowStylesManager = new Interaction<StylesManagerWindowViewModel, Unit>();
        AttachReferenceFile = new Interaction<Unit, Uri?>();
        // Solution
        // Video
        OpenVideo = new Interaction<Unit, Uri?>();
        ShowJumpDialog = new Interaction<JumpDialogViewModel, JumpDialogClosedMessage?>();
        // Timing
        ShowShiftTimesDialog = new Interaction<ShiftTimesDialogViewModel, Unit>();
        // Scripts
        ShowPackageManager = new Interaction<PkgManWindowViewModel, Unit>();
        // Help
        ShowLogWindow = new Interaction<LogWindowViewModel, Unit>();
        ShowAboutWindow = new Interaction<AboutWindowViewModel, Unit>();
        ShowKeybindsWindow = new Interaction<KeybindsWindowViewModel, Unit>();
        #endregion

        #region Commands
        // File
        NewCommand = CreateNewCommand();
        OpenSubtitleCommand = CreateOpenSubtitleCommand();
        OpenSubtitleNoGuiCommand = CreateOpenSubtitleNoGuiCommand();
        SaveSubtitleCommand = CreateSaveSubtitleCommand();
        SaveSubtitleAsCommand = CreateSaveSubtitleAsCommand();
        ExportSubtitleCommand = CreateExportSubtitleCommand();
        OpenSolutionCommand = CreateOpenSolutionCommand();
        OpenSolutionNoGuiCommand = CreateOpenSolutionNoGuiCommand();
        OpenFolderAsSolutionCommand = CreateOpenFolderAsSolutionCommand();
        SaveSolutionCommand = CreateSaveSolutionCommand();
        CloseTabCommand = CreateCloseTabCommand();
        QuitCommand = CreateQuitCommand();
        // Subtitle
        ShowStylesManagerCommand = CreateShowStylesManagerCommand();
        AttachReferenceFileCommand = CreateAttachReferenceFileCommand();
        // Solution
        // Timing
        ShowShiftTimesDialogCommand = CreateShowShiftTimesDialogCommand();
        // Video
        OpenVideoCommand = CreateOpenVideoCommand();
        OpenVideoNoGuiCommand = CreateOpenVideoNoGuiCommand();
        ShowJumpDialogCommand = CreateShowJumpDialogCommand();
        // Scripts
        ExecuteScriptCommand = CreateExecuteScriptCommand();
        ReloadScriptsCommand = CreateReloadScriptsCommand();
        ShowPackageManagerCommand = CreateShowPackageManagerCommand();
        // Layouts
        SelectLayoutCommand = CreateSelectLayoutCommand();
        RefreshLayoutsCommand = CreateRefreshLayoutsCommand();
        // Help
        ShowLogWindowCommand = CreateShowLogWindowCommand();
        ShowAboutWindowCommand = CreateShowAboutWindowCommand();
        ShowKeybindsWindowCommand = CreateShowKeybindsWindowCommand();
        // Other
        RemoveDocumentFromSolutionCommand = CreateRemoveDocumentFromSolutionCommand();
        RemoveDirectoryFromSolutionCommand = CreateRemoveDirectoryFromSolutionCommand();
        RenameDocumentCommand = CreateRenameDocumentCommand();
        RenameDirectoryCommand = CreateRenameDirectoryCommand();
        #endregion

        ScriptMenuItems = [];
        _scriptService.OnReload += (_, _) => GenerateScriptsMenu();

        LayoutMenuItems = [];
        LayoutProvider.OnLayoutChanged += (_, _) => GenerateLayoutsMenu();
        GenerateLayoutsMenu();
    }
}
