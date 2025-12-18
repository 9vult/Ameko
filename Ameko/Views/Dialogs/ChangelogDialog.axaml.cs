// SPDX-License-Identifier: GPL-3.0-only

using Ameko.ViewModels.Dialogs;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Dialogs;

public partial class ChangelogDialog : ReactiveWindow<SelectTrackDialogViewModel>
{
    public ChangelogDialog()
    {
        InitializeComponent();
    }
}
