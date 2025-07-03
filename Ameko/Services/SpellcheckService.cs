// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using Ameko.DataModels;
using Avalonia.Platform;
using Holo.Providers;
using WeCantSpell.Hunspell;

namespace Ameko.Services;

public class SpellcheckService
{
    private readonly WordList _dictionary;
    private readonly ISolutionProvider _solutionProvider;

    public void RegisterWord(string word)
    {
        _dictionary.Add(word);
    }

    public void RegisterWord(IList<string> words)
    {
        foreach (var word in words)
        {
            RegisterWord(word);
        }
    }

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
    public List<SpellcheckSuggestion> CheckSpelling()
    {
        var document = _solutionProvider.Current.WorkingSpace?.Document;
        if (document is null)
            return [];

        List<SpellcheckSuggestion> suggestions = [];

        // Exclude some events from being checked
        var candidates = document
            .EventManager.Events.Where(e => !e.IsComment)
            .Where(e => !new[] { "fx", "karaoke", "code" }.Contains(e.Effect))
            .Where(e => !e.Text.Contains(@"\pos", StringComparison.CurrentCultureIgnoreCase));

        foreach (var @event in candidates)
        {
            foreach (var word in @event.Text.Split(' '))
            {
                if (_dictionary.Check(word))
                    continue;

                // Not in the dictionary
                suggestions.Add(
                    new SpellcheckSuggestion
                    {
                        EventId = @event.Id,
                        Word = word,
                        Suggestions = _dictionary.Suggest(word).ToList(),
                    }
                );
            }
        }
        return suggestions;
    }

    public SpellcheckService(ISolutionProvider solutionProvider)
    {
        _solutionProvider = solutionProvider;
        _dictionary = WordList.CreateFromStreams(
            AssetLoader.Open(new Uri("avares://Ameko/Assets/Spellcheck/en_US.dic")),
            AssetLoader.Open(new Uri("avares://Ameko/Assets/Spellcheck/en_US.aff"))
        );
    }
}
