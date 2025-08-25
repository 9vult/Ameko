// SPDX-License-Identifier: MPL-2.0

namespace AssCS;

public static class Options
{
    /// <summary>
    /// If whitespace should be included in <see cref="Event.Cps"/> calculation
    /// </summary>
    public static bool CpsIncludesWhitespace { get; set; } = true;

    /// <summary>
    /// If punctuation should be included in <see cref="Event.Cps"/> calculation
    /// </summary>
    public static bool CpsIncludesPunctuation { get; set; } = true;

    /// <summary>
    /// If whitespace should be included in <see cref="Event.MaxLineWidth"/> calculation
    /// </summary>
    public static bool LineWidthIncludesWhitespace { get; set; } = true;

    /// <summary>
    /// If punctuation should be included in <see cref="Event.MaxLineWidth"/> calculation
    /// </summary>
    public static bool LineWidthIncludesPunctuation { get; set; } = false;

    /// <summary>
    /// Default layer to use when creating events
    /// </summary>
    public static int DefaultLayer { get; set; } = 0;
}
