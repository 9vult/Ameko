using Ameko.Services;
using Ameko.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    private SearchWindow _searchWindow;
    private ScriptLogWindow? _scriptLogWindow;
    private bool _isSearching = false;
    private static string[] DragDropExtensions = { ".ass" };

    private bool _canClose = false;

    public async void DoShowDependencyControlWindow(InteractionContext<DependencyControlWindowViewModel, Unit> interaction)
    {
        var dialog = new DependencyControlWindow();
        dialog.DataContext = interaction.Input;
        await dialog.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    public async void DoShowJumpWindow(InteractionContext<JumpWindowViewModel, Unit> interaction)
    {
        var dialog = new JumpWindow();
        dialog.DataContext = interaction.Input;
        await dialog.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    public void DoShowSearchWindow(InteractionContext<SearchWindowViewModel, string?> interaction)
    {
        if (_searchWindow == null) return;
        _searchWindow.DataContext ??= interaction.Input;

        if (_isSearching)
            _searchWindow?.Activate();
        else
        {
            _isSearching = true;
            _searchWindow?.Show();
        }
    }

    private async Task DoShowShiftTimesDialog(InteractionContext<ShiftTimesWindowViewModel, Unit> interaction)
    {
        var dialog = new ShiftTimesWindow();
        dialog.DataContext = interaction.Input;
        await dialog.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowScriptPropertiesDialog(InteractionContext<ScriptPropertiesWindowViewModel, Unit> interaction)
    {
        var dialog = new ScriptPropertiesWindow();
        dialog.DataContext = interaction.Input;
        await dialog.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowAboutDialogAsync(InteractionContext<AboutWindowViewModel, AboutWindowViewModel?> interaction)
    {
        var dialog = new AboutWindow();
        dialog.DataContext = interaction.Input;
        await dialog.ShowDialog(this);
        interaction.SetOutput(null);
    }

    private async Task DoShowOpenFileDialogAsync(InteractionContext<MainViewModel, Uri?> interaction)
    {
        var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Subtitle File",
            AllowMultiple = false,
            FileTypeFilter = new[] {
                new FilePickerFileType("ASS Files") { Patterns = new[] { "*.ass" } }
            }
        });
        if (files.Count > 0)
        {
            interaction.SetOutput(files[0].Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowSaveAsFileDialogAsync(InteractionContext<FileWrapper, Uri?> interaction)
    {
        if (interaction.Input == null) return;

        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Subtitle File As...",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("ASS Files") { Patterns = new[] { "*.ass" } }
            },
            SuggestedFileName = $"{interaction.Input.Title}"
        });
        if (file != null)
        {
            interaction.SetOutput(file.Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowExportSaveAsFileDialogAsync(InteractionContext<FileWrapper, Uri?> interaction)
    {
        if (interaction.Input == null) return;

        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Subtitle File As...",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } }
            },
            SuggestedFileName = $"{interaction.Input.Title}"
        });
        if (file != null)
        {
            interaction.SetOutput(file.Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowSaveAsWorkspaceDialogAsync(InteractionContext<Workspace, Uri?> interaction)
    {
        if (interaction.Input == null) return;

        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Workspace As...",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Ameko Workspace Files") { Patterns = new[] { "*.amk" } }
            },
            SuggestedFileName = $"Workspace"
        });
        if (file != null)
        {
            interaction.SetOutput(file.Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowOpenWorkspaceDialogAsync(InteractionContext<MainViewModel, Uri?> interaction)
    {
        var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Workspace File",
            AllowMultiple = false,
            FileTypeFilter = new[] {
                new FilePickerFileType("Ameko Workspace Files") { Patterns = new[] { "*.amk" } }
            }
        });
        if (files.Count > 0)
        {
            interaction.SetOutput(files[0].Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowOpenVideoDialogAsync(InteractionContext<MainViewModel, Uri?> interaction)
    {
        string[] extensions = [ "*.asf", "*.avi", "*.avs", "*.d2v",
                                "*.h264", "*.hevc", "*.m2ts", "*.m4v",
                                "*.mkv", "*.mov", "*.mp4", "*.mpeg",
                                "*.ogm", "*.webm", "*.wmv", "*.ts", 
                                "*.y4m", "*.yuv" ];
        var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Video File",
            AllowMultiple = false,
            FileTypeFilter = new[] {
                new FilePickerFileType("Video Files") { Patterns = extensions }
            }
        });
        if (files.Count > 0)
        {
            interaction.SetOutput(files[0].Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private void DoShowStylesManager(InteractionContext<StylesManagerViewModel, StylesManagerViewModel?> interaction)
    {
        var manager = new StylesManagerWindow();
        manager.DataContext = interaction.Input;
        manager.Show();
        interaction.SetOutput(null);
    }
    
    private async void DoShowConfigWindowAsync(InteractionContext<ConfigWindowViewModel, Unit> interaction)
    {
        interaction.SetOutput(Unit.Default);
        var manager = new ConfigWindow();
        manager.DataContext = interaction.Input;
        await manager.ShowDialog(this);
    }

    private async void DoShowWorkspaceConfigWindowAsync(InteractionContext<WorkspaceConfigWindowViewModel, Unit> interaction)
    {
        interaction.SetOutput(Unit.Default);
        var manager = new WorkspaceConfigWindow();
        manager.DataContext = interaction.Input;
        await manager.ShowDialog(this);
    }

    private async void DoShowKeybindsWindowAsync(InteractionContext<KeybindsWindowViewModel, Unit> interaction)
    {
        interaction.SetOutput(Unit.Default);
        var manager = new KeybindsWindow();
        manager.DataContext = interaction.Input;
        await manager.ShowDialog(this);
    }

    private async Task DoShowScriptLogDialogAsync(InteractionContext<ScriptLogWindowViewModel, Unit> interaction)
    {
        _scriptLogWindow = new ScriptLogWindow();
        _scriptLogWindow.DataContext = interaction.Input;
        await _scriptLogWindow.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private void DoCloseScriptLogDialog(InteractionContext<Unit, Unit> interaction)
    {
        _scriptLogWindow?.Close();
        interaction.SetOutput(Unit.Default);
    }

    private void DoShowLogsWindow(InteractionContext<LogWindowViewModel, Unit> interaction)
    {
        interaction.SetOutput(Unit.Default);
        var manager = new LogWindow();
        manager.DataContext = interaction.Input;
        manager.Show();
    }

    private void DoShowFreeformPlayground(InteractionContext<FreeformWindowViewModel, Unit> interaction)
    {
        interaction.SetOutput(Unit.Default);
        var playground = new FreeformWindow();
        playground.DataContext = interaction.Input;
        playground.Show();
    }

    private void SetKeybinds()
    {
        if (ViewModel == null) return;
        this.KeyBindings.Clear();
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.file.new", ViewModel.NewFileCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.file.open", ViewModel.ShowOpenFileDialogCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.file.save", ViewModel.ShowSaveFileDialogCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.file.saveas", ViewModel.ShowSaveAsFileDialogCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.file.search", ViewModel.ShowSearchDialogCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.file.shift", ViewModel.ShowShiftTimesDialogCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.file.undo", ViewModel.UndoCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.file.redo", ViewModel.RedoCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.video.jump", ViewModel.ShowJumpDialogCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.app.about", ViewModel.ShowAboutDialogCommand);
        KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, "ameko.app.quit", ViewModel.QuitCommand);

        // Assign global script keybinds
        foreach (var pair in HoloContext.Instance.ConfigurationManager.KeybindsRegistry.GlobalBinds)
        {
            if (pair.Key.StartsWith("ameko")) continue; // Skip builtins
            KeybindService.TrySetKeybind(this, KeybindContext.GLOBAL, pair.Key, ViewModel.ActivateScriptCommand, pair.Key);
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        var autosave = new AutosaveService();

        _searchWindow = new SearchWindow();
        // _searchWindow.Unloaded += (o, e) => _isSearching = false;
        _searchWindow.Closing += (o, e) => { 
            if (o == null) return; 
            ((SearchWindow)o).Hide();
            _isSearching = false;
            e.Cancel = true; 
        };

        HoloContext.Instance.ConfigurationManager.PropertyChanged += (o, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(HoloContext.Instance.ConfigurationManager.KeybindsRegistry):
                    SetKeybinds();
                    break;
            }
        };

        AddHandler(DragDrop.DropEvent, (s, e) =>
        {
            if (e.Data.GetFiles() is { } files && ViewModel != null)
            {
                foreach (var file in files)
                {
                    var lp = file.Path.LocalPath;
                    if (DragDropExtensions.Contains(System.IO.Path.GetExtension(lp)))
                        HoloContext.Instance.Workspace.AddFileToWorkspace(file.Path);
                }
            }
        });

        this.Closing += async (o, e) =>
        {
            if (!_canClose)
            {
                e.Cancel = true;
                if (ViewModel != null)
                {
                    var toClose = await IOCommandService.CloseWindow(ViewModel.ShowSaveAsFileDialog, ViewModel);
                    if (toClose)
                    {
                        _canClose = true;
                        Close();
                    }
                }
            }
        };

        this.WhenActivated((CompositeDisposable disposables) =>
        {
            if (ViewModel != null)
            {
                if (this.KeyBindings.Count == 0) SetKeybinds();

                ViewModel.ShowAboutDialog.RegisterHandler(DoShowAboutDialogAsync);
                ViewModel.ShowOpenFileDialog.RegisterHandler(DoShowOpenFileDialogAsync);
                ViewModel.ShowSaveAsFileDialog.RegisterHandler(DoShowSaveAsFileDialogAsync);
                ViewModel.ShowExportFileDialog.RegisterHandler(DoShowExportSaveAsFileDialogAsync);
                ViewModel.ShowSaveAsWorkspaceDialog.RegisterHandler(DoShowSaveAsWorkspaceDialogAsync);
                ViewModel.ShowOpenWorkspaceDialog.RegisterHandler(DoShowOpenWorkspaceDialogAsync);
                ViewModel.ShowOpenVideoDialog.RegisterHandler(DoShowOpenVideoDialogAsync);
                
                ViewModel.ShowStylesManager.RegisterHandler(DoShowStylesManager);
                ViewModel.ShowSearchDialog.RegisterHandler(DoShowSearchWindow);
                ViewModel.ShowShiftTimesDialog.RegisterHandler(DoShowShiftTimesDialog);
                ViewModel.ShowScriptPropertiesDialog.RegisterHandler(DoShowScriptPropertiesDialog);
                ViewModel.ShowDependencyControlWindow.RegisterHandler(DoShowDependencyControlWindow);
                ViewModel.ShowConfigWindow.RegisterHandler(DoShowConfigWindowAsync);
                ViewModel.ShowWorkspaceConfigWindow.RegisterHandler(DoShowWorkspaceConfigWindowAsync);
                ViewModel.ShowKeybindsWindow.RegisterHandler(DoShowKeybindsWindowAsync);
                ViewModel.ShowLogsWindow.RegisterHandler(DoShowLogsWindow);
                ViewModel.ShowScriptLogsDialog.RegisterHandler(DoShowScriptLogDialogAsync);
                ViewModel.CloseScriptLogsDialog.RegisterHandler(DoCloseScriptLogDialog);
                ViewModel.ShowFreeformPlayground.RegisterHandler(DoShowFreeformPlayground);
                ViewModel.ShowJumpDialog.RegisterHandler(DoShowJumpWindow);
            }

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}
