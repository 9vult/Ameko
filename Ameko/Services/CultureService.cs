// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Holo.Configuration;
using Holo.IO;
using Holo.Models;
using Holo.Providers;

namespace Ameko.Services;

public class CultureService : ICultureService
{
    // Some fun stuff because this needs to be both statically-accessible (for startup)
    // and accessible via DI (for everything else)

    private static readonly List<DisplayLanguage> StaticLanguages =
    [
        new("English (US)", "en-US"),
        new("English (UK)", "en-GB"),
        new("Spanish (LATM)", "es-419"),
    ];

    private static readonly List<CultureInfo> StaticCultures = StaticLanguages
        .Select(l => new CultureInfo(l.Locale))
        .ToList();

    private static DisplayLanguage _currentLanguage = StaticLanguages[0];

    /// <inheritdoc />
    public IReadOnlyCollection<DisplayLanguage> AvailableLanguages => StaticLanguages;

    /// <inheritdoc />
    public IReadOnlyCollection<CultureInfo> AvailableCultures => StaticCultures;

    /// <inheritdoc />
    public DisplayLanguage CurrentLanguage => _currentLanguage;

    public static void SetCulture()
    {
        var cultureName = GetConfiguredCulture();
        var culture =
            StaticCultures.FirstOrDefault(c => c.Name == cultureName) ?? StaticCultures[0];

        _currentLanguage = StaticLanguages.Any(l => l.Locale == cultureName)
            ? StaticLanguages.First(l => l.Locale == cultureName)
            : StaticLanguages.First();

        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
    }

    /// <summary>
    /// Get the configured culture
    /// </summary>
    /// <returns>The configured culture</returns>
    /// <remarks>
    /// This method is needed to bypass the DI container, since using the "normal way" of <see cref="IConfiguration"/>
    /// results in issues since this needs to be called <i>before</i> the window XMLs are loaded
    /// </remarks>
    private static string GetConfiguredCulture()
    {
        const string defaultCulture = "en-US";

        var configPath = Paths.Configuration.LocalPath;
        if (!File.Exists(configPath))
        {
            return defaultCulture;
        }

        try
        {
            using var fs = new FileStream(
                configPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var reader = new StreamReader(fs);
            using var doc = JsonDocument.Parse(reader.ReadToEnd());
            if (doc.RootElement.TryGetProperty("Culture", out var cultureElement))
            {
                return cultureElement.GetString() ?? defaultCulture;
            }

            return defaultCulture;
        }
        catch
        {
            return defaultCulture;
        }
    }
}
