// SPDX-License-Identifier: GPL-3.0-only

using AssCS;

namespace Ameko.Messages;

public class ScrollIntoViewMessage(Event @event)
{
    public Event Event { get; } = @event;
}
