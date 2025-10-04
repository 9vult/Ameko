// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Holo.Configuration;
using Holo.Models;
using Microsoft.Extensions.Logging;
using WeCantSpell.Hunspell;

namespace Holo.Providers;

public partial class SpellcheckService(
    IFileSystem fileSystem,
    ILogger<SpellcheckService> logger,
    IDictionaryService dictionaryService,
    IConfiguration configuration,
    IGlobals globals,
    IProjectProvider projectProvider
) : ISpellcheckService
{
    private static readonly string[] EffectHeuristics = ["fx", "karaoke", "code"];
    private const string TypesettingHeuristic = @"\pos";

    private WordList _dictionary = WordList.CreateFromWords([]); // Empty

    public SpellcheckLanguage CurrentLanguage { get; private set; } =
        SpellcheckLanguage.AvailableLanguages[0];

    /// <summary>
    /// Check the current document,
    /// using basic heuristics to exclude assumed typesetting events
    /// </summary>
    /// <remarks>
    ///<b>THIS METHOD IS NAIVE!</b> It uses spaces to find word boundaries.
    /// In the future, it may be worth looking into additional libraries,
    /// like ICU4N's BreakIterator, to find word boundaries in a fully
    /// language-agnostic way.
    /// </remarks>
    public IEnumerable<SpellcheckSuggestion> CheckSpelling()
    {
        var document = projectProvider.Current.WorkingSpace?.Document;
        if (document is null)
            yield break;

        // Heuristically exclude some events from being checked
        var candidates = document
            .EventManager.Events.Where(e => !e.IsComment)
            .Where(e => !EffectHeuristics.Contains(e.Effect))
            .Where(e =>
                !e.Text.Contains(TypesettingHeuristic, StringComparison.CurrentCultureIgnoreCase)
            );

        foreach (var @event in candidates)
        {
            var wordMatches = WordRegex().Matches(@event.Text.Trim());
            foreach (Match match in wordMatches)
            {
                var word = match.Value;
                if (string.IsNullOrWhiteSpace(word))
                    continue;
                if (_dictionary.Check(word))
                    continue;

                // Not in the dictionary
                yield return new SpellcheckSuggestion
                {
                    EventId = @event.Id,
                    Word = word,
                    Suggestions = _dictionary.Suggest(word).ToList(),
                };
            }
        }
    }

    /// <summary>
    /// Rebuild the spellcheck dictionary using the current culture and custom words
    /// </summary>
    public void RebuildDictionary()
    {
        var culture = projectProvider.Current.SpellcheckCulture ?? configuration.SpellcheckCulture;
        CurrentLanguage =
            SpellcheckLanguage.AvailableLanguages.FirstOrDefault(l => l.Locale == culture)
            ?? CurrentLanguage;

        if (!dictionaryService.TryGetDictionary(culture, out var spd))
        {
            logger.LogWarning(
                "Spellcheck dictionary could not be found for culture {Culture}",
                culture
            );
            return;
        }

        var dic = fileSystem.FileStream.New(
            spd.DictionaryPath.LocalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );
        var aff = fileSystem.FileStream.New(
            spd.AffixPath.LocalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );

        _dictionary = WordList.CreateFromStreams(dic, aff);

        foreach (var word in projectProvider.Current.CustomWords)
        {
            _dictionary.Add(word);
        }

        foreach (var word in globals.CustomWords)
        {
            _dictionary.Add(word);
        }
    }

    /// <inheritdoc />
    public bool IsDictionaryInstalled(string culture)
    {
        logger.LogDebug("Checking if {Culture} dictionary is installed...", culture);
        if (!dictionaryService.TryGetDictionary(culture, out _))
        {
            var lang = SpellcheckLanguage.AvailableLanguages.FirstOrDefault(l =>
                l.Locale == culture
            );
            if (lang is null)
            {
                logger.LogWarning("Language {Culture} not found, ignoring for now...", culture);
                return true; // Ignore
            }

            return false;
        }
        return true;
    }

    /// <inheritdoc />
    public void AddWordToProject(string word)
    {
        projectProvider.Current.AddCustomWord(word);
        _dictionary.Add(word);
    }

    /// <inheritdoc />
    public void AddWordToGlobals(string word)
    {
        globals.AddCustomWord(word);
        _dictionary.Add(word);
    }

    [GeneratedRegex(@"\b[\w']+\b")]
    private static partial Regex WordRegex();
}
