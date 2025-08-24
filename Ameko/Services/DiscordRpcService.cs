// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using Ameko.Messages;
using DiscordRPC;
using DiscordRPC.Logging;
using Holo;
using Holo.Configuration;
using Holo.Providers;
using NLog;
using ReactiveUI;
using LogLevel = DiscordRPC.Logging.LogLevel;

namespace Ameko.Services;

/// <summary>
/// Manages Discord RPC
/// </summary>
public class DiscordRpcService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly ISolutionProvider _solutionProvider;
    private readonly IConfiguration _configuration;
    private readonly DiscordRpcClient _client;
    private readonly Timestamps _timestamps;

    public void Update(WorkingSpaceChangedMessage _)
    {
        var slnName = _solutionProvider.Current.Title;
        var wspName = _solutionProvider.Current.WorkingSpace?.SavePath is not null
            ? Path.GetFileNameWithoutExtension(
                _solutionProvider.Current.WorkingSpace.SavePath.LocalPath
            )
            : _solutionProvider.Current.WorkingSpace?.Title;

        if (_configuration.DiscordRpcEnabled)
        {
            Logger.Debug($"Setting rich presence to {slnName}/{wspName}");

            _client.SetPresence(
                new RichPresence
                {
                    Details = $"Editing {wspName}.ass",
                    State = $"in {slnName}.asln",
                    Timestamps = _timestamps,
                }
            );
        }
        else
        {
            _client.ClearPresence();
        }
    }

    public DiscordRpcService(ISolutionProvider solutionProvider, IConfiguration config)
    {
        _solutionProvider = solutionProvider;
        _configuration = config;

        _client = new DiscordRpcClient("1209896771719921704");
        _client.Logger = new ConsoleLogger { Level = LogLevel.Warning };
        _client.Initialize();
        _timestamps = Timestamps.Now;

        MessageBus.Current.Listen<WorkingSpaceChangedMessage>().Subscribe(Update);
    }

    ~DiscordRpcService()
    {
        _client.ClearPresence();
        _client.Dispose();
    }
}
