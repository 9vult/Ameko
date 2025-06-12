// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Holo.Configuration;
using NLog;

namespace Ameko.Services;

public class CultureService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IConfiguration _configuration;

    public IReadOnlyList<CultureInfo> AvailableCultures { get; } = [new("en-US")];

    private void SetCulture(string cultureName)
    {
        var culture =
            AvailableCultures.FirstOrDefault(c => c.Name == cultureName) ?? AvailableCultures[0];

        Logger.Info($"Setting culture to {culture.Name}");

        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;

        _configuration.Culture = culture.Name;
    }

    private void OnConfigurationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IConfiguration.Culture))
        {
            SetCulture(_configuration.Culture);
        }
    }

    public CultureService(IConfiguration configuration)
    {
        _configuration = configuration;

        SetCulture(_configuration.Culture);
        _configuration.PropertyChanged += OnConfigurationChanged;
    }
}
