// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.ViewModels.Dialogs;
using Ameko.ViewModels.Windows;
using Ameko.Views.Dialogs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Holo;
using Holo.Configuration.Keybinds;
using NLog;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
        Log.Trace("Displaying Styles Manager");
        var window = new StylesManagerWindow { DataContext = interaction.Input };
        window.Show();
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowShiftTimesDialogAsync(
        IInteractionContext<ShiftTimesDialogViewModel, Unit> interaction
    )
    {
        Log.Trace("Displaying Shift Times dialog");
        var window = new ShiftTimesDialog { DataContext = interaction.Input };
        await window.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowDepCtlWindowAsync(
        IInteractionContext<DepCtrlWindowViewModel, Unit> interaction
    )
    {
        Log.Trace("Displaying Dependency Control");
        var window = new DepCtrlWindow { DataContext = interaction.Input };
        await window.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private static void DoShowLogWindow(IInteractionContext<LogWindowViewModel, Unit> interaction)
    {
        Log.Trace("Displaying Log Window");
        var window = new LogWindow { DataContext = interaction.Input };
        window.Show();
        interaction.SetOutput(Unit.Default);
    }

    private async Task DoShowAboutWindowAsync(
        IInteractionContext<AboutWindowViewModel, Unit> interaction
    )
    {
        Log.Trace("Displaying About Window");
        var window = new AboutWindow() { DataContext = interaction.Input };
        await window.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    public MainWindow()
    {
        Log.Info("Initializing Main Window...");
        InitializeComponent();

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
                // Solution
                // Timing
                ViewModel.ShowShiftTimesDialog.RegisterHandler(DoShowShiftTimesDialogAsync);
                // Scripts
                ViewModel.ShowDependencyControl.RegisterHandler(DoShowDepCtlWindowAsync);
                // Help
                ViewModel.ShowLogWindow.RegisterHandler(DoShowLogWindow);
                ViewModel.ShowAboutWindow.RegisterHandler(DoShowAboutWindowAsync);

                // Register keybinds
                AttachKeybinds();
                ViewModel.KeybindService.KeybindRegistrar.OnKeybindsChanged += (_, _) =>
                {
                    AttachKeybinds();
                };
            }

            Disposable.Create(() => { }).DisposeWith(disposables);
        });

        Log.Info("Done!");
    }

    private void AttachKeybinds()
    {
        if (ViewModel is null)
            return;
        ViewModel.KeybindService.AttachKeybinds(ViewModel, this);
        ViewModel.KeybindService.AttachScriptKeybinds(
            ViewModel.ExecuteScriptCommand,
            KeybindContext.Global,
            this
        );
    }
}
