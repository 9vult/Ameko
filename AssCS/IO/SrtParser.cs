// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.RegularExpressions;

namespace AssCS.IO;

/// <summary>
/// Parse a SRT file to an ass document
/// </summary>
public partial class SrtParser : FileParser
{
    protected override Document Parse(TextReader reader)
    {
        Document doc = new(false);
        doc.StyleManager.LoadDefault();
        doc.ScriptInfoManager.LoadDefault();

        const string newline = @"\N";

        var lineNumber = 0;
        var linebreakDebt = 0;
        Match? timestampMatch = null;

        Event? @event = null;
        StringBuilder text = new();
        var state = ParseState.Initial;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            line = line.Trim();
            var foundTimestamps = false;

            switch (state)
            {
                case ParseState.Initial:
                    // Ignore leading blank lines
                    if (string.IsNullOrEmpty(line))
                        break;
                    if (int.TryParse(line, out _)) // Timestamp
                    {
                        state = ParseState.Timestamp;
                        break;
                    }

                    timestampMatch = TimestampRegex().Match(line);
                    if (timestampMatch.Success)
                    {
                        foundTimestamps = true;
                        break;
                    }
                    throw new FormatException($"Expected subtitle index at line {lineNumber}");
                case ParseState.Timestamp:
                    timestampMatch = TimestampRegex().Match(line);
                    if (timestampMatch.Success)
                    {
                        foundTimestamps = true;
                        break;
                    }
                    throw new FormatException($"Expected timestamps at line {lineNumber}");
                case ParseState.FirstBodyLine:
                    if (string.IsNullOrEmpty(line))
                    {
                        state = ParseState.LastWasBlank;
                        linebreakDebt = 0;
                        break;
                    }
                    text.Append(line);
                    state = ParseState.BodyLine;
                    break;
                case ParseState.BodyLine:
                    if (string.IsNullOrEmpty(line))
                    {
                        state = ParseState.LastWasBlank;
                        linebreakDebt = 1;
                        break;
                    }

                    text.Append(newline);
                    text.Append(line);
                    break;
                case ParseState.LastWasBlank:
                    linebreakDebt++;
                    if (string.IsNullOrEmpty(line))
                        break;
                    if (int.TryParse(line, out _))
                    {
                        state = ParseState.Timestamp;
                        break;
                    }
                    timestampMatch = TimestampRegex().Match(line);
                    if (timestampMatch.Success)
                    {
                        foundTimestamps = true;
                        break;
                    }

                    while (linebreakDebt-- > 0)
                        text.Append(newline);
                    text.Append(line);
                    state = ParseState.BodyLine;
                    break;
            }

            if (foundTimestamps)
            {
                if (@event is not null)
                {
                    @event.Text = SrtTagParser.ToAss(text.ToString().Trim());
                    text.Clear();
                }

                @event = new Event(doc.EventManager.NextId)
                {
                    Start = Time.FromSrt(timestampMatch?.Groups[1].Value ?? "0:00:00.00"),
                    End = Time.FromSrt(timestampMatch?.Groups[2].Value ?? "0:00:00.00"),
                };
                doc.EventManager.AddLast(@event);
                state = ParseState.FirstBodyLine;
            }
        }

        if (state is ParseState.Timestamp or ParseState.FirstBodyLine)
            throw new FormatException("Unexpected end of SRT file");

        @event?.Text = SrtTagParser.ToAss(text.ToString().Trim());

