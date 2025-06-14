// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Windows.Input;
using AssCS;
using Holo.Models;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class PasteOverDialogViewModel : ViewModelBase
{
    private EventField _fields;

    public EventField Fields
    {
        get => _fields;
        set => this.RaiseAndSetIfChanged(ref _fields, value);
    }

    public ICommand SelectAllCommand { get; }
    public ICommand SelectNoneCommand { get; }
    public ICommand SelectTimesCommand { get; }
    public ICommand SelectTextCommand { get; }
    public ReactiveCommand<Unit, EventField> ConfirmCommand { get; }

    public PasteOverDialogViewModel()
    {
        _fields = EventField.Text;

        SelectAllCommand = ReactiveCommand.Create(() =>
            Fields =
                EventField.Comment
                | EventField.Layer
                | EventField.StartTime
                | EventField.EndTime
                | EventField.Style
                | EventField.Actor
                | EventField.MarginLeft
                | EventField.MarginRight
                | EventField.MarginVertical
                | EventField.Effect
                | EventField.Text
        );
        SelectNoneCommand = ReactiveCommand.Create(() => Fields = EventField.None);
        SelectTimesCommand = ReactiveCommand.Create(() =>
            Fields = EventField.StartTime | EventField.EndTime
        );
        SelectTextCommand = ReactiveCommand.Create(() => Fields = EventField.Text);

        ConfirmCommand = ReactiveCommand.Create<EventField>(() => Fields);
    }
}
