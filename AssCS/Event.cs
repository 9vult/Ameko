// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AssCS.Overrides;
using AssCS.Overrides.Blocks;
using AssCS.Utilities;

namespace AssCS;

/// <summary>
/// An event in a subtitle document
/// </summary>
/// <param name="id">ID of the event</param>
public partial class Event(int id) : BindableBase, IEntry
{
    private bool _isComment;
    private int _layer = Options.DefaultLayer;
    private Time _start = Time.FromSeconds(0);
    private Time _end = Time.FromSeconds(5);
    private string _style = "Default";
    private string _actor = string.Empty;
    private Margins _margins = new(0, 0, 0);
    private string _effect = string.Empty;
    private string _text = string.Empty;

    /// <summary>
    /// Event ID
    /// </summary>
    public int Id => id;

    /// <summary>
    /// If the event is a comment
    /// </summary>
    public bool IsComment
    {
        get => _isComment;
        set => SetProperty(ref _isComment, value);
    }

    /// <summary>
    /// Z-Layer of the event
    /// </summary>
    public int Layer
    {
        get => _layer;
        set => SetProperty(ref _layer, value);
    }

    /// <summary>
    /// Time the event appears on the screen
    /// </summary>
    public Time Start
    {
        get => _start;
        set => SetProperty(ref _start, value);
    }

    /// <summary>
    /// Time the event ends at
    /// </summary>
    public Time End
    {
        get => _end;
        set => SetProperty(ref _end, value);
    }

    /// <summary>
    /// Name of the style applied to this event
    /// </summary>
    [NotNull]
    public string? Style
    {
        get => _style;
        set
        {
            if (value is not null)
                SetProperty(ref _style, value);
        }
    }

    /// <summary>
    /// Name of the event's actor
    /// </summary>
    public string Actor
    {
        get => _actor;
        set => SetProperty(ref _actor, value);
    }

    /// <summary>
    /// Margins
    /// </summary>
    public Margins Margins
    {
        get => _margins;
        set => SetProperty(ref _margins, value);
    }

    /// <summary>
    /// Effect data
    /// </summary>
    public string Effect
    {
        get => _effect;
        set => SetProperty(ref _effect, value);
    }

