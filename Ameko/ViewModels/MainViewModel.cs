﻿using Ameko.DataModels;
using Ameko.Services;
using AssCS.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using DynamicData;
using ExCSS;
using Holo;
using ReactiveUI;
using Svg.Skia;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
    public Interaction<MainViewModel, Uri?> ShowOpenWorkspaceDialog { get; }
    public Interaction<Workspace, Uri?> ShowSaveAsWorkspaceDialog { get; }
    public Interaction<SearchWindowViewModel, string?> ShowSearchDialog { get; }
    public Interaction<ShiftTimesWindowViewModel, Unit> ShowShiftTimesDialog { get; }
    public ICommand ShowAboutDialogCommand { get; }
    public ICommand ShowStylesManagerCommand { get; }
    public ICommand NewFileCommand { get; }
    public ICommand ShowOpenFileDialogCommand { get; }
    public ICommand ShowSaveFileDialogCommand { get; }
    public ICommand ShowSaveAsFileDialogCommand { get; }
    public ICommand ShowOpenWorkspaceDialogCommand { get; }
    public ICommand ShowSaveWorkspaceDialogCommand { get; }
    public ICommand CloseTabCommand { get; }
    public ICommand RemoveFromWorkspaceCommand { get; }
    public ICommand ActivateScriptCommand { get; }
    public ICommand ReloadScriptsCommand { get; }
    public ICommand QuitCommand { get; }
    public ICommand ShowSearchDialogCommand { get; }
    public ICommand ShowShiftTimesDialogCommand { get; }

    public ObservableCollection<TabItemViewModel> Tabs { get; set; }
    public ObservableCollection<TemplatedControl> ScriptMenuItems { get; }
    public bool HasScripts { get; set; }

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

    private void GenerateScriptsMenu()
    {
        ScriptMenuItems.Clear();
        var scriptSvg = new Avalonia.Svg.Skia.Svg(new Uri("avares://Ameko/Assets/B5/code-slash.svg")) { Path = new Uri("avares://Ameko/Assets/B5/code-slash.svg").LocalPath };
        var reloadSvg = new Avalonia.Svg.Skia.Svg(new Uri("avares://Ameko/Assets/B5/arrow-clockwise.svg")) { Path = new Uri("avares://Ameko/Assets/B5/arrow-clockwise.svg").LocalPath };
        var dcSvg = new Avalonia.Svg.Skia.Svg(new Uri("avares://Ameko/Assets/B5/globe.svg")) { Path = new Uri("avares://Ameko/Assets/B5/globe.svg").LocalPath };

        foreach (var script in ScriptService.Instance.LoadedScripts)
        {
            var mi = new MenuItem
            {
                Header = script.Item2,
                Command = ActivateScriptCommand,
                CommandParameter = script.Item1,
                Icon = scriptSvg
            };
            ScriptMenuItems.Add(mi);
        }

        ScriptMenuItems.Add(new Separator());
        ScriptMenuItems.Add(new MenuItem
        {
            Header = "_Reload Scripts",
            Command = ReloadScriptsCommand,
            Icon = reloadSvg
        });
        ScriptMenuItems.Add(new MenuItem
        {
            Header = "_Dependency Control",
            // Command = ReloadScriptsCommand,
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
        ShowOpenWorkspaceDialog = new Interaction<MainViewModel, Uri?>();
        ShowSaveAsWorkspaceDialog = new Interaction<Workspace, Uri?>();
        ShowSearchDialog = new Interaction<SearchWindowViewModel, string?>();
        ShowShiftTimesDialog = new Interaction<ShiftTimesWindowViewModel, Unit>();

        ShowAboutDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplayAboutBox(ShowAboutDialog));
        ShowStylesManagerCommand = ReactiveCommand.Create(() => IOCommandService.DisplayStylesManager(ShowStylesManager, this));
        ShowOpenFileDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplayOpenSubtitleFileDialog(ShowOpenFileDialog, this));
        ShowSaveFileDialogCommand = ReactiveCommand.Create(() => IOCommandService.SaveOrDisplaySaveAsDialog(ShowSaveAsFileDialog));
        ShowSaveAsFileDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplaySaveAsDialog(ShowSaveAsFileDialog));
        ShowSaveWorkspaceDialogCommand = ReactiveCommand.Create(() => IOCommandService.WorkspaceSaveOrDisplaySaveAsDialog(ShowSaveAsWorkspaceDialog));
        ShowOpenWorkspaceDialogCommand = ReactiveCommand.Create(() => IOCommandService.DisplayWorkspaceOpenDialog(ShowOpenWorkspaceDialog, this));

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

        ActivateScriptCommand = ReactiveCommand.Create<string>(async (string scriptName) =>
        {
            var script = ScriptService.Instance.Get(scriptName);
            if (script == null) return;
            var result = await script.Execute();
            Debug.WriteLine($"Script `{script.Name}` finished executing with status `{result.Status}` and message `{result.Message}`");
        });

        ReloadScriptsCommand = ReactiveCommand.Create(() =>
        {
            ScriptService.Instance.Reload(true);
        });

        Tabs = new ObservableCollection<TabItemViewModel>(HoloContext.Instance.Workspace.Files.Select(f => new TabItemViewModel(f.Title, f)));

        ScriptMenuItems = new ObservableCollection<TemplatedControl>();
        GenerateScriptsMenu();
        HasScripts = ScriptMenuItems.Any();

        HoloContext.Instance.Workspace.Files.CollectionChanged += UpdateLoadedTabsCallback;
        ScriptService.Instance.LoadedScripts.CollectionChanged += (o, e) =>
        {
            GenerateScriptsMenu();
            HasScripts = ScriptMenuItems.Any();
            this.RaisePropertyChanged(nameof(HasScripts));
        };
    }
}
