using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public class SelectAudioDialogViewModel : ReactiveObject
{
    public ObservableCollection<string> Options { get; }

    private string? _selectedOption;
    public string? SelectedOption
    {
        get => _selectedOption;
        set => this.RaiseAndSetIfChanged(ref _selectedOption, value);
    }

    public ReactiveCommand<Unit, string?> OkCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public SelectAudioDialogViewModel(IEnumerable<string> options)
    {
        Options = new ObservableCollection<string>(options);

        OkCommand = ReactiveCommand.Create(() => SelectedOption);

        CancelCommand = ReactiveCommand.Create(() => { });
    }
}
