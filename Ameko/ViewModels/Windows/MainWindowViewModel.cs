// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Ameko.Messages;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Controls;
using Ameko.ViewModels.Dialogs;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using DynamicData;
using Holo;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.Media.Providers;
using Holo.Providers;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IViewModelFactory _vmFactory;
    private readonly IMessageService _messageService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISpellcheckService _spellcheckService;
    private readonly IDictionaryService _dictionaryService;
    private readonly ITabFactory _tabFactory;
    private readonly ILogger _logger;

    public IConfiguration Configuration { get; }
    public IPersistence Persistence { get; }
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
    public Interaction<ProjectConfigDialogViewModel, Unit> ShowProjectConfigDialog { get; }

    // Timing
    public Interaction<ShiftTimesDialogViewModel, Unit> ShowShiftTimesDialog { get; }

    // Video
    public Interaction<Unit, Uri?> OpenVideo { get; }
    public Interaction<JumpDialogViewModel, JumpDialogClosedMessage?> ShowJumpDialog { get; }

    // Audio
    public Interaction<Unit, Uri?> OpenAudio { get; }

    // Scripts
    public Interaction<PkgManWindowViewModel, Unit> ShowPackageManager { get; }
    public Interaction<PlaygroundWindowViewModel, Unit> ShowPlaygroundWindow { get; }

    // Help
    public Interaction<HelpWindowViewModel, Unit> ShowHelpWindow { get; }
    public Interaction<LogWindowViewModel, Unit> ShowLogWindow { get; }
    public Interaction<AboutWindowViewModel, Unit> ShowAboutWindow { get; }
    public Interaction<ConfigDialogViewModel, Unit> ShowConfigDialog { get; }
    public Interaction<KeybindsDialogViewModel, Unit> ShowKeybindsDialog { get; }
    public Interaction<Unit, Unit> OpenIssueTracker { get; }

    // Other
    public Interaction<InstallDictionaryDialogViewModel, Unit> ShowInstallDictionaryDialog { get; }
    #endregion

    #region Commands
    // File
    [KeybindTarget("ameko.document.new", "Ctrl+N", KeybindContext.Global)]
    public ICommand NewCommand { get; }

    [KeybindTarget("ameko.document.open", "Ctrl+O", KeybindContext.Global)]
    public ICommand OpenSubtitleCommand { get; }
    public ICommand OpenSubtitleNoGuiCommand { get; }
    public ICommand OpenSubtitlesNoGuiCommand { get; }

    [KeybindTarget("ameko.document.save", "Ctrl+S", KeybindContext.Global)]
    public ICommand SaveSubtitleCommand { get; }

    [KeybindTarget("ameko.document.saveAs", "Ctrl+Shift+S", KeybindContext.Global)]
    public ICommand SaveSubtitleAsCommand { get; }

    [KeybindTarget("ameko.document.export", KeybindContext.Global)]
    public ICommand ExportSubtitleCommand { get; }
    public ICommand ClearRecentSubtitlesCommand { get; }

    [KeybindTarget("ameko.project.open", KeybindContext.Global)]
    public ICommand OpenProjectCommand { get; }
    public ICommand OpenProjectNoGuiCommand { get; }

    [KeybindTarget("ameko.project.openFolder", KeybindContext.Global)]
    public ICommand OpenFolderAsProjectCommand { get; }

    [KeybindTarget("ameko.project.save", KeybindContext.Global)]
    public ICommand SaveProjectCommand { get; }
    public ICommand ClearRecentProjectsCommand { get; }

    [KeybindTarget("ameko.workspace.close", "Ctrl+W", KeybindContext.Global)]
    public ICommand CloseTabCommand { get; }

    [KeybindTarget("ameko.project.close", KeybindContext.Global)]
    public ICommand CloseProjectCommand { get; }

    [KeybindTarget("ameko.application.quit", "Ctrl+Q", KeybindContext.Global)]
    public ICommand QuitCommand { get; }

    // Edit
    [KeybindTarget("ameko.document.undo", "Ctrl+Z", KeybindContext.Global)]
    public ICommand UndoCommand { get; }

    [KeybindTarget("ameko.document.redo", "Ctrl+Y", KeybindContext.Global)]
    public ICommand RedoCommand { get; }

    [KeybindTarget("ameko.document.search", "Ctrl+F", KeybindContext.Global)]
    public ICommand ShowSearchDialogCommand { get; }

    [KeybindTarget("ameko.document.spellcheck", "F7", KeybindContext.Global)]
    public ICommand ShowSpellcheckDialogCommand { get; }

    // Subtitle
    [KeybindTarget("ameko.stylesManager.show", KeybindContext.Global)]
    public ICommand ShowStylesManagerCommand { get; }

    [KeybindTarget("ameko.reference.attach", KeybindContext.Global)]
    public ICommand AttachReferenceFileCommand { get; }

    [KeybindTarget("ameko.reference.detach", KeybindContext.Global)]
    public ICommand DetachReferenceFileCommand { get; }

    // Project
    [KeybindTarget("ameko.project.config.show", KeybindContext.Global)]
    public ICommand ShowProjectConfigDialogCommand { get; }

    // Timing
    [KeybindTarget("ameko.document.shiftTimes", "Ctrl+I", KeybindContext.Global)]
    public ICommand ShowShiftTimesDialogCommand { get; }

    // Video
    [KeybindTarget("ameko.video.open", KeybindContext.Global)]
    public ICommand OpenVideoCommand { get; }
    public ICommand OpenVideoNoGuiCommand { get; }

    [KeybindTarget("ameko.video.close", KeybindContext.Global)]
    public ICommand CloseVideoCommand { get; }

    [KeybindTarget("ameko.video.jump", "Ctrl+G", KeybindContext.Global)]
    public ICommand ShowJumpDialogCommand { get; }

    [KeybindTarget("ameko.audio.open", KeybindContext.Global)]
    public ICommand OpenAudioCommand { get; }

    [KeybindTarget("ameko.audio.close", KeybindContext.Global)]
    public ICommand CloseAudioCommand { get; }

    [KeybindTarget("ameko.audio.changeTracks", KeybindContext.Global)]
    public ICommand ChangeTracksCommand { get; }

    // Scripts
    // Command execution doesn't get a keybind. So sad :(
    public ICommand ExecuteScriptCommand { get; }

    [KeybindTarget("ameko.scripts.reload", KeybindContext.Global)]
    public ICommand ReloadScriptsCommand { get; }

    [KeybindTarget("ameko.pkgMan.show", KeybindContext.Global)]
    public ICommand ShowPackageManagerCommand { get; }

    [KeybindTarget("ameko.playground.show", KeybindContext.Global)]
    public ICommand ShowPlaygroundWindowCommand { get; }

    // Layouts
    public ICommand SelectLayoutCommand { get; }
    public ICommand RefreshLayoutsCommand { get; }

    // Help
    [KeybindTarget("ameko.help.show", "F1", KeybindContext.Global)]
    public ICommand ShowHelpWindowCommand { get; }

    [KeybindTarget("ameko.logs.show", "Ctrl+L", KeybindContext.Global)]
    public ICommand ShowLogWindowCommand { get; }

    [KeybindTarget("ameko.about.show", "Shift+F1", KeybindContext.Global)]
    public ICommand ShowAboutWindowCommand { get; }

    [KeybindTarget("ameko.config.show", "Ctrl+,", KeybindContext.Global)]
    public ICommand ShowConfigDialogCommand { get; }

    [KeybindTarget("ameko.keybinds.show", KeybindContext.Global)]
    public ICommand ShowKeybindsDialogCommand { get; }

    [KeybindTarget("ameko.issues.open", KeybindContext.Global)]
    public ICommand OpenIssueTrackerCommand { get; }

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
    public IIoService IoService { get; }
    public GitToolboxViewModel GitToolboxViewModel { get; }

    public ObservableCollection<TemplatedControl> ScriptMenuItems { get; }
    public ObservableCollection<TemplatedControl> LayoutMenuItems { get; }
    public ObservableCollection<TemplatedControl> RecentDocumentMenuItems { get; }
    public ObservableCollection<TemplatedControl> RecentProjectMenuItems { get; }

    public string CurrentMessage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = I18N.Resources.Message_Welcome;

    /// <summary>
    /// WindowSection title
    /// </summary>
    public string WindowTitle { get; } = $"Ameko {VersionService.FullLabel}";

    /// <summary>
    /// Set the <see cref="Project.WorkingSpace"/> to the selected workspace, opening it if needed
    /// </summary>
    /// <param name="workspaceId">ID to open</param>
    public async Task TryLoadReferenced(int workspaceId)
    {
        var wsp = ProjectProvider.Current.LoadedWorkspaces.FirstOrDefault(w => w.Id == workspaceId);
        if (wsp is not null)
        {
            ProjectProvider.Current.WorkingSpace = wsp;
            return;
        }

        wsp = ProjectProvider.Current.OpenDocument(workspaceId);
        if (wsp is null)
            return;

        ISourceProvider.IndexingProgressCallback? callback = null;
        if (_tabFactory.TryGetViewModel(wsp, out var tabVm))
        {
            callback = (current, total) =>
            {
                var progress = (double)current / total;
                Dispatcher.UIThread.Post(() => tabVm.IndexingProgress = progress);
            };
        }

        try
        {
            Dispatcher.UIThread.Post(() => tabVm?.IsIndexing = true);
            await IoService.ProcessProjectGarbageAsync(wsp, ProjectProvider.Current, callback);
        }
        finally
        {
            Dispatcher.UIThread.Post(() => tabVm?.IsIndexing = false);
        }
    }

    private void GenerateScriptsMenu()
    {
        _logger.LogDebug("Regenerating scripts menu...");
        var menuItems = ScriptMenuService.GenerateMenuItemSource(
            ScriptService.Scripts,
            Configuration.ScriptMenuOverrides,
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
        _logger.LogDebug("Done!");
    }

    private void GenerateLayoutsMenu()
    {
        _logger.LogDebug("Regenerating layouts menu...");
        var menuItems = LayoutMenuService.GenerateMenuItemSource(
            LayoutProvider.Layouts,
            SelectLayoutCommand
        );
        LayoutMenuItems.Clear();
        LayoutMenuItems.AddRange(menuItems);
        if (menuItems.Count > 0)
            LayoutMenuItems.Add(new Separator());
        LayoutMenuItems.Add(LayoutMenuService.GenerateReloadMenuItem(RefreshLayoutsCommand));
        _logger.LogDebug("Done!");
    }

    private void GenerateRecentsMenus()
    {
        _logger.LogDebug("Regenerating recents menus...");
        var subsMenuItems = RecentsMenuService.GenerateMenuItemSource(
            Persistence.RecentDocuments,
            OpenSubtitleNoGuiCommand
        );
        var prjMenuItems = RecentsMenuService.GenerateMenuItemSource(
            Persistence.RecentProjects,
            OpenProjectNoGuiCommand
        );

        RecentDocumentMenuItems.Clear();
        RecentProjectMenuItems.Clear();
        RecentDocumentMenuItems.AddRange(subsMenuItems);
        RecentProjectMenuItems.AddRange(prjMenuItems);
        if (RecentDocumentMenuItems.Count > 0)
            RecentDocumentMenuItems.Add(new Separator());
        if (RecentProjectMenuItems.Count > 0)
            RecentProjectMenuItems.Add(new Separator());
        RecentDocumentMenuItems.Add(
            RecentsMenuService.GenerateClearMenuItem(ClearRecentSubtitlesCommand)
        );
        RecentProjectMenuItems.Add(
            RecentsMenuService.GenerateClearMenuItem(ClearRecentProjectsCommand)
        );
        _logger.LogDebug("Done!");
    }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IConfiguration configuration,
        IPersistence persistence,
        IIoService ioService,
        ILayoutProvider layoutProvider,
        IProjectProvider projectProvider,
        IScriptService scriptService,
        IDictionaryService dictionaryService,
        IMessageBoxService messageBoxService,
        IMessageService messageService,
        ISpellcheckService spellCheckService,
        ITabFactory tabFactory,
        IViewModelFactory vmFactory
    )
    {
        _logger = logger;

        Configuration = configuration;
        Persistence = persistence;
        IoService = ioService;
        LayoutProvider = layoutProvider;
        ProjectProvider = projectProvider;
        ScriptService = scriptService;

        _dictionaryService = dictionaryService;
        _messageBoxService = messageBoxService;
        _messageService = messageService;
        _spellcheckService = spellCheckService;
        _tabFactory = tabFactory;
        _vmFactory = vmFactory;

        GitToolboxViewModel = _vmFactory.Create<GitToolboxViewModel>();

        #region Interactions
        // File
        OpenSubtitle = new Interaction<Unit, Uri[]?>();
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
        ShowProjectConfigDialog = new Interaction<ProjectConfigDialogViewModel, Unit>();
        // Video
        OpenVideo = new Interaction<Unit, Uri?>();
        ShowJumpDialog = new Interaction<JumpDialogViewModel, JumpDialogClosedMessage?>();
        // Audio
        OpenAudio = new Interaction<Unit, Uri?>();
        // Timing
        ShowShiftTimesDialog = new Interaction<ShiftTimesDialogViewModel, Unit>();
        // Scripts
        ShowPackageManager = new Interaction<PkgManWindowViewModel, Unit>();
        ShowPlaygroundWindow = new Interaction<PlaygroundWindowViewModel, Unit>();
        // Help
        ShowHelpWindow = new Interaction<HelpWindowViewModel, Unit>();
        ShowLogWindow = new Interaction<LogWindowViewModel, Unit>();
        ShowAboutWindow = new Interaction<AboutWindowViewModel, Unit>();
        ShowConfigDialog = new Interaction<ConfigDialogViewModel, Unit>();
        ShowKeybindsDialog = new Interaction<KeybindsDialogViewModel, Unit>();
        OpenIssueTracker = new Interaction<Unit, Unit>();
        // Other
        ShowInstallDictionaryDialog = new Interaction<InstallDictionaryDialogViewModel, Unit>();
        #endregion

        #region Commands
        // File
        NewCommand = CreateNewCommand();
        OpenSubtitleCommand = CreateOpenSubtitleCommand();
        OpenSubtitleNoGuiCommand = CreateOpenSubtitleNoGuiCommand();
        OpenSubtitlesNoGuiCommand = CreateOpenSubtitlesNoGuiCommand();
        SaveSubtitleCommand = CreateSaveSubtitleCommand();
        SaveSubtitleAsCommand = CreateSaveSubtitleAsCommand();
        ExportSubtitleCommand = CreateExportSubtitleCommand();
        ClearRecentSubtitlesCommand = CreateClearRecentSubtitlesCommand();
        OpenProjectCommand = CreateOpenProjectCommand();
        OpenProjectNoGuiCommand = CreateOpenProjectNoGuiCommand();
        OpenFolderAsProjectCommand = CreateOpenFolderAsProjectCommand();
        SaveProjectCommand = CreateSaveProjectCommand();
        CloseTabCommand = CreateCloseTabCommand();
        CloseProjectCommand = CreateCloseProjectCommand();
        ClearRecentProjectsCommand = CreateClearRecentProjectsCommand();
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
        ShowProjectConfigDialogCommand = CreateShowProjectConfigDialogCommand();
        // Timing
        ShowShiftTimesDialogCommand = CreateShowShiftTimesDialogCommand();
        // Video
        OpenVideoCommand = CreateOpenVideoCommand();
        OpenVideoNoGuiCommand = CreateOpenVideoNoGuiCommand();
        CloseVideoCommand = CreateCloseVideoCommand();
        ShowJumpDialogCommand = CreateShowJumpDialogCommand();
        // Audio
        OpenAudioCommand = CreateOpenAudioCommand();
        CloseAudioCommand = CreateCloseAudioCommand();
        ChangeTracksCommand = CreateChangeTracksCommand();
        // Scripts
        ExecuteScriptCommand = CreateExecuteScriptCommand();
        ReloadScriptsCommand = CreateReloadScriptsCommand();
        ShowPackageManagerCommand = CreateShowPackageManagerCommand();
        ShowPlaygroundWindowCommand = CreateShowPlaygroundCommand();
        // Layouts
        SelectLayoutCommand = CreateSelectLayoutCommand();
        RefreshLayoutsCommand = CreateRefreshLayoutsCommand();
        // Help
        ShowHelpWindowCommand = CreateShowHelpWindowCommand();
        ShowLogWindowCommand = CreateShowLogWindowCommand();
        ShowAboutWindowCommand = CreateShowAboutWindowCommand();
        ShowConfigDialogCommand = CreateShowConfigDialogCommand();
        ShowKeybindsDialogCommand = CreateShowKeybindsDialogCommand();
        OpenIssueTrackerCommand = CreateOpenIssueTrackerCommand();
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

        RecentDocumentMenuItems = [];
        RecentProjectMenuItems = [];
        Persistence.PropertyChanged += (_, args) =>
        {
            var flag =
                args.PropertyName
                is nameof(Persistence.RecentDocuments)
                    or nameof(Persistence.RecentProjects);
            if (flag)
            {
                GenerateRecentsMenus();
            }
        };
        GenerateRecentsMenus();

        _messageService.MessageReady += (_, msg) => CurrentMessage = msg.Content;
        _messageService.QueueDrained += (_, _) => CurrentMessage = string.Empty;
    }
}