    /// <summary>
    /// Text content of the event
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            SetProperty(ref _text, value);
            RaisePropertyChanged(nameof(Cps));
            RaisePropertyChanged(nameof(MaxLineWidth));
        }
    }

    /// <summary>
    /// Row number in the document
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// IDs of extradata linked to this event
    /// </summary>
    public List<int> LinkedExtradatas { get; set; } = [];

    /// <summary>
    /// Characters per second
    /// </summary>
    /// <remarks>It is recommended to keep dialogue events below 18 CPS.</remarks>
    public double Cps
    {
        get
        {
            var secs = (End - Start).TotalSeconds;
            if (secs == 0)
                return 0;

            var input = NewlineHardSpaceRegex().Replace(GetStrippedText(), string.Empty);
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            var charCount = 0;

            var tee = StringInfo.GetTextElementEnumerator(input);
            while (tee.MoveNext())
            {
                var element = tee.GetTextElement();
                if (true)
                    if (char.IsWhiteSpace(element[0]))
                        continue;
                if (true)
                    if (char.IsPunctuation(element[0]))
                        continue;
                charCount++;
            }
            return Math.Round(charCount / secs);
        }
    }

    /// <summary>
    /// Length (in characters) of the longest line in the event
    /// </summary>
    public int MaxLineWidth
    {
        get
        {
            var lines = GetStrippedText().Split(["\\N", "\\n"], StringSplitOptions.None);
            return lines
                .Select(line =>
                {
                    var input = NewlineHardSpaceRegex().Replace(line, string.Empty);
                    if (string.IsNullOrWhiteSpace(input))
                        return 0;
                    var charCount = 0;

                    var tee = StringInfo.GetTextElementEnumerator(input);
                    while (tee.MoveNext())
                    {
                        var element = tee.GetTextElement();
                        if (Options.LineWidthIncludesWhitespace)
                            if (char.IsWhiteSpace(element[0]))
                                continue;
                        if (Options.LineWidthIncludesPunctuation)
                            if (char.IsPunctuation(element[0]))
                                continue;
                        charCount++;
                    }
                    return charCount;
                })
                .Max();
        }
    }

    /// <summary>
    /// Make inline Lua code snippets ass-compliant
    /// by encoding newlines and indentation to comments
    /// </summary>
    /// <returns>Ass-compliant Lua code</returns>
    /// <remarks>This function reverses <see cref="TransformAssToCode"/></remarks>
    public string TransformCodeToAss()
    {
        return CodeToAssRegex()
            .Replace(
                Text,
                m =>
                {
                    var spacesCount = m.Groups[2].Value.Length;
                    return $"--[[{spacesCount}]]";
                }
            );
    }

    /// <summary>
    /// Make inline Lua code snippets display
    /// across multiple lines by decoding
    /// newlines and indentation from comments
    /// </summary>
    /// <returns>Formatted Lua code</returns>
    /// <remarks>This function reverses <see cref="TransformCodeToAss"/></remarks>
    public string TransformAssToCode()
    {
        return AssToCodeRegex()
            .Replace(
                Text,
                m =>
                {
                    var spacesCount = int.Parse(m.Groups[1].Value);
                    return Environment.NewLine + new string(' ', spacesCount);
                }
            );
    }

    /// <summary>
    /// Check if this event collides with another event.
    /// Events collide if their timestamps overlap.
    /// </summary>
    /// <param name="other">Event to check against</param>
    /// <returns><see langword="true"/> if the events collide</returns>
    public bool CollidesWith(Event other)
    {
        return (Start < other.Start) ? (other.Start < End) : (Start < other.End);
    }

    /// <summary>
    /// Set fields in this event to the fields from another event
    /// </summary>
    /// <param name="fields">Fields to set</param>
    /// <param name="other">Source of new fields</param>
    public void SetFields(EventField fields, Event other)
    {
        foreach (EventField field in fields.GetSetFlags())
        {
            switch (field)
            {
                case EventField.Comment:
                    IsComment = other.IsComment;
                    break;
                case EventField.Layer:
                    Layer = other.Layer;
                    break;
                case EventField.StartTime:
                    Start = Time.FromTime(other.Start);
                    break;
                case EventField.EndTime:
                    End = Time.FromTime(other.End);
                    break;
                case EventField.Style:
                    Style = other.Style;
                    break;
                case EventField.Actor:
                    Actor = other.Actor;
                    break;
                case EventField.MarginLeft:
                    Margins.Left = other.Margins.Left;
                    break;
                case EventField.MarginRight:
                    Margins.Right = other.Margins.Right;
                    break;
                case EventField.MarginVertical:
                    Margins.Vertical = other.Margins.Vertical;
                    break;
                case EventField.Effect:
                    Effect = other.Effect;
                    break;
                case EventField.Text:
                    Text = other.Text;
                    break;
                case EventField.None:
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Get the ass representation of this event
    /// </summary>
    /// <returns>Ass-formatted string</returns>
    public string AsAss()
    {
        var extradatas =
            LinkedExtradatas.Count > 0 ? $"{{{string.Join("=", LinkedExtradatas)}}}" : "";
        var textContent = Effect.Contains("code") ? TransformCodeToAss() : Text;

        return $"{(IsComment ? "Comment" : "Dialogue")}: {Layer},{Start.AsAss()},{End.AsAss()},{Style},{Actor},"
            + $"{Margins.Left},{Margins.Right},{Margins.Vertical},{Effect},{extradatas}"
            + $"{textContent}";
    }

    /// <summary>
    /// Initialize an event from an ass-formatted string
    /// </summary>
    /// <param name="id">ID of the event to create</param>
    /// <param name="data">Ass-formatted string</param>
    /// <returns>Event object represented by the string</returns>
    /// <exception cref="ArgumentException">If the data is malformed</exception>
    public static Event FromAss(int id, string data)
    {
        var match = EventRegex().Match(data);
        if (!match.Success)
            throw new ArgumentException($"Event {data} is invalid or malformed.");

        return new Event(id)
        {
            _isComment = match.Groups[1].Value == "Comment",
            _layer = Convert.ToInt32(match.Groups[2].Value),
            _start = Time.FromAss(match.Groups[3].Value),
            _end = Time.FromAss(match.Groups[4].Value),
            _style = match.Groups[5].Value,
            _actor = match.Groups[6].Value,
            _margins =
            {
                Left = match.Groups[7].Value.ParseAssInt(),
                Right = match.Groups[8].Value.ParseAssInt(),
                Vertical = match.Groups[9].Value.ParseAssInt(),
            },
            _effect = match.Groups[10].Value,
            _text = match.Groups[11].Value,
            LinkedExtradatas = ParseExtradata(data),
        };
    }

    /// <summary>
    /// Initialize a congruent event
    /// </summary>
    /// <param name="id">ID of the event to create</param>
    /// <param name="e">Base event</param>
    /// <returns>Congruent event</returns>
    public static Event FromEvent(int id, Event e)
    {
        return new Event(id)
        {
            _isComment = e.IsComment,
            _layer = e.Layer,
            _start = Time.FromTime(e.Start),
            _end = Time.FromTime(e.End),
            _style = e.Style,
            _actor = e.Actor,
            _margins =
            {
                Left = e.Margins.Left,
                Right = e.Margins.Right,
                Vertical = e.Margins.Vertical,
            },
            _effect = e.Effect,
            _text = e.Text,
        };
    }

    /// <summary>
    /// Clone this event
    /// </summary>
    /// <returns>Clone of the event</returns>
    public Event Clone()
    {
        return new Event(Id)
        {
            IsComment = IsComment,
            Layer = Layer,
            Start = Time.FromTime(Start),
            End = Time.FromTime(End),
            Style = Style,
            Actor = Actor,
            Margins = new Margins(Margins.Left, Margins.Right, Margins.Vertical),
            Effect = Effect,
            Text = Text,
            LinkedExtradatas = [.. LinkedExtradatas],
        };
    }

    /// <summary>
    /// Clone this event with a new ID
    /// </summary>
    /// <param name="eventId">New ID</param>
    /// <returns>Clone of the event</returns>
    public Event Clone(int eventId)
    {
        return new Event(eventId)
        {
            IsComment = IsComment,
            Layer = Layer,
            Start = Time.FromTime(Start),
            End = Time.FromTime(End),
            Style = Style,
            Actor = Actor,
            Margins = new Margins(Margins.Left, Margins.Right, Margins.Vertical),
            Effect = Effect,
            Text = Text,
            LinkedExtradatas = [.. LinkedExtradatas],
        };
    }

    /// <summary>
    /// Validate if a given string is a valid Event
    /// </summary>
    /// <param name="data">Data to validate</param>
    /// <returns><see langword="true"/> if the <paramref name="data"/> is valid</returns>
    public static bool ValidateAssString(string data)
    {
        return EventRegex().Match(data).Success;
    }

    #region Tags n stuff

    /// <summary>
    /// Parse the event into a list of Blocks
    /// </summary>
    /// <returns>List of Blocks representing the event</returns>
    public List<Block> ParseText()
    {
        List<Block> blocks = [];
        if (_text.Length <= 0)
        {
            blocks.Add(new PlainBlock(string.Empty));
            return blocks;
        }

        var data = _text.AsSpan();
        var drawingLevel = 0;

        while (!data.IsEmpty)
        {
            var p = data[0];

            if (p is '{' && data.IndexOf('}') is var q and >= 0)
            {
                // Comment
                if (data[..q].IndexOf('\\') < 0)
                {
                    var block = new CommentBlock(data[1..q].ToString());
                    blocks.Add(block);
                    data = data[(q + 1)..];
                }
                // Override
                else
                {
                    var block = new OverrideBlock(data[..q]);
                    blocks.Add(block);
                    data = data[(q + 1)..];

                    // Search for drawings
                    foreach (var pTag in block.Tags.OfType<OverrideTag.P>())
                    {
                        drawingLevel = pTag.Level;
                    }
                }
            }
            else if (drawingLevel != 0)
            {
                q = (p == '{' ? data[1..] : data).IndexOf('{');
                if (q < 0)
                    q = data.Length;
                blocks.Add(new DrawingBlock(data[..q].ToString(), drawingLevel));
                data = data[q..];
            }
            else
            {
                q = data.IndexOf('{');
                if (q < 0)
                    q = data.Length;

                blocks.Add(new PlainBlock(data[..q].ToString()));
                data = data[q..];
            }
        }

        return blocks;
    }

    /// <summary>
    /// Get the event's text content without override tags,
    /// comments, or drawings
    /// </summary>
    /// <returns>Stripped text content</returns>
    public string GetStrippedText()
    {
        var blocks = ParseText();
        return string.Join(string.Empty, blocks.OfType<PlainBlock>().Select(b => b.Text));
    }

    /// <summary>
    /// Strip away all tags in the event
    /// </summary>
    /// <seealso cref="GetStrippedText"/>
    public void StripTags()
    {
        Text = GetStrippedText();
    }

    /// <summary>
    /// Toggle a tag
    /// </summary>
    /// <param name="tag">Tag to toggle</param>
    /// <param name="style">Event style</param>
    /// <param name="selStart">Start of the selection</param>
    /// <param name="selEnd">End of the selection</param>
    /// <returns>Number of characters to shift the cursor</returns>
    public int ToggleTag(string tag, Style? style, int selStart, int selEnd)
    {
        if (selStart > selEnd)
            (selStart, selEnd) = (selEnd, selStart);

        var normSelStart = NormalizePos(selStart);
        var normSelEnd = NormalizePos(selEnd);

        var parsed = new ParsedEvent(this);
        var blockN = parsed.BlockAt(normSelStart);

        var state = false;
        // var (state, tagType) = tag switch
        // {
        //     "\\b" => (, typeof(OverrideTag.B)),
        //     "\\i" => (style?.IsItalic, typeof(OverrideTag.I)),
        //     "\\u" => (style?.IsUnderline, typeof(OverrideTag.U)),
        //     "\\s" => (style?.IsStrikethrough, typeof(OverrideTag.S)),
        //     _ => (false, typeof(OverrideTag.Unknown)),
        // };
        // state ??= false; // If style is null

        var blockTag = parsed.FindTag(blockN, tag, "");
        switch (tag)
        {
            case @"\b":
                state = style?.IsBold ?? false;
                if (blockTag is OverrideTag.B { Value: not null } b)
                    state = b.Value.Value;
                break;
            case @"\i":
                state = style?.IsItalic ?? false;
                if (blockTag is OverrideTag.I { Value: not null } i)
                    state = i.Value.Value;
                break;
            case @"\u":
                state = style?.IsUnderline ?? false;
                if (blockTag is OverrideTag.U { Value: not null } u)
                    state = u.Value.Value;
                break;
            case @"\s":
                state = style?.IsStrikethrough ?? false;
                if (blockTag is OverrideTag.S { Value: not null } s)
                    state = s.Value.Value;
                break;
        }

        var shift = parsed.SetTag(tag, state ? "0" : "1", normSelStart, selStart);
        if (selStart != selEnd)
            parsed.SetTag(tag, state ? "1" : "0", normSelEnd, selEnd + shift);
        return shift;
    }

    /// <summary>
    /// Replace the text in this line.
    /// Operation is skipped if the input is empty.
    /// </summary>
    /// <param name="blocks">Blocks to set</param>
    public void UpdateText(List<Block> blocks)
    {
        if (blocks.Count == 0)
            return;
        Text = string.Join(string.Empty, blocks.Select(b => b.Text));
    }

    #endregion

    public override bool Equals(object? obj)
    {
        return obj is Event @event && Id == @event.Id && IsCongruentWith(@event);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Checks equality of non-<see cref="Id"/> fields
    /// </summary>
    /// <param name="obj">Event to check</param>
    /// <returns><see langword="true"/> if,
    /// excluding the <see cref="Id"/>, <paramref name="obj"/> is equal.</returns>
    public bool IsCongruentWith(Event? obj)
    {
        return obj is { } @event
            && _isComment == @event._isComment
            && _layer == @event._layer
            && _start == @event._start
            && _end == @event._end
            && _style == @event._style
            && _actor == @event._actor
            && _margins == @event._margins
            && _effect == @event._effect
            && _text == @event._text;
    }

    public static bool operator ==(Event? left, Event? right)
    {
        return left?.Equals(right) ?? false;
    }

    public static bool operator !=(Event? left, Event? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Parse the event's extradata
    /// </summary>
    /// <param name="data">Ass-formatted string</param>
    /// <returns>List of extradata IDs</returns>
    private static List<int> ParseExtradata(string data)
    {
        if (data.Length < 2)
            return [];
        if (!data.StartsWith("{="))
            return [];
        var match = ExtradataRegex().Match(data);
        if (!match.Success)
            return [];

        List<int> result = [];

        for (var i = 1; i < match.Groups.Count; i++)
        {
            var rawId = match.Groups[i].Value[1..]; // =123 → 123
            var id = Convert.ToInt32(rawId);
            result.Add(id);
        }
        return result;
    }

    #region Private tag stuff

    /// <summary>
    /// Normalize positions inside the text
    /// </summary>
    /// <param name="pos">Original position</param>
    /// <returns>Normalized position</returns>
    private int NormalizePos(int pos)
    {
        var plainLength = 0;
        var inside = false;
        for (int i = 0, max = Text.Length - 1; i < pos && i <= max; i++)
        {
            if (Text[i] == '{')
                inside = true;
            if (!inside)
                plainLength++;
            if (Text[i] == '}' && inside)
                inside = false;
        }
        return plainLength;
    }

    private class ParsedEvent
    {
        readonly Event _line;
        List<Block> _blocks;

        public ParsedEvent(Event line)
        {
            _line = line;
            _blocks = _line.ParseText();
        }

        /// <summary>
        /// Find the tag with the given name
        /// </summary>
        /// <param name="blockN">Block number to check</param>
        /// <param name="tagName">Name of the tag</param>
        /// <param name="alt">Alternate name for the tag</param>
        /// <returns>The tag, or <see langword="null"/> if not found</returns>
        public OverrideTag? FindTag(int blockN, string tagName, string alt)
        {
            return _blocks
                .GetRange(0, blockN + 1)
                .AsEnumerable()
                .Reverse()
                .OfType<OverrideBlock>()
                .SelectMany(ovr => ovr.Tags.AsEnumerable().Reverse())
                .FirstOrDefault(tag => tag.Name == tagName || tag.Name == alt);
        }

        /// <summary>
        /// Get the block number for the text at the index
        /// </summary>
        /// <param name="index">Index in the text to look up</param>
        /// <returns>Block number</returns>
        public int BlockAt(int index)
        {
            var n = 0;
            var inside = false;
            for (var i = 0; i <= _line.Text.Length - 1; i++)
            {
                switch (_line.Text[i])
                {
                    case '{':
                        if (!inside && i > 0 && index >= 0)
                            n++;
                        inside = true;
                        break;

                    case '}' when inside:
                        inside = false;
                        if (
                            index > 0
                            && (i + 1 == _line.Text.Length - 1 || _line.Text[i + 1] != '{')
                        )
                            n++;
                        break;

                    default:
                        if (!inside)
                        {
                            if (--index == 0)
                                return n
                                    + (
                                        (i < _line.Text.Length - 1 && _line.Text[i + 1] == '{')
                                            ? 1
                                            : 0
                                    );
                        }
                        break;
                }
            }
            return n - (inside ? 1 : 0);
        }

        /// <summary>
        /// Set the value of a tag
        /// </summary>
        /// <param name="tag">Tag to set</param>
        /// <param name="value">New value</param>
        /// <param name="normPos">Normalized position</param>
        /// <param name="originPos">Original position</param>
        /// <exception cref="ArgumentOutOfRangeException">If the <see cref="BlockType"/> is invalid</exception>
        /// <returns>Number of characters to shift the caret</returns>
        public int SetTag(string tag, string value, int normPos, int originPos)
        {
            var blockN = BlockAt(normPos);
            PlainBlock? plain = null;
            OverrideBlock? ovr = null;
            while (blockN >= 0 && plain is null && ovr is null)
            {
                var block = _blocks[blockN];
                switch (block.Type)
                {
                    case BlockType.Plain:
                        plain = (PlainBlock)block;
                        break;
                    case BlockType.Drawing:
                        --blockN;
                        break;
                    case BlockType.Comment:
                        --blockN;
                        originPos = _line.Text.IndexOf('{', originPos);
                        break;
                    case BlockType.Override:
                        ovr = (OverrideBlock)block;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Invalid block type: {block.Type}");
                }
            }

            // If there is no suitable block, place it at the beginning of the line
            if (blockN < 0)
                originPos = 0;

            var insert = tag + value;
            var shift = insert.Length;

            if (plain is not null || blockN < 0)
            {
                _line.Text = string.Concat(
                    _line.Text.AsSpan(0, originPos),
                    $"{{{insert}}}",
                    _line.Text.AsSpan(originPos)
                );
                shift += 2;
                _blocks = _line.ParseText();
            }
            else if (ovr is not null)
            {
                var alt = string.Empty;
                if (tag == "\\c")
                    alt = "\\1c";
                var found = false;
                for (var i = 0; i < ovr.Tags.Count; i++)
                {
                    var name = ovr.Tags[i].Name;
                    if (tag != name && alt != name)
                        continue;

                    shift -= (ovr.Tags[i].ToString()).Length;
                    if (found)
                    {
                        ovr.Tags.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        // TODO Reimplement
                        // ovr.Tags[i].Parameters[0].Set(value);
                        found = true;
                    }
                }
                // TODO Reimplement
                // if (!found)
                //     ovr.AddTag(insert);

                _line.UpdateText(_blocks);
            }
            return shift;
        }
    }

    #endregion

    [GeneratedRegex(
        @"^(Comment|Dialogue):\ (\d+),\s*(\d+:\d+:\d+.\d+),\s*(\d+:\d+:\d+.\d+),\s*([^,]*),\s*([^,]*),\s*([^,]*),\s*([^,]*),\s*([^,]*),\s*([^,]*),(.*)"
    )]
    private static partial Regex EventRegex();

    [GeneratedRegex(@"^\{(=\d+)+\}")]
    private static partial Regex ExtradataRegex();

    [GeneratedRegex(@"(\r\n|\r|\n)([\ |\t]*)")]
    private static partial Regex CodeToAssRegex();

    [GeneratedRegex(@"--\[\[([0-9]+)\]\]")]
    private static partial Regex AssToCodeRegex();

    [GeneratedRegex(@"\\[Nnh]")]
    private static partial Regex NewlineHardSpaceRegex();
}
