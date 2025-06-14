// SPDX-License-Identifier: GPL-3.0-only

using AssCS;

namespace Ameko.Messages;

public class PasteOverDialogClosedMessage(EventField fields)
{
    public EventField Fields { get; } = fields;
}
