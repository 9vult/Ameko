// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Holo.Configuration;
using Microsoft.Extensions.Logging;

namespace Ameko.Services;

public class CultureService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public IReadOnlyList<CultureInfo> AvailableCultures { get; } = [new("en-US")];

    private void SetCulture(string cultureName)
    {
        var culture =
            AvailableCultures.FirstOrDefault(c => c.Name == cultureName) ?? AvailableCultures[0];

        _logger.LogInformation("Setting culture to {CultureName}", culture.Name);

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

    public CultureService(IConfiguration configuration, ILogger<CultureService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        SetCulture(_configuration.Culture);
        _configuration.PropertyChanged += OnConfigurationChanged;
    }
}
