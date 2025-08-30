// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.Messages;
using Ameko.ViewModels.Dialogs;
using Ameko.ViewModels.Windows;
using Ameko.Views.Dialogs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Holo.Configuration.Keybinds;
using Holo.Models;
using NLog;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static readonly string[] ScriptExtensions = [".ass", ".txt"];
    private static readonly string[] VideoExtensions = [".mkv", ".mp4"];
    private const string SolutionExtension = ".asln";

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

    private async Task DoShowOpenSolutionDialogAsync(IInteractionContext<Unit, Uri?> interaction)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = I18N.Other.FileDialog_OpenSolution_Title,
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Solution)
                    {
                        Patterns = ["*.asln"],
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

    private async Task DoShowOpenFolderAsSolutionDialogAsync(
        IInteractionContext<Unit, Uri?> interaction
    )
    {
        var dirs = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = I18N.Other.FileDialog_OpenFolderAsSolution_Title,
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

    private async Task DoShowSaveSolutionAsDialogAsync(
        IInteractionContext<string, Uri?> interaction
    )
    {
        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = I18N.Other.FileDialog_SaveSolution_Title,
                FileTypeChoices =
                [
                    new FilePickerFileType(I18N.Other.FileDialog_FileType_Solution)
                    {
                        Patterns = ["*.asln"],
                    },
                ],
                SuggestedFileName = interaction.Input,
            }
        );

        if (file is not null)
        {
            var path = file.Path;
            if (!Path.HasExtension(path.LocalPath))
                path = new Uri(Path.ChangeExtension(path.LocalPath, ".asln"));

            interaction.SetOutput(path);
            return;
        }
        interaction.SetOutput(null);
    }

    private static void DoShowStylesManager(
        IInteractionContext<StylesManagerWindowViewModel, Unit> interaction
    )
    {
        Log.Debug("Displaying Styles Manager");
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
        Log.Debug("Displaying Shift Times dialog");
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
        Log.Debug("Displaying Jump dialog");
        var dialog = new JumpDialog { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<JumpDialogClosedMessage?>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowPkgManWindowAsync(
        IInteractionContext<PkgManWindowViewModel, Unit> interaction
    )
    {
        Log.Debug("Displaying Dependency Control");
        var window = new PkgManWindow { DataContext = interaction.Input };
        await window.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private static void DoShowLogWindow(IInteractionContext<LogWindowViewModel, Unit> interaction)
    {
        Log.Debug("Displaying Log Window");
        var window = new LogWindow { DataContext = interaction.Input };
        window.Show();
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowAboutWindowAsync(
        IInteractionContext<AboutWindowViewModel, Unit> interaction
    )
    {
        Log.Debug("Displaying About Window");
        var window = new AboutWindow() { DataContext = interaction.Input };
        await window.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private static void DoShowKeybindsWindow(
        IInteractionContext<KeybindsWindowViewModel, Unit> interaction
    )
    {
        Log.Debug("Displaying Keybinds Window");
        var window = new KeybindsWindow() { DataContext = interaction.Input };
        window.Show();
        interaction.SetOutput(Unit.Default);
    }

    public MainWindow()
    {
        Log.Info("Initializing Main Window...");
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, DoDragAndDrop);

        this.WhenActivated(disposables =>
        {
            if (ViewModel is not null)
            {
                // File
                ViewModel.OpenSubtitle.RegisterHandler(DoShowOpenSubtitleDialogAsync);
                ViewModel.SaveSubtitleAs.RegisterHandler(DoShowSaveSubtitleAsDialogAsync);
                ViewModel.ExportSubtitle.RegisterHandler(DoShowExportSubtitleDialogAsync);
                ViewModel.OpenSolution.RegisterHandler(DoShowOpenSolutionDialogAsync);
                ViewModel.OpenFolderAsSolution.RegisterHandler(
                    DoShowOpenFolderAsSolutionDialogAsync
                );
                ViewModel.SaveSolutionAs.RegisterHandler(DoShowSaveSolutionAsDialogAsync);
                // Subtitle
                ViewModel.ShowStylesManager.RegisterHandler(DoShowStylesManager);
                ViewModel.AttachReferenceFile.RegisterHandler(DoShowAttachReferenceFileDialogAsync);
                // Solution
                // Timing
                ViewModel.ShowShiftTimesDialog.RegisterHandler(DoShowShiftTimesDialogAsync);
                // Video
                ViewModel.OpenVideo.RegisterHandler(DoShowOpenVideoDialogAsync);
                ViewModel.ShowJumpDialog.RegisterHandler(DoShowJumpDialogAsync);
                // Scripts
                ViewModel.ShowPackageManager.RegisterHandler(DoShowPkgManWindowAsync);
                // Help
                ViewModel.ShowLogWindow.RegisterHandler(DoShowLogWindow);
                ViewModel.ShowAboutWindow.RegisterHandler(DoShowAboutWindowAsync);
                ViewModel.ShowKeybindsWindow.RegisterHandler(DoShowKeybindsWindow);

                // Register keybinds
                AttachKeybinds();
                ViewModel.KeybindService.KeybindRegistrar.OnKeybindsChanged += (_, _) =>
                {
                    AttachKeybinds();
                };

                // Apply layouts
                ApplyLayout(ViewModel, ViewModel.LayoutProvider.Current);
                ViewModel.LayoutProvider.OnLayoutChanged += (_, args) =>
                {
                    ApplyLayout(ViewModel, args.Layout);
                };
            }

            Disposable.Create(() => { }).DisposeWith(disposables);
        });

        Log.Info("Done!");
    }

    private void ApplyLayout(MainWindowViewModel? vm, Layout? layout)
    {
        if (vm is null || layout is null)
            return;

        if (layout.Window.IsSolutionExplorerOnLeft)
        {
            var columnDefinitions = new ColumnDefinitions("Auto, 2, *");
            columnDefinitions[0].MinWidth = 100;
            columnDefinitions[2].MinWidth = 500;
            MainWindowGrid.ColumnDefinitions = columnDefinitions;

            SolutionExplorer.SetValue(Grid.ColumnProperty, 0);
            WorkspaceTabControl.SetValue(Grid.ColumnProperty, 2);
        }
        else
        {
            var columnDefinitions = new ColumnDefinitions("*, 2, Auto");
            columnDefinitions[0].MinWidth = 500;
            columnDefinitions[2].MinWidth = 150;
            MainWindowGrid.ColumnDefinitions = columnDefinitions;

            SolutionExplorer.SetValue(Grid.ColumnProperty, 2);
            WorkspaceTabControl.SetValue(Grid.ColumnProperty, 0);
        }
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

            if (ext == SolutionExtension)
            {
                ViewModel.OpenSolutionNoGuiCommand.Execute(file.Path);
                continue;
            }
        }
    }
}
