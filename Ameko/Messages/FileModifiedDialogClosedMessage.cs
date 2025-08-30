// SPDX-License-Identifier: GPL-3.0-only

namespace Ameko.Messages;

public class FileModifiedDialogClosedMessage(FileModifiedDialogClosedResult result)
{
    public FileModifiedDialogClosedResult Result { get; } = result;
}

public enum FileModifiedDialogClosedResult
{
    Ignore,
    SaveAs,
}
