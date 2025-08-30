// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using Ameko.Messages;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class FileModifiedDialogViewModel(string fileName) : ViewModelBase
{
    public string Header { get; } = string.Format(I18N.FileModified.FileModified_Header, fileName);

    public ReactiveCommand<Unit, FileModifiedDialogClosedMessage> IgnoreCommand { get; } =
        ReactiveCommand.Create(() =>
            new FileModifiedDialogClosedMessage(FileModifiedDialogClosedResult.Ignore)
        );

    public ReactiveCommand<Unit, FileModifiedDialogClosedMessage> SaveAsCommand { get; } =
        ReactiveCommand.Create(() =>
            new FileModifiedDialogClosedMessage(FileModifiedDialogClosedResult.SaveAs)
        );
}
