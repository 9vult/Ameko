// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Reactive.Disposables;
using Ameko.ViewModels.Windows;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Holo;
using NLog;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static void DoShowLogWindow(IInteractionContext<LogWindowViewModel, Unit> interaction)
    {
        Log.Trace("Displaying Log Window");
        var window = new LogWindow { DataContext = interaction.Input };
        window.Show();
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
                ViewModel.ShowLogWindow.RegisterHandler(DoShowLogWindow);
            }

            Disposable.Create(() => { }).DisposeWith(disposables);
        });

        Log.Info("Main Window initialized");
    }
}
