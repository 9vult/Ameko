// SPDX-License-Identifier: MPL-2.0

using System.Text;

namespace AssCS.Overrides;

/// <summary>
/// An individual syllable in a karaoke line
/// </summary>
public class Syllable
{
    /// <summary>
    /// Start time of the syllable
    /// </summary>
    /// <remarks>
    /// The start time is relative to "time zero" (the beginning of the file),
    /// not the start of the line containing the syllable
    /// </remarks>
    public Time Start { get; set; } = new();

    /// <summary>
    /// Duration of the syllable in milliseconds
    /// </summary>
    public long Duration { get; set; } = 0;

    /// <summary>
    /// Type of karaoke tag
    /// </summary>
    public string TagType { get; set; } = string.Empty;

    /// <summary>
    /// Contents of the syllable
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Override tags applied to the syllable
    /// </summary>
    public Dictionary<int, string> OverrideTags { get; private set; } = [];

    public string GetFormattedText(bool includeKTag)
    {
        var sb = new StringBuilder();

        if (includeKTag)
            sb.Append($"{TagType}{(Duration + 5) / 10}");

        int i = 0;
        foreach (var pair in OverrideTags)
        {
            sb.Append(Text[i..pair.Key]);
            sb.Append(pair.Value);
            i = pair.Key;
        }

        sb.Append(Text[i..]);
        return sb.ToString();
    }
}
