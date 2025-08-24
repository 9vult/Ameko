// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using Ameko.Messages;
using AssCS;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class JumpDialogViewModel : ViewModelBase
{
    public int Frame { get; set; }
    public int Line { get; set; }
    public Time Time { get; set; } = Time.FromSeconds(0);
    public bool VideoLoaded { get; set; }

    public ReactiveCommand<Unit, JumpDialogClosedMessage> ConfirmCommand { get; }

    public JumpDialogViewModel(bool videoLoaded)
    {
        VideoLoaded = videoLoaded;

        ConfirmCommand = ReactiveCommand.Create(() =>
            new JumpDialogClosedMessage(Frame, Line, Time)
        );
    }
}
