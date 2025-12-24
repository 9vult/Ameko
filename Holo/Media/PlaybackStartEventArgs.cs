// SPDX-License-Identifier: MPL-2.0

using Holo.Models;

namespace Holo.Media;

public class PlaybackStartEventArgs(PlaybackTarget target, long start, long goal) : EventArgs
{
    public PlaybackTarget Target { get; } = target;
    public long StartTime { get; } = start;
    public long GoalTime { get; } = goal;
}
