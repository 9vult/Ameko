using Ameko.Services;
using Ameko.ViewModels;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;

namespace Ameko.Views
{
    public partial class JumpWindow : ReactiveWindow<JumpWindowViewModel>
    {
        public void DoCloseWindow(InteractionContext<Unit, Unit> interaction)
        {
            interaction.SetOutput(Unit.Default);
            this.Close();
        }

        public JumpWindow()
        {
            InitializeComponent();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                this.GetObservable(ViewModelProperty).WhereNotNull()
                .Subscribe(vm =>
                {
                    // Every time
                    timeBox.AddHandler(InputElement.KeyDownEvent, Helpers.TimeBox_PreKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);

                    ViewModel?.CloseWindow.RegisterHandler(DoCloseWindow);
                });
            });
        }

        private void TextBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ViewModel?.JumpCommand.Execute(Unit.Default);
            }
        }
    }
}
