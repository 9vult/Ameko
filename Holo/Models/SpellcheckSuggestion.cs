// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

public readonly struct SpellcheckSuggestion
{
    public int EventId { get; init; }
    public string Word { get; init; }
    public List<string> Suggestions { get; init; }
}
