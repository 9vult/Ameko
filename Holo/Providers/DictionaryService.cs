// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Holo.IO;
using Holo.Models;
using NLog;

namespace Holo.Providers;

public class DictionaryService : IDictionaryService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static readonly Uri DictionariesRoot = new(
        Path.Combine(Directories.DataHome, "dictionaries")
    );

    private readonly IFileSystem _fileSystem;
    private readonly HttpClient _httpClient;

    private readonly ObservableCollection<SpellcheckDictionary> _installedDictionaries;

    public ReadOnlyObservableCollection<SpellcheckDictionary> InstalledDictionaries;

    /// <inheritdoc />
    public bool TryGetDictionary(
        string lang,
        [NotNullWhen(true)] out SpellcheckDictionary? dictionary
    )
    {
        dictionary = _installedDictionaries.FirstOrDefault(d => d.Lang.Locale == lang);
        return dictionary is not null;
    }

    /// <inheritdoc />
    public async Task<bool> DownloadDictionary(SpellcheckLanguage lang)
    {
        if (TryGetDictionary(lang.Locale, out _))
            return false;

        Logger.Info($"Attempting to download dictionary {lang}");
        try
        {
            if (!_fileSystem.Directory.Exists(DictionariesRoot.LocalPath))
                _fileSystem.Directory.CreateDirectory(DictionariesRoot.LocalPath);

            var dicPath = Path.Combine(DictionariesRoot.LocalPath, $"{lang.Locale}.dic");
            var affPath = Path.Combine(DictionariesRoot.LocalPath, $"{lang.Locale}.aff");

            // Download dictionary
            await using var dicStream = await _httpClient.GetStreamAsync(GetDicUri(lang));
            await using var dicFs = _fileSystem.FileStream.New(
                dicPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            await dicStream.CopyToAsync(dicFs);

            // Download affixes
            await using var affStream = await _httpClient.GetStreamAsync(GetAffUri(lang));
            await using var affFs = _fileSystem.FileStream.New(
                affPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            await affStream.CopyToAsync(affFs);
            PopulateInstalledDictionaries();
            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to download dictionary {lang}");
            Logger.Error(e);
            return false;
        }
    }

    /// <summary>
    /// Find all installed dictionaries
    /// </summary>
    private void PopulateInstalledDictionaries()
    {
        try
        {
            if (!_fileSystem.Directory.Exists(DictionariesRoot.LocalPath))
                _fileSystem.Directory.CreateDirectory(DictionariesRoot.LocalPath);

            _installedDictionaries.Clear();
            foreach (var dic in _fileSystem.Directory.GetFiles(DictionariesRoot.LocalPath, "*.dic"))
            {
                var name = Path.GetFileNameWithoutExtension(dic);
                _installedDictionaries.Add(
                    new SpellcheckDictionary()
                    {
                        Lang = SpellcheckLanguage.AvailableLanguages.First(l => l.Locale == name),
                        DictionaryPath = new Uri(dic),
                        AffixPath = new Uri(dic.Replace(".dic", ".aff")),
                    }
                );
            }
        }
        catch (Exception)
        {
            Logger.Error($"Failed to populate dictionaries list");
        }
    }

    private static Uri GetDicUri(SpellcheckLanguage locale)
    {
        const string baseUrl =
            "https://raw.githubusercontent.com/LibreOffice/dictionaries/refs/heads/master";
        return new Uri($"{baseUrl}/{locale.Directory}/{locale.Locale}{locale.Suffix}.dic");
    }

    private static Uri GetAffUri(SpellcheckLanguage locale)
    {
        const string baseUrl =
            "https://raw.githubusercontent.com/LibreOffice/dictionaries/refs/heads/master";
        return new Uri($"{baseUrl}/{locale.Directory}/{locale.Locale}{locale.Suffix}.aff");
    }

    public DictionaryService(IFileSystem fileSystem, HttpClient httpClient)
    {
        _fileSystem = fileSystem;
        _httpClient = httpClient;
        _installedDictionaries = [];

        InstalledDictionaries = new ReadOnlyObservableCollection<SpellcheckDictionary>(
            _installedDictionaries
        );

        PopulateInstalledDictionaries();
    }
}
