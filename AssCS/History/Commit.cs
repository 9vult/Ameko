// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

/// <summary>
/// A commit in history
/// </summary>
public record Commit(
    int Id,
    ChangeType Type,
    IReadOnlyList<int> Chain,
    IReadOnlyDictionary<int, Event> Events,
    IReadOnlyList<Style> Styles,
    IReadOnlyList<ExtradataEntry> Extradata,
    IReadOnlyList<int>? Selection
);
