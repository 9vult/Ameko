// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;
using AssCS.History;
using AssCS.Utilities;

namespace AssCS.IO;

/// <summary>
/// Parse an ass document
/// </summary>
public partial class AssParser : FileParser
{
    /// <inheritdoc/>
    protected override Document Parse(TextReader reader)
    {
        var doc = new Document(false);
        doc.ScriptInfoManager.LoadDefault();

        ParseFunc parseState = ParseUnknown;

        while (reader.ReadLine() is { } line)
        {
            var data = line.AsSpan();
            if (data.IsEmpty)
                continue;

            if (data[0] is '[' && data[^1] is ']')
            {
                var upper = data[1..^1].ToString().ToUpperInvariant();

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
            parseState(data, doc);
        }

        if (doc.StyleManager.Count == 0)
            doc.StyleManager.LoadDefault();
        if (doc.EventManager.Count == 0)
            doc.EventManager.LoadDefault();

        doc.HistoryManager.Commit(ChangeType.Initial, [doc.EventManager.Head.Id]);

        return doc;
    }

    /// <summary>
    /// Delegate function for parsing a type of line
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private delegate void ParseFunc(ReadOnlySpan<char> line, Document doc);

    /// <summary>
    /// Parse style lines
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private static void ParseStyle(ReadOnlySpan<char> line, Document doc)
    {
        // TODO: Style versioning
        if (doc.Version != AssVersion.V400P)
            return;

        var style = Style.FromAss(doc.StyleManager.NextId, line);
        if (style is not null)
            doc.StyleManager.Add(style);
    }

    /// <summary>
    /// Parse event lines
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private static void ParseEvent(ReadOnlySpan<char> line, Document doc)
    {
        // TODO: Event versioning
        if (doc.Version != AssVersion.V400P)
            return;

        var @event = Event.FromAss(doc.EventManager.NextId, line);
        if (@event is not null)
            doc.EventManager.AddLast(@event);
    }

    /// <summary>
    /// Parse script info lines
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <param name="doc">Document to add the parsed line to</param>
    private static void ParseScriptInfo(ReadOnlySpan<char> line, Document doc)
    {
        // This block can have comments
        if (line.IsEmpty || line[0] is ';')
            return;

        var separator = line.IndexOf(':');
        if (separator <= 0)
            return; // Not a key:value pair

        var key = line[..separator].Trim().ToString();
        var value = line[(separator + 1)..].Trim().ToString();

        doc.ScriptInfoManager.Set(key, value);

        if (key.ToUpperInvariant().Equals("SCRIPTTYPE"))
        {
            doc.Version = value switch
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
    private static void ParseGarbage(ReadOnlySpan<char> line, Document doc)
    {
        var separator = line.IndexOf(':');
        if (separator <= 0)
            return; // Not a key:value pair

        var key = line[..separator].Trim().ToString();
        var value = line[(separator + 1)..].Trim().ToString();

        doc.GarbageManager.Set(key, value);
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
    private static void ParseExtradata(ReadOnlySpan<char> line, Document doc)
    {
        if (!line.StartsWith("Data:"))
            return;
        var data = line.ToString();

        var match = ExtradataRegex().Match(data);
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
    private static void ParseUnknown(ReadOnlySpan<char> _1, Document _2)
    {
        // Do nothing
        return;
    }

    [GeneratedRegex(@"^Data:\ *(\d+),([^,]+),(.)(.*)")]
    private static partial Regex ExtradataRegex();
}
