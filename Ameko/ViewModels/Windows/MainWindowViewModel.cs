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
using Holo.Configuration;
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
    private readonly IMessageService _messageService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISpellcheckService _spellcheckService;
    private readonly IDictionaryService _dictionaryService;
    private readonly IConfiguration _configuration;
    private readonly IFileSystem _fileSystem;

    private string _currentMessage = I18N.Resources.Message_Welcome;

    public IScriptService ScriptService { get; }
    public ILayoutProvider LayoutProvider { get; }

    public bool DisplayInWindowMenu { get; } = !OperatingSystem.IsMacOS();

    #region Interactions
    // File
    public Interaction<Unit, Uri[]?> OpenSubtitle { get; }
    public Interaction<string, Uri?> SaveSubtitleAs { get; }
    public Interaction<string, Uri?> ExportSubtitle { get; }
    public Interaction<Unit, Uri?> OpenProject { get; }
    public Interaction<Unit, Uri?> OpenFolderAsProject { get; }
    public Interaction<string, Uri?> SaveProjectAs { get; }
    public Interaction<Unit, Uri?> AttachReferenceFile { get; }

    // Edit
    public Interaction<SearchDialogViewModel, Unit> ShowSearchDialog { get; }
    public Interaction<SpellcheckDialogViewModel, Unit> ShowSpellcheckDialog { get; }

    // Subtitle
    public Interaction<StylesManagerWindowViewModel, Unit> ShowStylesManager { get; }

    // Project
    // Timing
    public Interaction<ShiftTimesDialogViewModel, Unit> ShowShiftTimesDialog { get; }

    // Video
    public Interaction<Unit, Uri?> OpenVideo { get; }
    public Interaction<JumpDialogViewModel, JumpDialogClosedMessage?> ShowJumpDialog { get; }

    // Scripts
    public Interaction<PkgManWindowViewModel, Unit> ShowPackageManager { get; }
    public Interaction<PlaygroundWindowViewModel, Unit> ShowPlaygroundWindow { get; }

    // Help
    public Interaction<LogWindowViewModel, Unit> ShowLogWindow { get; }
    public Interaction<AboutWindowViewModel, Unit> ShowAboutWindow { get; }
    public Interaction<KeybindsWindowViewModel, Unit> ShowKeybindsWindow { get; }

    // Other
    public Interaction<InstallDictionaryDialogViewModel, Unit> ShowInstallDictionaryDialog { get; }
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

    [KeybindTarget("ameko.project.open")]
    public ICommand OpenProjectCommand { get; }
    public ICommand OpenProjectNoGuiCommand { get; }

    [KeybindTarget("ameko.project.openFolder")]
    public ICommand OpenFolderAsProjectCommand { get; }

    [KeybindTarget("ameko.project.save")]
    public ICommand SaveProjectCommand { get; }

    [KeybindTarget("ameko.workspace.close", "Ctrl+W")]
    public ICommand CloseTabCommand { get; }

    [KeybindTarget("ameko.project.close")]
    public ICommand CloseProjectCommand { get; }

    [KeybindTarget("ameko.application.quit", "Ctrl+Q")]
    public ICommand QuitCommand { get; }

    // Edit
    [KeybindTarget("ameko.document.undo", "Ctrl+Z")]
    public ICommand UndoCommand { get; }

    [KeybindTarget("ameko.document.redo", "Ctrl+Y")]
    public ICommand RedoCommand { get; }

    [KeybindTarget("ameko.document.search", "Ctrl+F")]
    public ICommand ShowSearchDialogCommand { get; }

    [KeybindTarget("ameko.document.spellcheck", "F7")]
    public ICommand ShowSpellcheckDialogCommand { get; }

    // Subtitle
    [KeybindTarget("ameko.stylesManager.show")]
    public ICommand ShowStylesManagerCommand { get; }

    [KeybindTarget("ameko.reference.attach", KeybindContext.Global)]
    public ICommand AttachReferenceFileCommand { get; }

    [KeybindTarget("ameko.reference.detach", KeybindContext.Global)]
    public ICommand DetachReferenceFileCommand { get; }

    // Project
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

    [KeybindTarget("ameko.playground.show")]
    public ICommand ShowPlaygroundWindowCommand { get; }

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
    public ICommand RemoveDocumentFromProjectCommand { get; }
    public ICommand RemoveDirectoryFromProjectCommand { get; }
    public ICommand OpenDocumentCommand { get; }
    public ICommand RenameDocumentCommand { get; }
    public ICommand RenameDirectoryCommand { get; }
    public ICommand AttachReferenceFileNoGuiCommand { get; }
    public ICommand CheckSpellcheckDictionaryCommand { get; }
    #endregion

    public IProjectProvider ProjectProvider { get; }
    public IKeybindService KeybindService { get; }
    public IIoService IoService { get; }
    public GitToolboxViewModel GitToolboxViewModel { get; }

    public ObservableCollection<TemplatedControl> ScriptMenuItems { get; }
    public ObservableCollection<TemplatedControl> LayoutMenuItems { get; }

    public string CurrentMessage
    {
        get => _currentMessage;
        set => this.RaiseAndSetIfChanged(ref _currentMessage, value);
    }

    /// <summary>
    /// WindowSection title
    /// </summary>
    public string WindowTitle { get; } = $"Ameko {VersionService.FullLabel}";

    /// <summary>
    /// Set the <see cref="Project.WorkingSpace"/> to the selected workspace, opening it if needed
    /// </summary>
    /// <param name="workspaceId">ID to open</param>
    public void TryLoadReferenced(int workspaceId)
    {
        var wsp = ProjectProvider.Current.LoadedWorkspaces.FirstOrDefault(w => w.Id == workspaceId);
        if (wsp is not null)
        {
            ProjectProvider.Current.WorkingSpace = wsp;
            return;
        }

        ProjectProvider.Current.OpenDocument(workspaceId);
    }

    private void GenerateScriptsMenu()
    {
        Logger.Debug("Regenerating scripts menu...");
        var menuItems = ScriptMenuService.GenerateMenuItemSource(
            ScriptService.Scripts,
            ExecuteScriptCommand
        );
        ScriptMenuItems.Clear();
        ScriptMenuItems.AddRange(menuItems);
        if (menuItems.Count > 0)
            ScriptMenuItems.Add(new Separator());
        ScriptMenuItems.Add(
            ScriptMenuService.GeneratePlaygroundMenuItem(ShowPlaygroundWindowCommand)
        );
        ScriptMenuItems.Add(new Separator());
        ScriptMenuItems.Add(ScriptMenuService.GenerateReloadMenuItem(ReloadScriptsCommand));
        ScriptMenuItems.Add(ScriptMenuService.GeneratePkgManMenuItem(ShowPackageManagerCommand));
        Logger.Debug("Done!");
    }

    private void GenerateLayoutsMenu()
    {
        Logger.Debug("Regenerating layouts menu...");
        var menuItems = LayoutMenuService.GenerateMenuItemSource(
            LayoutProvider.Layouts,
            SelectLayoutCommand
        );
        LayoutMenuItems.Clear();
        LayoutMenuItems.AddRange(menuItems);
        if (menuItems.Count > 0)
            LayoutMenuItems.Add(new Separator());
        LayoutMenuItems.Add(LayoutMenuService.GenerateReloadMenuItem(RefreshLayoutsCommand));
        Logger.Debug("Done!");
    }

    public MainWindowViewModel(
        IServiceProvider serviceProvider,
        IFileSystem fileSystem,
        IIoService ioService,
        ILayoutProvider layoutProvider,
        IProjectProvider projectProvider,
        IStylesManagerFactory stylesManagerFactory,
        IScriptService scriptService,
        IKeybindService keybindService,
        IMessageService messageService,
        IMessageBoxService messageBoxService,
        ISpellcheckService spellCheckService,
        IDictionaryService dictionaryService,
        IConfiguration configuration,
        GitToolboxViewModel gitToolboxViewModel
    )
    {
        _serviceProvider = serviceProvider;
        _fileSystem = fileSystem;
        IoService = ioService;
        LayoutProvider = layoutProvider;
        ProjectProvider = projectProvider;
        _stylesManagerFactory = stylesManagerFactory;
        ScriptService = scriptService;
        KeybindService = keybindService;
        _messageService = messageService;
        _messageBoxService = messageBoxService;
        _spellcheckService = spellCheckService;
        _dictionaryService = dictionaryService;
        _configuration = configuration;
        GitToolboxViewModel = gitToolboxViewModel;

        #region Interactions
        // File
        OpenSubtitle = new Interaction<Unit, Uri[]>();
        SaveSubtitleAs = new Interaction<string, Uri?>();
        ExportSubtitle = new Interaction<string, Uri?>();
        OpenProject = new Interaction<Unit, Uri?>();
        OpenFolderAsProject = new Interaction<Unit, Uri?>();
        SaveProjectAs = new Interaction<string, Uri?>();
        // Edit
        ShowSearchDialog = new Interaction<SearchDialogViewModel, Unit>();
        ShowSpellcheckDialog = new Interaction<SpellcheckDialogViewModel, Unit>();
        // Subtitle
        ShowStylesManager = new Interaction<StylesManagerWindowViewModel, Unit>();
        AttachReferenceFile = new Interaction<Unit, Uri?>();
        // Project
        // Video
        OpenVideo = new Interaction<Unit, Uri?>();
        ShowJumpDialog = new Interaction<JumpDialogViewModel, JumpDialogClosedMessage?>();
        // Timing
        ShowShiftTimesDialog = new Interaction<ShiftTimesDialogViewModel, Unit>();
        // Scripts
        ShowPackageManager = new Interaction<PkgManWindowViewModel, Unit>();
        ShowPlaygroundWindow = new Interaction<PlaygroundWindowViewModel, Unit>();
        // Help
        ShowLogWindow = new Interaction<LogWindowViewModel, Unit>();
        ShowAboutWindow = new Interaction<AboutWindowViewModel, Unit>();
        ShowKeybindsWindow = new Interaction<KeybindsWindowViewModel, Unit>();
        // Other
        ShowInstallDictionaryDialog = new Interaction<InstallDictionaryDialogViewModel, Unit>();
        #endregion

        #region Commands
        // File
        NewCommand = CreateNewCommand();
        OpenSubtitleCommand = CreateOpenSubtitleCommand();
        OpenSubtitleNoGuiCommand = CreateOpenSubtitleNoGuiCommand();
        SaveSubtitleCommand = CreateSaveSubtitleCommand();
        SaveSubtitleAsCommand = CreateSaveSubtitleAsCommand();
        ExportSubtitleCommand = CreateExportSubtitleCommand();
        OpenProjectCommand = CreateOpenProjectCommand();
        OpenProjectNoGuiCommand = CreateOpenProjectNoGuiCommand();
        OpenFolderAsProjectCommand = CreateOpenFolderAsProjectCommand();
        SaveProjectCommand = CreateSaveProjectCommand();
        CloseTabCommand = CreateCloseTabCommand();
        CloseProjectCommand = CreateCloseProjectCommand();
        QuitCommand = CreateQuitCommand();
        // Edit
        UndoCommand = CreateUndoCommand();
        RedoCommand = CreateRedoCommand();
        ShowSearchDialogCommand = CreateShowSearchDialogCommand();
        ShowSpellcheckDialogCommand = CreateShowSpellcheckDialogCommand();
        // Subtitle
        ShowStylesManagerCommand = CreateShowStylesManagerCommand();
        AttachReferenceFileCommand = CreateAttachReferenceFileCommand();
        DetachReferenceFileCommand = CreateDetachReferenceFileCommand();
        // Project
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
        ShowPlaygroundWindowCommand = CreateShowPlaygroundCommand();
        // Layouts
        SelectLayoutCommand = CreateSelectLayoutCommand();
        RefreshLayoutsCommand = CreateRefreshLayoutsCommand();
        // Help
        ShowLogWindowCommand = CreateShowLogWindowCommand();
        ShowAboutWindowCommand = CreateShowAboutWindowCommand();
        ShowKeybindsWindowCommand = CreateShowKeybindsWindowCommand();
        // Other
        RemoveDocumentFromProjectCommand = CreateRemoveDocumentFromProjectCommand();
        RemoveDirectoryFromProjectCommand = CreateRemoveDirectoryFromProjectCommand();
        OpenDocumentCommand = CreateOpenDocumentCommand();
        AttachReferenceFileNoGuiCommand = CreateAttachReferenceFileNoGuiCommand();
        RenameDocumentCommand = CreateRenameDocumentCommand();
        RenameDirectoryCommand = CreateRenameDirectoryCommand();
        CheckSpellcheckDictionaryCommand = CreateCheckSpellcheckDictionaryCommand();
        #endregion

        ScriptMenuItems = [];
        ScriptService.OnReload += (_, _) => GenerateScriptsMenu();

        LayoutMenuItems = [];
        LayoutProvider.OnLayoutChanged += (_, _) => GenerateLayoutsMenu();
        GenerateLayoutsMenu();

        _messageService.MessageReady += (_, msg) => CurrentMessage = msg.Content;
        _messageService.QueueDrained += (_, _) => CurrentMessage = string.Empty;
    }
}
