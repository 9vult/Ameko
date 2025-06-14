// SPDX-License-Identifier: GPL-3.0-only

using AssCS;

namespace Ameko.Messages;

public class ColorDialogClosedMessage(Color color)
{
    public Color Color { get; } = color;
}
