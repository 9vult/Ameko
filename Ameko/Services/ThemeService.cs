// SPDX-License-Identifier: GPL-3.0-only

using System.ComponentModel;
using Avalonia;
using Avalonia.Styling;
using Holo;
using Holo.Models;
using NLog;

namespace Ameko.Services;

public class ThemeService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Configuration _configuration;
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
        if (e.PropertyName == nameof(Configuration.Theme))
        {
            ApplyTheme(_configuration.Theme);
        }
    }

    public ThemeService(Configuration configuration)
    {
        _configuration = configuration;
        _app = Application.Current;

        ApplyTheme(_configuration.Theme);
        _configuration.PropertyChanged += OnConfigurationChanged;
    }
}
