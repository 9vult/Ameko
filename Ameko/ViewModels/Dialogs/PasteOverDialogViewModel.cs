// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Windows.Input;
using Holo.Models;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class PasteOverDialogViewModel : ViewModelBase
{
    private PasteOverField _fields;

    public PasteOverField Fields
    {
        get => _fields;
        set => this.RaiseAndSetIfChanged(ref _fields, value);
    }

    public ICommand SelectAllCommand { get; }
    public ICommand SelectNoneCommand { get; }
    public ICommand SelectTimesCommand { get; }
    public ICommand SelectTextCommand { get; }
    public ReactiveCommand<Unit, PasteOverField> ConfirmCommand { get; }

    public PasteOverDialogViewModel()
    {
        _fields = PasteOverField.Text;

        SelectAllCommand = ReactiveCommand.Create(() =>
            Fields =
                PasteOverField.Comment
                | PasteOverField.Layer
                | PasteOverField.StartTime
                | PasteOverField.EndTime
                | PasteOverField.Style
                | PasteOverField.Actor
                | PasteOverField.MarginLeft
                | PasteOverField.MarginRight
                | PasteOverField.MarginVertical
                | PasteOverField.Effect
                | PasteOverField.Text
        );
        SelectNoneCommand = ReactiveCommand.Create(() => Fields = PasteOverField.None);
        SelectTimesCommand = ReactiveCommand.Create(() =>
            Fields = PasteOverField.StartTime | PasteOverField.EndTime
        );
        SelectTextCommand = ReactiveCommand.Create(() => Fields = PasteOverField.Text);

        ConfirmCommand = ReactiveCommand.Create<PasteOverField>(() => Fields);
    }
}
