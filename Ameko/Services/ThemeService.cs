// SPDX-License-Identifier: GPL-3.0-only

using System.ComponentModel;
using Avalonia;
using Avalonia.Styling;
using Holo.Configuration;
using Holo.Models;
using NLog;

namespace Ameko.Services;

public class ThemeService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IConfiguration _configuration;
    private readonly Application? _app;

    private void ApplyTheme(Theme theme)
    {
        if (_app is null)
            return;

        Logger.Info($"Applying {theme} theme");

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

    public ThemeService(IConfiguration configuration)
    {
        _configuration = configuration;
        _app = Application.Current;

        ApplyTheme(_configuration.Theme);
        _configuration.PropertyChanged += OnConfigurationChanged;
    }
}
