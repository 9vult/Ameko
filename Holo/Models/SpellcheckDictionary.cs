// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

public class SpellcheckDictionary
{
    public required SpellcheckLanguage Lang { get; init; }
    public required Uri DictionaryPath { get; init; }
    public required Uri AffixPath { get; init; }
}
