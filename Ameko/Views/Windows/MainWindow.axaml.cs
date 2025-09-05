// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.Messages;
using Ameko.Services;
using Ameko.ViewModels.Dialogs;
using Ameko.ViewModels.Windows;
using Ameko.Views.Dialogs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using DynamicData;
using Holo.Configuration.Keybinds;
using Holo.Models;
using NLog;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static readonly string[] ScriptExtensions = [".ass", ".srt", ".txt"];
    private static readonly string[] VideoExtensions = [".mkv", ".mp4"];
    private const string ProjectExtension = ".aproj";

    private SearchDialog _searchDialog;
    private bool _isSearching = false;
    private bool _canClose = false;

    private async Task DoShowOpenSubtitleDialogAsync(IInteractionContext<Unit, Uri[]> interaction)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = I18N.Other.FileDialog_OpenSubtitle_Title,
                AllowMultiple = true,
                FileTypeFilter =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Ass)
                    {
                        Patterns = ["*.ass"],
                    },
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Srt)
                    {
                        Patterns = ["*.srt"],
                    },
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Text)
                    {
                        Patterns = ["*.txt"],
                    },
                ],
            }
        );
        if (files.Count > 0)
        {
            interaction.SetOutput(files.Select(f => f.Path).ToArray());
            return;
        }
        interaction.SetOutput([]);
    }

    private async Task DoShowSaveSubtitleAsDialogAsync(
        IInteractionContext<string, Uri?> interaction
    )
    {
        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = I18N.Other.FileDialog_SaveSubtitle_Title,
                FileTypeChoices =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Ass)
                    {
                        Patterns = ["*.ass"],
                    },
                ],
                SuggestedFileName = interaction.Input,
            }
        );

        if (file is not null)
        {
            var path = file.Path;
            if (!Path.HasExtension(path.LocalPath))
                path = new Uri(Path.ChangeExtension(path.LocalPath, ".ass"));

            interaction.SetOutput(path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowExportSubtitleDialogAsync(
        IInteractionContext<string, Uri?> interaction
    )
    {
        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = I18N.Other.FileDialog_ExportSubtitle_Title,
                FileTypeChoices =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Text)
                    {
                        Patterns = ["*.txt"],
                    },
                ],
                SuggestedFileName = interaction.Input,
            }
        );

        if (file is not null)
        {
            var path = file.Path;
            if (!Path.HasExtension(path.LocalPath))
                path = new Uri(Path.ChangeExtension(path.LocalPath, ".txt"));

            interaction.SetOutput(path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowOpenProjectDialogAsync(IInteractionContext<Unit, Uri?> interaction)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = I18N.Other.FileDialog_OpenProject_Title,
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Project)
                    {
                        Patterns = ["*.aproj"],
                    },
                ],
            }
        );
        if (files.Count > 0)
        {
            interaction.SetOutput(files[0].Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowOpenFolderAsProjectDialogAsync(
        IInteractionContext<Unit, Uri?> interaction
    )
    {
        var dirs = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = I18N.Other.FileDialog_OpenFolderAsProject_Title,
                AllowMultiple = false,
            }
        );
        if (dirs.Count > 0)
        {
            interaction.SetOutput(dirs[0].Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowSaveProjectAsDialogAsync(IInteractionContext<string, Uri?> interaction)
    {
        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = I18N.Other.FileDialog_SaveProject_Title,
                FileTypeChoices =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Project)
                    {
                        Patterns = ["*.aproj"],
                    },
                ],
                SuggestedFileName = interaction.Input,
            }
        );

        if (file is not null)
        {
            var path = file.Path;
            if (!Path.HasExtension(path.LocalPath))
                path = new Uri(Path.ChangeExtension(path.LocalPath, ".aproj"));

            interaction.SetOutput(path);
            return;
        }
        interaction.SetOutput(null);
    }

    private void DoShowSearchDialog(IInteractionContext<SearchDialogViewModel, Unit> interaction)
    {
        _searchDialog.DataContext ??= interaction.Input;

        if (_isSearching)
        {
            _searchDialog.Activate();
        }
        else
        {
            _isSearching = true;
            _searchDialog.Show();
        }
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowSpellcheckDialogAsync(
        IInteractionContext<SpellcheckDialogViewModel, Unit> interaction
    )
    {
        Logger.Debug("Displaying spellcheck dialog");
        var dialog = new SpellcheckDialog { DataContext = interaction.Input };
        await dialog.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private static void DoShowStylesManager(
        IInteractionContext<StylesManagerWindowViewModel, Unit> interaction
    )
    {
        Logger.Debug("Displaying Styles Manager");
        var window = new StylesManagerWindow { DataContext = interaction.Input };
        window.Show();
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowAttachReferenceFileDialogAsync(
        IInteractionContext<Unit, Uri?> interaction
    )
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = I18N.Other.FileDialog_AttachReferenceFile_Title,
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Ass)
                    {
                        Patterns = ["*.ass"],
                    },
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Srt)
                    {
                        Patterns = ["*.srt"],
                    },
                ],
            }
        );
        if (files.Count > 0)
        {
            interaction.SetOutput(files[0].Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowShiftTimesDialogAsync(
        IInteractionContext<ShiftTimesDialogViewModel, Unit> interaction
    )
    {
        Logger.Debug("Displaying Shift Times dialog");
        var window = new ShiftTimesDialog { DataContext = interaction.Input };
        await window.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowOpenVideoDialogAsync(IInteractionContext<Unit, Uri?> interaction)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = I18N.Other.FileDialog_OpenVideo_Title,
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Video)
                    {
                        Patterns = ["*.mkv", "*.mp4", "*.m4v", "*.mov"],
                    },
                ],
            }
        );
        if (files.Count > 0)
        {
            interaction.SetOutput(files[0].Path);
            return;
        }
        interaction.SetOutput(null);
    }

    private async Task DoShowJumpDialogAsync(
        IInteractionContext<JumpDialogViewModel, JumpDialogClosedMessage?> interaction
    )
    {
        Logger.Debug("Displaying Jump dialog");
        var dialog = new JumpDialog { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<JumpDialogClosedMessage?>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowPkgManWindowAsync(
        IInteractionContext<PkgManWindowViewModel, Unit> interaction
    )
    {
        Logger.Debug("Displaying Dependency Control");
        var window = new PkgManWindow { DataContext = interaction.Input };
        await window.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private void DoShowPlaygroundWindow(
        IInteractionContext<PlaygroundWindowViewModel, Unit> interaction
    )
    {
        Logger.Debug("Displaying Script Playground");
        var window = new PlaygroundWindow { DataContext = interaction.Input };
        window.Show();
        interaction.SetOutput(Unit.Default);
    }

    private static void DoShowLogWindow(IInteractionContext<LogWindowViewModel, Unit> interaction)
    {
        Logger.Debug("Displaying Log Window");
        var window = new LogWindow { DataContext = interaction.Input };
        window.Show();
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowAboutWindowAsync(
        IInteractionContext<AboutWindowViewModel, Unit> interaction
    )
    {
        Logger.Debug("Displaying About Window");
        var window = new AboutWindow() { DataContext = interaction.Input };
        await window.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private static void DoShowKeybindsWindow(
        IInteractionContext<KeybindsWindowViewModel, Unit> interaction
    )
    {
        Logger.Debug("Displaying Keybinds Window");
        var window = new KeybindsWindow() { DataContext = interaction.Input };
        window.Show();
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowinstallDictionaryDialogAsync(
        IInteractionContext<InstallDictionaryDialogViewModel, Unit> interaction
    )
    {
        Logger.Debug("Displaying install dictionary dialog");
        var dialog = new InstallDictionaryDialog { DataContext = interaction.Input };
        await dialog.ShowDialog<EmptyMessage>(this);
        interaction.SetOutput(Unit.Default);
    }

    public MainWindow()
    {
        Logger.Info("Initializing Main Window...");
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, DoDragAndDrop);

        // Set up search dialog
        _searchDialog = new SearchDialog();
        _searchDialog.Closing += (sender, args) =>
        {
            if (sender is not SearchDialog searchDialog)
                return;
            args.Cancel = true;
            searchDialog.Hide();
            _isSearching = false;
        };

        Closing += async (sender, args) => await OnWindowClosing(sender, args);

        this.WhenActivated(disposables =>
        {
            if (ViewModel is not null)
            {
                // File
                ViewModel.OpenSubtitle.RegisterHandler(DoShowOpenSubtitleDialogAsync);
                ViewModel.SaveSubtitleAs.RegisterHandler(DoShowSaveSubtitleAsDialogAsync);
                ViewModel.ExportSubtitle.RegisterHandler(DoShowExportSubtitleDialogAsync);
                ViewModel.OpenProject.RegisterHandler(DoShowOpenProjectDialogAsync);
                ViewModel.OpenFolderAsProject.RegisterHandler(DoShowOpenFolderAsProjectDialogAsync);
                ViewModel.SaveProjectAs.RegisterHandler(DoShowSaveProjectAsDialogAsync);
                // Edit
                ViewModel.ShowSearchDialog.RegisterHandler(DoShowSearchDialog);
                ViewModel.ShowSpellcheckDialog.RegisterHandler(DoShowSpellcheckDialogAsync);
                // Subtitle
                ViewModel.ShowStylesManager.RegisterHandler(DoShowStylesManager);
                ViewModel.AttachReferenceFile.RegisterHandler(DoShowAttachReferenceFileDialogAsync);
                // Project
                // Timing
                ViewModel.ShowShiftTimesDialog.RegisterHandler(DoShowShiftTimesDialogAsync);
                // Video
                ViewModel.OpenVideo.RegisterHandler(DoShowOpenVideoDialogAsync);
                ViewModel.ShowJumpDialog.RegisterHandler(DoShowJumpDialogAsync);
                // Scripts
                ViewModel.ShowPackageManager.RegisterHandler(DoShowPkgManWindowAsync);
                ViewModel.ShowPlaygroundWindow.RegisterHandler(DoShowPlaygroundWindow);
                // Help
                ViewModel.ShowLogWindow.RegisterHandler(DoShowLogWindow);
                ViewModel.ShowAboutWindow.RegisterHandler(DoShowAboutWindowAsync);
                ViewModel.ShowKeybindsWindow.RegisterHandler(DoShowKeybindsWindow);
                // Other
                ViewModel.ShowInstallDictionaryDialog.RegisterHandler(
                    DoShowinstallDictionaryDialogAsync
                );

                // Register keybinds
                AttachKeybinds();
                ViewModel.KeybindService.KeybindRegistrar.OnKeybindsChanged += (_, _) =>
                {
                    AttachKeybinds();
                };

                // Generate layouts menu and apply current layout
                ApplyLayout(ViewModel, ViewModel.LayoutProvider.Current);
                GenerateLayoutsMenu();
                ViewModel.LayoutProvider.OnLayoutChanged += (_, args) =>
                {
                    ApplyLayout(ViewModel, args.Layout);
                    GenerateLayoutsMenu();
                };

                // Generate scripts menu
                GenerateScriptsMenu();
                ViewModel.ScriptService.OnReload += (_, _) =>
                {
                    GenerateScriptsMenu();
                };
            }

            ViewModel?.CheckSpellcheckDictionaryCommand.Execute(null);

            Disposable.Create(() => { }).DisposeWith(disposables);
        });

        Logger.Info("Done!");
    }

    private void ApplyLayout(MainWindowViewModel? vm, Layout? layout)
    {
        if (vm is null || layout is null)
            return;

        if (layout.Window.IsProjectExplorerOnLeft)
        {
            var columnDefinitions = new ColumnDefinitions("Auto, 2, *");
            columnDefinitions[0].MinWidth = 100;
            columnDefinitions[2].MinWidth = 500;
            MainWindowGrid.ColumnDefinitions = columnDefinitions;

            ProjectExplorer.SetValue(Grid.ColumnProperty, 0);
            TabControlHost.SetValue(Grid.ColumnProperty, 2);
        }
        else
        {
            var columnDefinitions = new ColumnDefinitions("*, 2, Auto");
            columnDefinitions[0].MinWidth = 500;
            columnDefinitions[2].MinWidth = 100;
            MainWindowGrid.ColumnDefinitions = columnDefinitions;

            ProjectExplorer.SetValue(Grid.ColumnProperty, 2);
            TabControlHost.SetValue(Grid.ColumnProperty, 0);
        }
    }

    private void GenerateLayoutsMenu()
    {
        if (ViewModel is null || ViewModel.DisplayInWindowMenu)
            return;

        Logger.Debug("Regenerating layouts native menu...");
        var menuItems = LayoutMenuService.GenerateNativeMenuItemSource(
            ViewModel.LayoutProvider.Layouts,
            ViewModel.SelectLayoutCommand
        );
        var reloadItem = LayoutMenuService.GenerateReloadNativeMenuItem(
            ViewModel.RefreshLayoutsCommand
        );

        var menu = NativeMenu
            .GetMenu(this)
            ?.Items.OfType<NativeMenuItem>()
            .FirstOrDefault(m => m.Header == I18N.Resources.Menu_Layouts)
            ?.Menu;

        if (menu is null)
            return;

        menu.Items.Clear();
        menu.Items.AddRange(menuItems);
        if (menuItems.Count > 0)
            menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(reloadItem);
        Logger.Debug("Done!");
    }

    private void GenerateScriptsMenu()
    {
        if (ViewModel is null || ViewModel.DisplayInWindowMenu)
            return;

        Logger.Debug("Regenerating scripts native menu...");
        var menuItems = ScriptMenuService.GenerateNativeMenuItemSource(
            ViewModel.ScriptService.Scripts,
            ViewModel.ExecuteScriptCommand
        );
        var reloadItem = ScriptMenuService.GenerateReloadNativeMenuItem(
            ViewModel.ReloadScriptsCommand
        );
        var pkgManItem = ScriptMenuService.GeneratePkgManNativeMenuItem(
            ViewModel.ShowPackageManagerCommand
        );
        var playgroundItem = ScriptMenuService.GeneratePlaygroundNativeMenuItem(
            ViewModel.ShowPlaygroundWindowCommand
        );

        var menu = NativeMenu
            .GetMenu(this)
            ?.Items.OfType<NativeMenuItem>()
            .FirstOrDefault(m => m.Header == I18N.Resources.Menu_Scripts)
            ?.Menu;

        if (menu is null)
            return;

        menu.Items.Clear();
        menu.Items.AddRange(menuItems);
        if (menuItems.Count > 0)
            menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(playgroundItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(reloadItem);
        menu.Items.Add(pkgManItem);
        Logger.Debug("Done...");
    }

    private void AttachKeybinds()
    {
        if (ViewModel is null)
            return;
        ViewModel.KeybindService.AttachKeybinds(ViewModel, KeybindContext.Global, this);
        ViewModel.KeybindService.AttachScriptKeybinds(
            ViewModel.ExecuteScriptCommand,
            KeybindContext.Global,
            this
        );
    }

    private void DoDragAndDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() is not { } files || ViewModel is null)
            return;

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.Path.LocalPath);

            if (ScriptExtensions.Contains(ext))
            {
                ViewModel.OpenSubtitleNoGuiCommand.Execute(file.Path);
                continue;
            }

            if (VideoExtensions.Contains(ext))
            {
                ViewModel.OpenVideoNoGuiCommand.Execute(file.Path);
                continue;
            }

            if (ext == ProjectExtension)
            {
                ViewModel.OpenProjectNoGuiCommand.Execute(file.Path);
                continue;
            }
        }
    }

    private async Task OnWindowClosing(object? _, WindowClosingEventArgs e)
    {
        if (ViewModel is null || _canClose)
        {
            Logger.Info("Shutting down...");
            return;
        }

        e.Cancel = true;

        foreach (var wsp in ViewModel.ProjectProvider.Current.LoadedWorkspaces.ToArray())
        {
            await ViewModel.IoService.SafeCloseWorkspace(wsp, ViewModel.SaveSubtitleAs, false);
        }

        if (ViewModel.ProjectProvider.Current.LoadedWorkspaces.Count == 0)
        {
            _canClose = true;
            Close();
        }
        else
        {
            Logger.Info(
                $"Quit aborted - {ViewModel.ProjectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
            );
        }
    }
}
