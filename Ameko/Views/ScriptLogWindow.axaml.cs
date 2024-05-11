using Ameko.ViewModels;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;

namespace Ameko.Views
{
    public partial class ScriptLogWindow : ReactiveWindow<ScriptLogWindowViewModel>
    {
        public void DoCloseWindow(InteractionContext<Unit, Unit> interaction)
        {
            interaction.SetOutput(Unit.Default);
            this.Close();
        }

        public ScriptLogWindow()
        {
            InitializeComponent();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                ViewModel?.CloseWindow.RegisterHandler(DoCloseWindow);
            });
        }
    }
}
