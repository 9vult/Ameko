// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;
using AssCS.Utilities;

namespace AssCS.IO;

/// <summary>
/// Parse an ass document
/// </summary>
public partial class AssParser : FileParser
{
    /// <inheritdoc/>
    public override Document Parse(TextReader reader)
    {
        Document doc = new(false);

        ParseFunc parseState = ParseUnknown;

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            var match = HEADER_REGEX().Match(line);
            if (match.Success)
            {
                var upper = match.Groups[1].Value.ToUpperInvariant();
                parseState = upper switch
                {
                    "V4 STYLES" => ParseStyle,
                    "V4+ STYLES" => ParseStyle,
                    "V4++ STYLES" => ParseStyle,
                    "EVENTS" => ParseEvent,
                    "SCRIPT INFO" => ParseScriptInfo,
                    "AEGISUB PROJECT GARBAGE" => ParseGarbage,
                    "AEGISUB EXTRADATA" => ParseExtradata,
                    "GRAPHICS" => ParseUnknown, // TODO: Parse graphics
                    "FONTS" => ParseUnknown, // TODO: Parse attachments
                    _ => ParseUnknown,
                };
                // Skip further processing of the header line
                continue;
            }
            parseState(line, doc);
        }

        return doc;
    }

    /// <summary>
    /// Delegate function for parsing a type of line
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private delegate void ParseFunc(string line, Document doc);

    /// <summary>
    /// Parse style lines
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private void ParseStyle(string line, Document doc)
    {
        // TODO: Style versioning
        if (doc.Version != AssVersion.V400P)
            return;

        if (!line.StartsWith("Style:"))
            return;
        doc.StyleManager.Add(Style.FromAss(doc.StyleManager.NextId, line));
    }

    /// <summary>
    /// Parse event lines
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private void ParseEvent(string line, Document doc)
    {
        // TODO: Event versioning
        if (doc.Version != AssVersion.V400P)
            return;

        if (!line.StartsWith("Dialogue:") && !line.StartsWith("Comment:"))
            return;
        doc.EventManager.AddLast(Event.FromAss(doc.EventManager.NextId, line));
    }

    /// <summary>
    /// Parse script info lines
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private void ParseScriptInfo(string line, Document doc)
    {
        // This block can have comments
        if (line.StartsWith(';'))
            return;

        var pair = line.Split(":");
        if (pair.Length < 2)
            return; // Not a key:value pair

        doc.ScriptInfoManager.Set(
            pair[0].Trim(),
            string.Join(":", pair, 1, pair.Length - 1).Trim()
        );

        if (pair[0].Trim().ToUpperInvariant().Equals("SCRIPTTYPE"))
        {
            doc.Version = pair[1].Trim() switch
            {
                "v4.00" => AssVersion.V400,
                "v4.00+" => AssVersion.V400P,
                "v4.00++" => AssVersion.V400PP,
                _ => AssVersion.UNKNOWN,
            };
        }
    }

    /// <summary>
    /// Parse project garbage lines
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private void ParseGarbage(string line, Document doc)
    {
        var pair = line.Split(":");
        if (pair.Length < 2)
            return; // Not a key:value pair

        doc.GarbageManager.Set(pair[0].Trim(), string.Join(":", pair, 1, pair.Length - 1).Trim());
    }

    /// <summary>
    /// Parse extradata lines
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    /// <remarks>
    /// <para>
    /// AssCS Extradata is written with the format character <c>b</c>, and
    /// the value is always base64 encoded. This breaks compatibility with
    /// Aegisub Extradata, which uses <c>e</c> and <c>u</c> for Inline and
    /// UU encoding.
    /// </para><para>
    /// Extradata entries encoded to either Aegisub Extradata spec will be
    /// discarded during parsing. This behavior is subject to change if the
    /// need for extradata interop between Aegisub and AssCS applications
    /// arises.
    /// </para>
    /// </remarks>
    private void ParseExtradata(string line, Document doc)
    {
        if (!line.StartsWith("Data:"))
            return;

        var match = EXTRADATA_REGEX().Match(line);
        if (!match.Success)
            return;

        var id = Convert.ToInt32(match.Groups[1].Value);
        var key = StringEncoder.InlineDecode(match.Groups[2].Value);
        var type = match.Groups[3].Value;
        var value = type switch
        {
            "b" => StringEncoder.Base64Decode(match.Groups[4].Value),
            //"e" => StringEncoder.InlineDecode(match.Groups[4].Value),
            //"u" => Convert.ToString(StringEncoder.UUDecode(match.Groups[4].Value)),
            _ => string.Empty,
        };

        // Skip "empty" lines
        if (string.IsNullOrEmpty(value))
            return;

        // Ensure the next ID will be larger than the largest existing ID
        doc.ExtradataManager.NextId = Math.Max(id + 1, doc.ExtradataManager.NextId);
        doc.ExtradataManager.Add(new ExtradataEntry(id, 0, key, value));
    }

    /// <summary>
    /// Parse unknown lines
    /// </summary>
    /// <param name="_1">Unused</param>
    /// <param name="_2">Unused</param>
    private void ParseUnknown(string _1, Document _2)
    {
        // Do nothing
        return;
    }

    private const string HeaderTemplate = @"^\[(.+)\]";
    private const string ExtradataTemplate = @"^Data:\ *(\d+),([^,]+),(.)(.*)";

    [GeneratedRegex(HeaderTemplate)]
    private static partial Regex HEADER_REGEX();

    [GeneratedRegex(ExtradataTemplate)]
    private static partial Regex EXTRADATA_REGEX();
}
