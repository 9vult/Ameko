// SPDX-License-Identifier: GPL-3.0-only

using AssCS;

namespace Ameko.Messages;

public class JumpDialogClosedMessage(int frame, int line, Time time)
{
    public int Frame { get; set; } = frame;
    public int Line { get; set; } = line;
    public Time Time { get; set; } = time;
}
