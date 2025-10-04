// SPDX-License-Identifier: GPL-3.0-only

using System.ComponentModel;
using Avalonia;
using Avalonia.Styling;
using Holo.Configuration;
using Holo.Models;
using Microsoft.Extensions.Logging;

namespace Ameko.Services;

public class ThemeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly Application? _app;

    private void ApplyTheme(Theme theme)
    {
        if (_app is null)
            return;

        _logger.LogInformation("Applying {Theme} theme", theme);

        _app.RequestedThemeVariant = theme switch
        {
            Theme.Dark => ThemeVariant.Dark,
            Theme.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };
    }

    private void OnConfigurationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IConfiguration.Theme))
        {
            ApplyTheme(_configuration.Theme);
        }
    }

    public ThemeService(IConfiguration configuration, ILogger<ThemeService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _app = Application.Current;

        ApplyTheme(_configuration.Theme);
        _configuration.PropertyChanged += OnConfigurationChanged;
    }
}
