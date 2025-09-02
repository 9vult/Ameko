// SPDX-License-Identifier: MPL-2.0

using Holo.Models;

namespace Holo.Providers;

/// <summary>
/// A simple service for scheduling messages
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Enqueue a <see cref="Message"/> with the given <paramref name="content"/> and <paramref name="duration"/>
    /// </summary>
    /// <param name="content">Content of the message to display</param>
    /// <param name="duration">Duration to display the message for</param>
    void Enqueue(string content, TimeSpan duration);

    /// <summary>
    /// Enqueue a message to be displayed
    /// </summary>
    /// <param name="message"></param>
    void Enqueue(Message message);

    /// <summary>
    /// A new message is ready
    /// </summary>
    event EventHandler<Message>? MessageReady;

    /// <summary>
    /// The message queue is empty
    /// </summary>
    event EventHandler<EventArgs>? QueueDrained;
}
