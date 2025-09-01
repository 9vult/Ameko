// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using Holo.Configuration;
using Holo.Models;
using NLog;
using WeCantSpell.Hunspell;

namespace Holo.Providers;

public class SpellcheckService(
    IFileSystem fileSystem,
    IDictionaryService dictionaryService,
    IConfiguration configuration,
    IGlobals globals,
    IProjectProvider iProjectProvider
) : ISpellcheckService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string[] EffectHeuristics = ["fx", "karaoke", "code"];
    private const string TypesettingHeuristic = @"\pos";

    private WordList _dictionary = WordList.CreateFromWords([]); // Empty

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
        var document = iProjectProvider.Current.WorkingSpace?.Document;
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
            foreach (var word in @event.Text.Split(' '))
            {
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
        var culture = iProjectProvider.Current.SpellcheckCulture ?? configuration.SpellcheckCulture;
        if (!dictionaryService.TryGetDictionary(culture, out var spd))
        {
            Logger.Warn($"Spellcheck dictionary could not be found for culture {culture}");
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

        foreach (var word in iProjectProvider.Current.CustomWords)
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
        Logger.Debug($"Checking if {culture} dictionary is installed...");
        if (!dictionaryService.TryGetDictionary(culture, out _))
        {
            var lang = SpellcheckLanguage.AvailableLanguages.FirstOrDefault(l =>
                l.Locale == culture
            );
            if (lang is null)
            {
                Logger.Warn($"Language {culture} not found, ignoring for now...");
                return true; // Ignore
            }

            return false;
        }
        return true;
    }
}
