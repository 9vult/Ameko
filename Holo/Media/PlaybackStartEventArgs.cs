// SPDX-License-Identifier: MPL-2.0

namespace Holo.Media;

public class PlaybackStartEventArgs(long start, long goal) : EventArgs
{
    public long StartTime { get; } = start;
    public long GoalTime { get; } = goal;
}
