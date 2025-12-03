// SPDX-License-Identifier: GPL-3.0-only

using AssCS;

namespace Ameko.Messages;

public class StyleEditorDialogClosedMessage(Style? style)
{
    public Style? Style { get; } = style;
}
