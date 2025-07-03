// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;

namespace Ameko.DataModels;

public readonly struct SpellcheckSuggestion
{
    public int EventId { get; init; }
    public string Word { get; init; }
    public List<string> Suggestions { get; init; }
}
