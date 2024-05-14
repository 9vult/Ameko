using Ameko.ViewModels;
using AssCS;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Ameko.Views
{
    public partial class LogWindow : ReactiveWindow<LogWindowViewModel>
    {
        private async Task DoCopySelectedLogsAsync(InteractionContext<LogWindowViewModel, string?> interaction)
        {
            var window = GetTopLevel(this);
            var selectedLogs = logGrid.SelectedItems.Cast<Log>().ToList();
            if (window == null || selectedLogs == null)
            {
                interaction.SetOutput("");
                return;
            }
            var result = string.Join(Environment.NewLine, selectedLogs);
            await window.Clipboard!.SetTextAsync(result);
            interaction.SetOutput(result);
        }

        public LogWindow()
        {
            InitializeComponent();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                if (ViewModel != null)
                {
                    ViewModel.CopySelectedLogs.RegisterHandler(DoCopySelectedLogsAsync);
                    logGrid.KeyBindings.Clear();
                    logGrid.KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse("Ctrl+C"), Command = ViewModel.CopySelectedLogsCommand });
                }
            });
        }
    }
}