        return doc;
    }

    private enum ParseState
    {
        Initial,
        Timestamp,
        FirstBodyLine,
        BodyLine,
        LastWasBlank,
    }

    [GeneratedRegex(
        @"^([0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2},[0-9]{1,}) --> ([0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2},[0-9]{1,})"
    )]
    private static partial Regex TimestampRegex();

    private static partial class SrtTagParser
    {
        public static string ToAss(string srt)
        {
            var bold = new SrtTag('b');
            var italics = new SrtTag('i');
            var underline = new SrtTag('u');
            var strikethrough = new SrtTag('s');

            List<SrtFontInfo> fonts = [];

            var ass = new StringBuilder();

            while (srt.Length > 0)
            {
                var tagMatch = TagRegex().Match(srt);
                if (!tagMatch.Success)
                {
                    ass.Append(srt);
                    break;
                }

                // Tag found, translate it!
                var previousText = tagMatch.Groups[1].Value;
                var tagName = tagMatch.Groups[2].Value.ToLower();
                var tagAttributes = tagMatch.Groups[3].Value;
                var subsequentText = tagMatch.Groups[4].Value;

                ass.Append(previousText); // unchanged
                srt = subsequentText; // Ready for next loop

                switch (GetTagType(tagName))
                {
                    case SrtTagType.BoldOpen:
                        bold.Open(ass);
                        break;
                    case SrtTagType.BoldClose:
                        bold.Close(ass);
                        break;
                    case SrtTagType.ItalicsOpen:
                        italics.Open(ass);
                        break;
                    case SrtTagType.ItalicsClose:
                        italics.Close(ass);
                        break;
                    case SrtTagType.UnderlineOpen:
                        underline.Open(ass);
                        break;
                    case SrtTagType.UnderlineClose:
                        underline.Close(ass);
                        break;
                    case SrtTagType.StrikethroughOpen:
                        strikethrough.Open(ass);
                        break;
                    case SrtTagType.StrikethroughClose:
                        strikethrough.Close(ass);
                        break;
                    case SrtTagType.FontOpen:
                    {
                        SrtFontInfo oldInfo = default;
                        SrtFontInfo newInfo = default;

                        if (fonts.Count > 0)
                            oldInfo = fonts.Last();
                        newInfo = oldInfo;

                        // Find all attributes on the tag
                        var attribMatches = FontAttributeRegex().Matches(tagAttributes);
                        foreach (Match match in attribMatches)
                        {
                            var attribName = match.Groups[1].Value.ToLower();
                            var attribValue = match.Groups[2].Value[1..^1];

                            if (attribName == "face")
                                newInfo.Face = $"{{\\fn{attribValue}}}";
                            else if (attribName == "size")
                                newInfo.Size = $"{{\\fs{attribValue}}}";
                            else if (attribName == "color")
                                newInfo.Color =
                                    attribValue[0] == '#'
                                        ? $"{{\\c{Color.FromHex(attribValue).AsOverrideColor()}}}"
                                        : $"{{\\c{Color.FromHtml(attribValue)}}}";
                        }

                        if (newInfo.Face != oldInfo.Face)
                            ass.Append(newInfo.Face);
                        if (newInfo.Size != oldInfo.Size)
                            ass.Append(newInfo.Size);
                        if (newInfo.Color != oldInfo.Color)
                            ass.Append(newInfo.Color);
                        fonts.Add(newInfo);
                        break;
                    }
                    case SrtTagType.FontClose:
                    {
                        if (fonts.Count == 0)
                            break;
                        var currentInfo = fonts.Last();
                        fonts.RemoveAt(fonts.Count - 1);

                        // Get old attributes (if applicable)
                        var oldInfo = fonts.LastOrDefault();

                        // Restore to previous settings
                        if (currentInfo.Face != oldInfo.Face)
                            ass.Append(
                                string.IsNullOrEmpty(oldInfo.Face) ? @"{\fn}" : oldInfo.Face
                            );
                        if (currentInfo.Size != oldInfo.Size)
                            ass.Append(
                                string.IsNullOrEmpty(oldInfo.Size) ? @"{\fs}" : oldInfo.Size
                            );
                        if (currentInfo.Color != oldInfo.Color)
                            ass.Append(
                                string.IsNullOrEmpty(oldInfo.Color) ? @"{\c}" : oldInfo.Color
                            );
                        break;
                    }
                    default:
                        ass.Append($"<{tagName}{tagAttributes}>");
                        break;
                }
            }

            return ass.ToString().Replace("}{", string.Empty);
        }

        private static SrtTagType GetTagType(string tagName)
        {
            switch (tagName.Length)
            {
                case 1:
                    return tagName[0] switch
                    {
                        'b' => SrtTagType.BoldOpen,
                        'i' => SrtTagType.ItalicsOpen,
                        'u' => SrtTagType.UnderlineOpen,
                        's' => SrtTagType.StrikethroughOpen,
                        _ => SrtTagType.Unknown,
                    };
                case 2:
                    if (tagName[0] != '/')
                        return SrtTagType.Unknown;
                    return tagName[1] switch
                    {
                        'b' => SrtTagType.BoldClose,
                        'i' => SrtTagType.ItalicsClose,
                        'u' => SrtTagType.UnderlineClose,
                        's' => SrtTagType.StrikethroughClose,
                        _ => SrtTagType.Unknown,
                    };
                default:
                    return tagName switch
                    {
                        "font" => SrtTagType.FontOpen,
                        "/font" => SrtTagType.FontClose,
                        _ => SrtTagType.Unknown,
                    };
            }
        }

        private struct SrtFontInfo
        {
            public string? Face { get; set; }
            public string? Size { get; set; }
            public string? Color { get; set; }
        }

        private class SrtTag(char tag)
        {
            private int _level = 0;

            public void Open(StringBuilder builder)
            {
                if (_level == 0)
                    builder.Append($"{{\\{tag}1}}");
                _level++;
            }

            public void Close(StringBuilder builder)
            {
                if (_level == 1)
                    builder.Append($"{{\\{tag}0}}");
                if (_level > 0)
                    _level--;
            }
        }

        private enum SrtTagType
        {
            Unknown,
            BoldOpen,
            BoldClose,
            ItalicsOpen,
            ItalicsClose,
            UnderlineOpen,
            UnderlineClose,
            StrikethroughOpen,
            StrikethroughClose,
            FontOpen,
            FontClose,
        }

        [GeneratedRegex(
            "^(.*?)<(/?b|/?i|/?u|/?s|/?font)([^>]*)>(.*)$",
            RegexOptions.IgnoreCase,
            "en-US"
        )]
        private static partial Regex TagRegex();

        [GeneratedRegex("""\b(face|color|size)=(".*?"|'.*?')""", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex FontAttributeRegex();
    }
}
