using Ameko.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using DynamicData;
using Holo;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Ameko.ViewModels;

public class MainViewModel : ViewModelBase
{
    private int selectedTabIndex;
    public Workspace Workspace { get; private set; }

    public string WindowTitle { get; } = $"Ameko {AmekoService.VERSION_BUG}";
    public Interaction<AboutWindowViewModel, AboutWindowViewModel?> ShowAboutDialog { get; }
    public Interaction<StylesManagerViewModel, StylesManagerViewModel?> ShowStylesManager { get; }
    public Interaction<MainViewModel, Uri?> ShowOpenFileDialog { get; }
    public Interaction<FileWrapper, Uri?> ShowSaveAsFileDialog { get; }
    public Interaction<FileWrapper, Uri?> ShowExportFileDialog { get; }
    public Interaction<MainViewModel, Uri?> ShowOpenWorkspaceDialog { get; }
    public Interaction<Workspace, Uri?> ShowSaveAsWorkspaceDialog { get; }
    public Interaction<MainViewModel, Uri?> ShowOpenVideoDialog { get; }
    public Interaction<JumpWindowViewModel, Unit> ShowJumpDialog { get; }
    public Interaction<SearchWindowViewModel, string?> ShowSearchDialog { get; }
    public Interaction<ShiftTimesWindowViewModel, Unit> ShowShiftTimesDialog { get; }
    public Interaction<ScriptPropertiesWindowViewModel, Unit> ShowScriptPropertiesDialog { get; }
    public Interaction<DependencyControlWindowViewModel, Unit> ShowDependencyControlWindow { get; }
    public Interaction<ConfigWindowViewModel, Unit> ShowConfigWindow { get; }
    public Interaction<WorkspaceConfigWindowViewModel, Unit> ShowWorkspaceConfigWindow { get; }
    public Interaction<KeybindsWindowViewModel, Unit> ShowKeybindsWindow { get; }
    public Interaction<LogWindowViewModel, Unit> ShowLogsWindow { get; }
    public Interaction<ScriptLogWindowViewModel, Unit> ShowScriptLogsDialog { get; }
    public Interaction<Unit, Unit> CloseScriptLogsDialog { get; }
    public Interaction<FreeformWindowViewModel, Unit> ShowFreeformPlayground { get; }
    public ICommand ShowAboutDialogCommand { get; }
    public ICommand ShowStylesManagerCommand { get; }
    public ICommand NewFileCommand { get; }
    public ICommand ShowOpenFileDialogCommand { get; }
    public ICommand ShowSaveFileDialogCommand { get; }
    public ICommand ShowSaveAsFileDialogCommand { get; }
    public ICommand ShowExportFileDialogCommand { get; }
    public ICommand ShowOpenWorkspaceDialogCommand { get; }
    public ICommand ShowSaveWorkspaceDialogCommand { get; }
    public ICommand ShowOpenVideoDialogCommand { get; }
    public ICommand ShowJumpDialogCommand { get; }
    public ICommand CloseTabCommand { get; }
    public ICommand RemoveFromWorkspaceCommand { get; }
    public ICommand ActivateScriptCommand { get; }
    public ICommand ReloadScriptsCommand { get; }
    public ICommand ActivateLayoutCommand { get; }
    public ICommand ReloadLayoutsCommand { get; }
    public ICommand QuitCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    public ICommand ShowSearchDialogCommand { get; }
    public ICommand ShowShiftTimesDialogCommand { get; }
    public ICommand ShowScriptPropertiesDialogCommand { get; }
    public ICommand ShowDependencyControlWindowCommand { get; }
    public ICommand ShowConfigWindowCommand { get; }
    public ICommand ShowWorkspaceConfigWindowCommand { get; }
    public ICommand ShowKeybindsWindowCommand { get; }
    public ICommand ShowLogsWindowCommand { get; }
    public ICommand ShowScriptLogsDialogCommand { get; }
    public ICommand CloseScriptLogsDialogCommand { get; }
    public ICommand ShowFreeformPlaygroundCommand { get; }

    public ObservableCollection<TabItemViewModel> Tabs { get; set; }
    public ObservableCollection<TemplatedControl> ScriptMenuItems { get; }
    public ObservableCollection<TemplatedControl> LayoutMenuItems { get; }

