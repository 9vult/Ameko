// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.ViewModels.Windows;
using Avalonia;
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

    private async Task DoShowOpenSubtitleDialog(IInteractionContext<Unit, Uri[]> interaction)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = I18N.Resources.OpenFileDialog_Subtitle_Title,
                AllowMultiple = true,
                FileTypeFilter =
                [
                    new FilePickerFileType(I18N.Resources.OpenFileDialog_Subtitle_FileType)
                    {
                        Patterns = ["*.ass"],
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
                ViewModel.OpenSubtitle.RegisterHandler(DoShowOpenSubtitleDialog);
                // Help
                ViewModel.ShowLogWindow.RegisterHandler(DoShowLogWindow);
                ViewModel.ShowAboutWindow.RegisterHandler(DoShowAboutWindowAsync);
            }

            Disposable.Create(() => { }).DisposeWith(disposables);
        });

        Log.Info("Main Window initialized");
    }
}
