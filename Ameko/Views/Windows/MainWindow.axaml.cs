// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.ViewModels.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Holo;
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
                Title = I18N.Resources.FileDialog_OpenSubtitle_Title,
                AllowMultiple = true,
                FileTypeFilter =
                [
                    new FilePickerFileType(I18N.Resources.FileDialog_FileType_Ass)
                    {
                        Patterns = ["*.ass"],
                    },
                    new FilePickerFileType(I18N.Resources.FileDialog_FileType_Text)
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
                Title = I18N.Resources.FileDialog_SaveSubtitle_Title,
                FileTypeChoices =
                [
                    new FilePickerFileType(I18N.Resources.FileDialog_FileType_Ass)
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
                Title = I18N.Resources.FileDialog_ExportSubtitle_Title,
                FileTypeChoices =
                [
                    new FilePickerFileType(I18N.Resources.FileDialog_FileType_Text)
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
                Title = I18N.Resources.FileDialog_OpenSolution_Title,
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(I18N.Resources.FileDialog_FileType_Solution)
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

    private async Task DoShowSaveSolutionAsDialogAsync(
        IInteractionContext<string, Uri?> interaction
    )
    {
        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = I18N.Resources.FileDialog_SaveSolution_Title,
                FileTypeChoices =
                [
                    new FilePickerFileType(I18N.Resources.FileDialog_FileType_Solution)
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
                ViewModel.SaveSolutionAs.RegisterHandler(DoShowSaveSolutionAsDialogAsync);
                // Subtitle
                ViewModel.ShowStylesManager.RegisterHandler(DoShowStylesManager);
                // Help
                ViewModel.ShowLogWindow.RegisterHandler(DoShowLogWindow);
                ViewModel.ShowAboutWindow.RegisterHandler(DoShowAboutWindowAsync);
            }

            Disposable.Create(() => { }).DisposeWith(disposables);
        });

        Log.Info("Main Window initialized");
    }
}