    public int SelectedTabIndex
    {
        get => selectedTabIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedTabIndex, value);
            if (value >= 0)
            {
                var tab = Tabs[value];
                HoloContext.Instance.Workspace.WorkingIndex = tab.ID;
            }
        }
    }

    public void TryLoadReferenced(int fileId)
    {
        // Is the file already open?
        if (Tabs.Where(t => t.ID == fileId).Any())
        {
            SelectedTabIndex = Tabs.IndexOf(Tabs.Where(t => t.ID == fileId).Single());
            return;
        }
        // Open the file
        HoloContext.Instance.Workspace.OpenFileFromWorkspace(fileId);
    }

    private void UpdateLoadedTabsCallback(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (FileWrapper ni in e.NewItems)
            {
                Tabs?.Add(new TabItemViewModel(ni.Title, ni));
            }
        if (e.OldItems != null)
            foreach (FileWrapper oi in e.OldItems)
            {
                Tabs?.Remove(Tabs.Where(t => t.ID == oi.ID).Single());
            }
    }

    private void GenerateLayoutsMenu()
    {
        LayoutMenuItems.Clear();
        var reloadSvg = new Avalonia.Svg.Skia.Svg(new Uri("avares://Ameko/Assets/B5/arrow-clockwise.svg")) { Path = new Uri("avares://Ameko/Assets/B5/arrow-clockwise.svg").LocalPath };

        LayoutMenuItems.AddRange(LayoutMenuService.GenerateLayoutMenuItemSource(ActivateLayoutCommand));

        LayoutMenuItems.Add(new Separator());
        LayoutMenuItems.Add(new MenuItem
        {
            Header = "_Reload Layouts",
            Command = ReloadLayoutsCommand,
            Icon = reloadSvg
        });
    }

    private void GenerateScriptsMenu()
    {
        ScriptMenuItems.Clear();
        var reloadSvg = new Avalonia.Svg.Skia.Svg(new Uri("avares://Ameko/Assets/B5/arrow-clockwise.svg")) { Path = new Uri("avares://Ameko/Assets/B5/arrow-clockwise.svg").LocalPath };
        var freeSvg = new Avalonia.Svg.Skia.Svg(new Uri("avares://Ameko/Assets/B5/cone-striped.svg")) { Path = new Uri("avares://Ameko/Assets/B5/cone-striped.svg").LocalPath };
        var dcSvg = new Avalonia.Svg.Skia.Svg(new Uri("avares://Ameko/Assets/B5/globe.svg")) { Path = new Uri("avares://Ameko/Assets/B5/globe.svg").LocalPath };

        ScriptMenuItems.AddRange(ScriptMenuService.GenerateScriptMenuItemSource(ActivateScriptCommand));

        ScriptMenuItems.Add(new Separator());
        ScriptMenuItems.Add(new MenuItem
        {
            Header = "_Freeform Playground",
            Command = ShowFreeformPlaygroundCommand,
            Icon = freeSvg
        });
        ScriptMenuItems.Add(new MenuItem
        {
            Header = "_Reload Scripts",
            Command = ReloadScriptsCommand,
            Icon = reloadSvg
        });
        ScriptMenuItems.Add(new MenuItem
        {
            Header = "_Dependency Control",
            Command = ShowDependencyControlWindowCommand,
            Icon = dcSvg
        });
    }

    public MainViewModel()
    {
        Workspace = HoloContext.Instance.Workspace;
        ShowAboutDialog = new Interaction<AboutWindowViewModel, AboutWindowViewModel?>();
        ShowStylesManager = new Interaction<StylesManagerViewModel, StylesManagerViewModel?>();
        ShowOpenFileDialog = new Interaction<MainViewModel, Uri?>();
        ShowSaveAsFileDialog = new Interaction<FileWrapper, Uri?>();
        ShowExportFileDialog = new Interaction<FileWrapper, Uri?>();
        ShowOpenWorkspaceDialog = new Interaction<MainViewModel, Uri?>();
        ShowSaveAsWorkspaceDialog = new Interaction<Workspace, Uri?>();
        ShowOpenVideoDialog = new Interaction<MainViewModel, Uri?>();
        ShowJumpDialog = new Interaction<JumpWindowViewModel, Unit>();
        ShowSearchDialog = new Interaction<SearchWindowViewModel, string?>();
        ShowShiftTimesDialog = new Interaction<ShiftTimesWindowViewModel, Unit>();
        ShowScriptPropertiesDialog = new Interaction<ScriptPropertiesWindowViewModel, Unit>();
        ShowDependencyControlWindow = new Interaction<DependencyControlWindowViewModel, Unit>();
        ShowConfigWindow = new Interaction<ConfigWindowViewModel, Unit>();
        ShowWorkspaceConfigWindow = new Interaction<WorkspaceConfigWindowViewModel, Unit>();
        ShowKeybindsWindow = new Interaction<KeybindsWindowViewModel, Unit>();
        ShowLogsWindow = new Interaction<LogWindowViewModel, Unit>();
        ShowScriptLogsDialog = new Interaction<ScriptLogWindowViewModel, Unit>();
        CloseScriptLogsDialog = new Interaction<Unit, Unit>();
        ShowFreeformPlayground = new Interaction<FreeformWindowViewModel, Unit>();

        ShowAboutDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplayAboutBox(ShowAboutDialog));
        ShowStylesManagerCommand = ReactiveCommand.Create(() => IOCommandService.DisplayStylesManager(ShowStylesManager, this));
        ShowOpenFileDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplayOpenSubtitleFileDialog(ShowOpenFileDialog, this));
        ShowSaveFileDialogCommand = ReactiveCommand.Create(() => IOCommandService.SaveOrDisplaySaveAsDialog(ShowSaveAsFileDialog));
        ShowSaveAsFileDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplaySaveAsDialog(ShowSaveAsFileDialog));
        ShowExportFileDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplayExportDialog(ShowExportFileDialog));
        ShowSaveWorkspaceDialogCommand = ReactiveCommand.Create(() => IOCommandService.WorkspaceSaveOrDisplaySaveAsDialog(ShowSaveAsWorkspaceDialog));
        ShowOpenWorkspaceDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplayWorkspaceOpenDialog(ShowOpenWorkspaceDialog, this));
        ShowOpenVideoDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplayVideoOpenDialog(ShowOpenVideoDialog, this));

        UndoCommand = ReactiveCommand.Create(HoloContext.Instance.Workspace.WorkingFile.Undo);
        RedoCommand = ReactiveCommand.Create(HoloContext.Instance.Workspace.WorkingFile.Redo);

        NewFileCommand = ReactiveCommand.Create(() =>
        {
            Workspace.AddFileToWorkspace();
        });

        CloseTabCommand = ReactiveCommand.Create<int>(async (fileId) => await IOCommandService.CloseTab(fileId, ShowSaveAsFileDialog));

        RemoveFromWorkspaceCommand = ReactiveCommand.Create<int>((int fileId) =>
        {
            HoloContext.Instance.Workspace.RemoveFileFromWorkspace(fileId);
        });

        QuitCommand = ReactiveCommand.Create(() =>
        {
            // TODO saving and stuff
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        });

        ShowSearchDialogCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new SearchWindowViewModel(this);
            await ShowSearchDialog.Handle(vm);
        });

        ShowShiftTimesDialogCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new ShiftTimesWindowViewModel();
            await ShowShiftTimesDialog.Handle(vm);
        });

        ShowScriptPropertiesDialogCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new ScriptPropertiesWindowViewModel(HoloContext.Instance.Workspace.WorkingFile.File);
            await ShowScriptPropertiesDialog.Handle(vm);
        });

        ShowDependencyControlWindowCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new DependencyControlWindowViewModel();
            await ShowDependencyControlWindow.Handle(vm);
        });

        ShowConfigWindowCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new ConfigWindowViewModel();
            await ShowConfigWindow.Handle(vm);
        });

        ShowWorkspaceConfigWindowCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new WorkspaceConfigWindowViewModel();
            await ShowWorkspaceConfigWindow.Handle(vm);
        });

        ShowKeybindsWindowCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new KeybindsWindowViewModel();
            await ShowKeybindsWindow.Handle(vm);
        });

        ShowLogsWindowCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new LogWindowViewModel();
            await ShowLogsWindow.Handle(vm);
        });

        ShowScriptLogsDialogCommand = ReactiveCommand.Create(async (ScriptLogWindowViewModel lvm) =>
        {
            await ShowScriptLogsDialog.Handle(lvm);
        });

        CloseScriptLogsDialogCommand = ReactiveCommand.Create(async () =>
        {
            await CloseScriptLogsDialog.Handle(Unit.Default);
        });

        ActivateScriptCommand = ReactiveCommand.Create<string>(async (string scriptName) =>
        {
            await ScriptService.Instance.Execute(scriptName);
        });

        ReloadScriptsCommand = ReactiveCommand.Create(() =>
        {
            ScriptService.Instance.Reload(true);
        });

        ActivateLayoutCommand = ReactiveCommand.Create<string>(LayoutService.Instance.SelectLayout);

        ReloadLayoutsCommand = ReactiveCommand.Create(() =>
        {
            LayoutService.Instance.Reload(true);
        });

        ShowFreeformPlaygroundCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new FreeformWindowViewModel();
            await ShowFreeformPlayground.Handle(vm);
        });

        ShowJumpDialogCommand = ReactiveCommand.Create(async () =>
        {
            if (HoloContext.Instance.Workspace.WorkingFile.AVManager.IsVideoLoaded)
            {
                var vm = new JumpWindowViewModel();
                await ShowJumpDialog.Handle(vm);
            }
        });

        Tabs = new ObservableCollection<TabItemViewModel>(HoloContext.Instance.Workspace.Files.Select(f => new TabItemViewModel(f.Title, f)));

        ScriptMenuItems = [];
        GenerateScriptsMenu();

        LayoutMenuItems = [];
        GenerateLayoutsMenu();

        HoloContext.Instance.Workspace.Files.CollectionChanged += UpdateLoadedTabsCallback;
        ScriptService.Instance.MainViewModel = this;
        ScriptService.Instance.LoadedScripts.CollectionChanged += (o, e) =>
        {
            GenerateScriptsMenu();
        };
        LayoutService.Instance.LoadedLayouts.CollectionChanged += (o, e) =>
        {
            GenerateLayoutsMenu();
        };
    }
}
