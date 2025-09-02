// SPDX-License-Identifier: MPL-2.0

using Holo.Models;
using NLog;

namespace Holo.Providers;

/// <summary>
/// A simple service for scheduling messages
/// </summary>
public class MessageService : IMessageService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Queue<Message> _messages = new();
    private readonly Lock _lock = new();
    private CancellationTokenSource? _cts;
    private Task? _worker;

    /// <inheritdoc />
    public void Enqueue(string content, TimeSpan duration) =>
        Enqueue(new Message(content, duration));

    /// <inheritdoc />
    public void Enqueue(Message message)
    {
        lock (_lock)
        {
            _messages.Enqueue(message);

            if (_worker is not null && !_worker.IsCompleted)
                return;
            _cts = new CancellationTokenSource();
            _worker = Task.Run(() => ProcMessages(_cts.Token), _cts.Token);
        }
    }

    /// <summary>
    /// Proc the next message in the queue and wait out its duration
    /// </summary>
    /// <param name="token">Cancellation token</param>
    private async Task ProcMessages(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Message? message;

            lock (_lock)
            {
                if (!_messages.TryDequeue(out message))
                {
                    _worker = null;
                    QueueDrained?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }

            MessageReady?.Invoke(this, message);
            try
            {
                await Task.Delay(message.Duration, token);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return;
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler<Message>? MessageReady;

    /// <inheritdoc />
    public event EventHandler<EventArgs>? QueueDrained;
}
