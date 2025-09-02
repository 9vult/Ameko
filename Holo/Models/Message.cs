// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// A message to be displayed by the GUI
/// </summary>
/// <param name="content">Content of the message</param>
/// <param name="duration">Duration to display the message for</param>
public class Message(string content, TimeSpan duration)
{
    /// <summary>
    /// Content of the message to be displayed
    /// </summary>
    public string Content { get; } = content;

    /// <summary>
    /// Duration to display the message for
    /// </summary>
    public TimeSpan Duration { get; } = duration;
}
