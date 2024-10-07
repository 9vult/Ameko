// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;
using AssCS.Overrides;
using AssCS.Overrides.Blocks;

namespace AssCS;

/// <summary>
/// An event in a subtitle document
/// </summary>
/// <param name="id">ID of the event</param>
public class Event(int id) : BindableBase, IEntry
{
    private readonly int _id = id;
    private bool _isComment = false;
    private int _layer = 0;
    private Time _start = Time.FromSeconds(0);
    private Time _end = Time.FromSeconds(5);
    private string _style = "Default";
    private string _actor = string.Empty;
    private Margins _margins = new(20, 20, 20);
    private string _effect = string.Empty;
    private string _text = string.Empty;
    private int _index;

    /// <summary>
    /// Event ID
    /// </summary>
    public int Id => _id;

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
    public string Style
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
    public int Index
    {
        get => _index;
        set => _index = value;
    }

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
            var text = GetStrippedText()
                .Replace("\\N", string.Empty)
                .Replace("\\n", string.Empty)
                .Replace(" ", string.Empty);
            return Math.Round(text.Length / (End - Start).TotalSeconds);
        }
    }

    /// <summary>
    /// Length (in characters) of the longest line in the event
    /// </summary>
    public int MaxLineWidth =>
        GetStrippedText()
            .Replace(" ", string.Empty)
            .Split(["\\N", "\\n"], StringSplitOptions.None)
            .Select(l => l.Length)
            .Max();

    /// <summary>
    /// Make inline Lua code snippets ass-compliant
    /// by encoding newlines and indentation to comments
    /// </summary>
    /// <returns>Ass-compliant Lua code</returns>
    /// <remarks>This function reverses <see cref="TransformAssToCode"/></remarks>
    public string TransformCodeToAss()
    {
        var pattern = @"(\r\n|\r|\n)([\ |\t]*)";
        return Regex.Replace(
            Text,
            pattern,
            m =>
            {
                int spacesCount = m.Groups[2].Value.Length;
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
        var pattern = @"--\[\[([0-9]+)\]\]";
        return Regex.Replace(
            Text,
            pattern,
            m =>
            {
                int spacesCount = int.Parse(m.Groups[1].Value);
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
        if (other == null)
            return false;
        return (Start < other.Start) ? (other.Start < End) : (Start < other.End);
    }

    /// <summary>
    /// Get the ass representation of this event
    /// </summary>
    /// <returns>Ass-formatted string</returns>
    public string AsAss()
    {
        string extradatas =
            LinkedExtradatas.Count > 0 ? $"{{{string.Join("=", LinkedExtradatas)}}}" : "";
        var textContent = Effect.Contains("code") ? TransformCodeToAss() : Text;

        return $"{(IsComment ? "Comment" : "Dialogue")}: {Layer},{Start.AsAss()},{End.AsAss()},{Style},{Actor},"
            + $"{Margins.Left},{Margins.Right},{Margins.Vertical},{Effect},{extradatas}"
            + $"{textContent}";
    }

    /// <summary>
    /// Initialize an event from an ass-formatted string
    /// </summary>
    /// <param name="id">Id of the event to create</param>
    /// <param name="data">Ass-formatted string</param>
    /// <returns>Event object represented by the string</returns>
    /// <exception cref="ArgumentException">If the data is malformed</exception>
    public static Event FromAss(int id, string data)
    {
        var eventRegex =
            @"^(Comment|Dialogue):\ (\d+),(\d+:\d+:\d+.\d+),(\d+:\d+:\d+.\d+),([^,]*),([^,]*),(-?\d+),(-?\d+),(-?\d+),([^,]*),(.*)";
        var match = Regex.Match(data, eventRegex);
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
                Left = Convert.ToInt32(match.Groups[7].Value),
                Right = Convert.ToInt32(match.Groups[8].Value),
                Vertical = Convert.ToInt32(match.Groups[9].Value),
            },
            _effect = match.Groups[10].Value,
            _text = match.Groups[11].Value,
            LinkedExtradatas = ParseExtradata(data),
        };
    }

    /// <summary>
    /// Clone this event
    /// </summary>
    /// <returns>Clone of the event</returns>
    /// <remarks>
    /// This method currently uses serialization
    /// as the clone method. This is subject to change.
    /// </remarks>
    public Event Clone()
    {
        return FromAss(Id, AsAss());
    }

    #region Tags n stuff

    /// <summary>
    /// Parse the event into a list of Blocks
    /// </summary>
    /// <returns>List of Blocks representing the event</returns>
    public List<Block> ParseTags()
    {
        List<Block> blocks = [];
        if (_text.Length <= 0)
        {
            blocks.Add(new PlainBlock(string.Empty));
            return blocks;
        }

        int drawingLevel = 0;
        string text = new(_text);
        string work;
        int endPlain;

        for (int len = text.Length, cur = 0; cur < len; )
        {
            // Override block
            if (text[cur] == '{')
            {
                int end = text.IndexOf('}', cur);
                if (end == -1)
                {
                    // ----- Plain -----
                    endPlain = text.IndexOf('{', cur + 1);
                    if (endPlain == -1)
                    {
                        work = text[cur..];
                        cur = len;
                    }
                    else
                    {
                        work = text[cur..endPlain];
                        cur = endPlain;
                    }
                    if (drawingLevel == 0)
                        blocks.Add(new PlainBlock(work));
                    else
                        blocks.Add(new DrawingBlock(work, drawingLevel));
                    // ----- End Plain -----
                }
                else
                {
                    ++cur;
                    // Get block contents
                    work = text[cur..end];
                    cur = end + 1;

                    if (work.Length > 0 && !work.Contains('\\'))
                    {
                        // Comment
                        blocks.Add(new CommentBlock(work));
                    }
                    else
                    {
                        // Create block
                        var block = new OverrideBlock(work);
                        block.ParseTags();
                        // Search for drawings
                        foreach (var tag in block.Tags)
                        {
                            if (tag.Name == "\\p")
                                drawingLevel = tag.Parameters[0].GetInt();
                        }
                        blocks.Add(block);
                    }
                    continue;
                }
            }
            // ----- Plain 2 electric boogaloo -----
            if (cur + 1 < text.Length)
            {
                endPlain = text.IndexOf('{', cur + 1);
                if (endPlain == -1)
                {
                    work = text[cur..];
                    cur = len;
                }
                else
                {
                    work = text[cur..endPlain];
                    cur = endPlain;
                }
                if (drawingLevel == 0)
                    blocks.Add(new PlainBlock(work));
                else
                    blocks.Add(new DrawingBlock(work, drawingLevel));
            }
            else
            {
                cur += 1;
            }
            // ----- End Plain -----
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
        var blocks = ParseTags();
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

        int normSelStart = NormalizePos(selStart);
        int normSelEnd = NormalizePos(selEnd);

        bool state =
            style is not null
            && tag switch
            {
                "\\b" => style.IsBold,
                "\\i" => style.IsItalic,
                "\\u" => style.IsUnderline,
                "\\s" => style.IsStrikethrough,
                _ => false,
            };

        ParsedEvent parsed = new ParsedEvent(this);
        int blockn = parsed.BlockAt(normSelStart);
        state = parsed.FindTag(blockn, tag, "")?.Parameters[0].GetBool() ?? state;

        int shift = parsed.SetTag(tag, state ? "0" : "1", normSelStart, selStart);
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
        return obj is Event @event
            && _id == @event._id
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

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_id);
        hash.Add(_isComment);
        hash.Add(_layer);
        hash.Add(_start);
        hash.Add(_end);
        hash.Add(_style);
        hash.Add(_actor);
        hash.Add(_margins);
        hash.Add(_effect);
        hash.Add(_text);
        return hash.ToHashCode();
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
        var extraRegex = @"^\{(=\d+)+\}";
        var match = Regex.Match(data, extraRegex);
        if (!match.Success)
            return [];

        List<int> result = [];

        for (int i = 1; i < match.Groups.Count; i++)
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
        int plainLength = 0;
        bool inside = false;
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
        readonly Event Line;
        List<Block> Blocks;

        public ParsedEvent(Event line)
        {
            Line = line;
            Blocks = Line.ParseTags();
        }

        /// <summary>
        /// Find the tag with the given name
        /// </summary>
        /// <param name="blockn">Block number to check</param>
        /// <param name="tagName">Name of the tag</param>
        /// <param name="alt">Alternate name for the tag</param>
        /// <returns>The tag, or <see langword="null"/> if not found</returns>
        public OverrideTag? FindTag(int blockn, string tagName, string alt)
        {
            foreach (
                var ovr in Blocks
                    .GetRange(0, blockn + 1)
                    .AsEnumerable()
                    .Reverse()
                    .OfType<OverrideBlock>()
            )
            {
                foreach (var tag in ovr.Tags.AsEnumerable().Reverse())
                {
                    if (tag.Name == tagName || tag.Name == alt)
                        return tag;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the block number for the text at the index
        /// </summary>
        /// <param name="index">Index in the text to look up</param>
        /// <returns>Block number</returns>
        public int BlockAt(int index)
        {
            int n = 0;
            bool inside = false;
            for (var i = 0; i <= Line.Text.Length - 1; i++)
            {
                if (Line.Text[i] == '{')
                {
                    if (!inside && i > 0 && index >= 0)
                        n++;
                    inside = true;
                }
                else if (Line.Text[i] == '}' && inside)
                {
                    inside = false;
                    if (index > 0 && (i + 1 == Line.Text.Length - 1 || Line.Text[i + 1] != '{'))
                        n++;
                }
                else if (!inside)
                {
                    if (--index == 0)
                        return n + ((i < Line.Text.Length - 1 && Line.Text[i + 1] == '{') ? 1 : 0);
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
        /// <returns>Number of characters to shift the caret</returns>
        public int SetTag(string tag, string value, int normPos, int originPos)
        {
            int blockn = BlockAt(normPos);
            PlainBlock? plain = null;
            OverrideBlock? ovr = null;
            while (blockn >= 0 && plain is null && ovr is null)
            {
                Block block = Blocks[blockn];
                switch (block.Type)
                {
                    case BlockType.Plain:
                        plain = (PlainBlock)block;
                        break;
                    case BlockType.Drawing:
                        --blockn;
                        break;
                    case BlockType.Comment:
                        --blockn;
                        originPos = Line.Text.IndexOf('{', originPos);
                        break;
                    case BlockType.Override:
                        ovr = (OverrideBlock)block;
                        break;
                }
            }

            // If there is no suitable block, place it at the beginning of the line
            if (blockn < 0)
                originPos = 0;

            string insert = tag + value;
            int shift = insert.Length;

            if (plain is not null || blockn < 0)
            {
                Line.Text = string.Concat(
                    Line.Text.AsSpan(0, originPos),
                    $"{{{insert}}}",
                    Line.Text.AsSpan(originPos)
                );
                shift += 2;
                Blocks = Line.ParseTags();
            }
            else if (ovr is not null)
            {
                string alt = string.Empty;
                if (tag == "\\c")
                    alt = "\\1c";
                bool found = false;
                for (var i = 0; i < ovr.Tags.Count; i++)
                {
                    var name = ovr.Tags[i].Name;
                    if (tag == name || alt == name)
                    {
                        shift -= (ovr.Tags[i].ToString()).Length;
                        if (found)
                        {
                            ovr.Tags.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            ovr.Tags[i].Parameters[0].Set(value);
                            found = true;
                        }
                    }
                }
                if (!found)
                    ovr.AddTag(insert);

                Line.UpdateText(Blocks);
            }
            else
            {
                // ?
            }
            return shift;
        }
    }
    #endregion
}
