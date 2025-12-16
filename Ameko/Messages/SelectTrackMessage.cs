// SPDX-License-Identifier: GPL-3.0-only

namespace Ameko.Messages;

public class SelectTrackMessage(int trackIndex)
{
    public int TrackIndex { get; } = trackIndex;
}
