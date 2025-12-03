// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using Avalonia.Threading;
using Holo.Configuration;
using Holo.IO;
using Holo.Providers;
using Microsoft.Extensions.Logging;

namespace Ameko.Services;

public class AutosaveService
{
    private static readonly string AutosaveDir = Path.Combine(Directories.DataHome, "autosave");

    private readonly ILogger<AutosaveService> _logger;
    private readonly IIoService _ioService;
    private readonly IConfiguration _configuration;
    private readonly IProjectProvider _projectProvider;
    private readonly IMessageService _messageService;

    private readonly DispatcherTimer _timer = new();

    private void OnConfigurationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IConfiguration.AutosaveEnabled):
                var isEnabled = _configuration.AutosaveEnabled;
                if (isEnabled)
                    _timer.Start();
                else
                    _timer.Stop();
                _logger.LogInformation("Autosave {Enabled}", isEnabled ? "enabled" : "disabled");
                break;

            case nameof(IConfiguration.AutosaveInterval):
                var interval = _configuration.AutosaveInterval;
                _timer.Interval = TimeSpan.FromSeconds(interval);
                _logger.LogInformation("Autosave interval set to {Interval} seconds", interval);
                break;
            default:
                return;
        }
    }

    private void DoAutosaveAsync(object? sender, EventArgs e)
    {
        var prj = _projectProvider.Current;
        if (prj.LoadedWorkspaces.Count == 0)
            return;

        var saveHappened = false;

        foreach (var wsp in prj.LoadedWorkspaces)
        {
            if (wsp.IsAutosaved)
                continue;

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}-{prj.Title}-{wsp.Title}.ass";
            var path = new Uri(Path.Combine(AutosaveDir, fileName));

            try
            {
                _ioService.SaveSubtitle(wsp, path);
                wsp.IsAutosaved = true;
                saveHappened = true;
                _logger.LogInformation("Autosaved {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to autosave {Workspace} to {Path}",
                    wsp.Title,
                    path.LocalPath
                );
            }
        }

        if (saveHappened)
            _messageService.Enqueue(I18N.Other.Message_AutosaveComplete, TimeSpan.FromSeconds(3));
    }

    public AutosaveService(
        ILogger<AutosaveService> logger,
        IFileSystem fileSystem,
        IIoService ioService,
        IConfiguration configuration,
        IProjectProvider projectProvider,
        IMessageService messageService
    )
    {
        _logger = logger;
        _ioService = ioService;
        _configuration = configuration;
        _projectProvider = projectProvider;
        _messageService = messageService;

        try
        {
            if (!fileSystem.Directory.Exists(AutosaveDir))
                fileSystem.Directory.CreateDirectory(AutosaveDir);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create autosave directory");
        }

        _timer.Interval = TimeSpan.FromSeconds(_configuration.AutosaveInterval);
        if (_configuration.AutosaveEnabled)
            _timer.Start();

        _timer.Tick += DoAutosaveAsync;
        _configuration.PropertyChanged += OnConfigurationPropertyChanged;
    }
}
