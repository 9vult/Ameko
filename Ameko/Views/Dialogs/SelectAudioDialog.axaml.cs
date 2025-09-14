using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq; // <--- needed for Subscribe(Action<T>)
using Ameko.ViewModels.Dialogs;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Dialogs
{
    public partial class SelectAudioDialog : ReactiveWindow<SelectAudioDialogViewModel>
    {
        public SelectAudioDialog()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                if (ViewModel is not null)
                {
                    ViewModel
                        .OkCommand.Subscribe(Observer.Create<string?>(result => Close(result)))
                        .DisposeWith(disposables);

                    ViewModel
                        .CancelCommand.Subscribe(Observer.Create<Unit>(_ => Close(null)))
                        .DisposeWith(disposables);
                }
            });
        }
    }
}
