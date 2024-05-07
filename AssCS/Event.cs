using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace AssCS
{
    /// <summary>
    /// The meat and potatoes of the subtitle file
    /// </summary>
    public class Event : IAssComponent, ICommitable, INotifyPropertyChanged
    {
        private int _id;
        private bool _comment;
        private int _layer;
        private Time _start;
        private Time _end;
        private string _style;
        private string _actor;
        private Margins _margins;
        private string _effect;
        private string _text;

        public int Id { get => _id; }
        public bool Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(nameof(Comment)); }
        }
        public int Layer
        {
            get => _layer;
            set { _layer = value; OnPropertyChanged(nameof(Layer)); }
        }
        public Time Start
        {
            get => _start;
            set { _start = value; OnPropertyChanged(nameof(Start)); }
        }
        public Time End
        {
            get => _end;
            set { _end = value; OnPropertyChanged(nameof(End)); }
        }
        public string Style
        {
            get => _style;
            set { _style = value; OnPropertyChanged(nameof(Style)); }
        }
        public string Actor
        {
            get => _actor;
            set { _actor = value; OnPropertyChanged(nameof(Actor)); }
        }
        public Margins Margins
        {
            get => _margins;
            set { _margins = value; OnPropertyChanged(nameof(Margins)); }
        }
        public string Effect
        {
            get => _effect;
            set { _effect = value; OnPropertyChanged(nameof(Effect)); }
        }
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
                OnPropertyChanged(nameof(Cps));
                OnPropertyChanged(nameof(MaxLineWidth));
            }
        }
        public List<int> LinkedExtradatas { get; set; }

        /// <summary>
        /// Characters per second. It is recommended to keep dialogue events under 18 CPS.
        /// </summary>
        public double Cps
        {
            get
            {
                var secs = (End - Start).TotalSeconds;
                if (secs == 0) return 0;
                var text = GetStrippedText().Replace("\\N", string.Empty).Replace(" ", string.Empty);
                return Math.Round(text.Length / (End - Start).TotalSeconds);
            }
        }
        /// <summary>
        /// Maximum line length (in characters).
        /// </summary>
        public int MaxLineWidth => GetStrippedText().Split("\\N").Select(l => l.Length).Max();

        /// <summary>
        /// Bootstrap this event from its representation in a file
        /// </summary>
        /// <param name="data">Line</param>
        public void FromAss(string data)
        {
            var eventRegex = @"^(Comment|Dialogue):\ (\d+),(\d+:\d+:\d+.\d+),(\d+:\d+:\d+.\d+),([^,]*),([^,]*),(-?\d+),(-?\d+),(-?\d+),([^,]*),(.*)";
            var match = Regex.Match(data, eventRegex);
            if (!match.Success) throw new ArgumentException($"Event {data} is invalid or malformed.");

            Comment = match.Groups[1].Value == "Comment";
            Layer = Convert.ToInt32(match.Groups[2].Value);
            Start = Time.FromAss(match.Groups[3].Value);
            End = Time.FromAss(match.Groups[4].Value);
            Style = match.Groups[5].Value;
            Actor = match.Groups[6].Value;
            Margins.Left = Convert.ToInt32(match.Groups[7].Value);
            Margins.Right = Convert.ToInt32(match.Groups[8].Value);
            Margins.Vertical = Convert.ToInt32(match.Groups[9].Value);
            Effect = match.Groups[10].Value;
            Text = match.Groups[11].Value;
            LoadExtradata(data);
        }

        private void LoadExtradata(string data)
        {
            if (data.Length < 2) return;
            if (!data.StartsWith("{=")) return;
            var extraRegex = @"^\{(=\d+)+\}";
            var match = Regex.Match(data, extraRegex);
            if (!match.Success) return;

            for (int i = 1; i < match.Groups.Count; i++)
            {
                var rawId = match.Groups[i].Value[1..]; // =123 → 123
                var id = Convert.ToInt32(rawId);
                LinkedExtradatas.Add(id);
            }

        }

        #region tags

        /// <summary>
        /// Parse this event's override tags
        /// </summary>
        /// <returns>This event, split up into a list of Blocks</returns>
        public List<Block> ParseTags()
        {
            List<Block> blocks = new List<Block>();
            if (Text.Length <= 0)
            {
                blocks.Add(new PlainBlock(""));
                return blocks;
            }

            int drawingLevel = 0;
            string text = string.Copy(Text);
            string work;
            int endPlain;
            for (int len = text.Length, cur = 0; cur < len;)
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
                            work = text.Substring(cur);
                            cur = len;
                        }
                        else
                        {
                            work = text.Substring(cur, endPlain - cur);
                            cur = endPlain;
                        }
                        if (drawingLevel == 0) blocks.Add(new PlainBlock(work));
                        else blocks.Add(new DrawingBlock(work, drawingLevel));
                        // ----- End Plain -----
                    }
                    else
                    {
                        ++cur;
                        // Get block contents
                        work = text.Substring(cur, end - cur);
                        cur = end + 1;

                        if (work.Length > 0 && work.IndexOf('\\') == -1)
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
                                if (tag.Name == "\\p") drawingLevel = tag.Parameters[0].GetInt();
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
                        work = text.Substring(cur);
                        cur = len;
                    }
                    else
                    {
                        work = text.Substring(cur, endPlain - cur);
                        cur = endPlain;
                    }
                    if (drawingLevel == 0) blocks.Add(new PlainBlock(work));
                    else blocks.Add(new DrawingBlock(work, drawingLevel));
                } else
                {
                    cur += 1;
                }
                // ----- End Plain -----
            }
            return blocks;
        }

        /// <summary>
        /// Strip away any override tags or comments
        /// </summary>
        public void StripTags()
        {
            Text = GetStrippedText();
        }

        /// <summary>
        /// Get the text without override tags or comments
        /// </summary>
        /// <returns>Stripped text</returns>
        public string GetStrippedText()
        {
            var blocks = ParseTags();
            return string.Join("", blocks.OfType<PlainBlock>().Select(b => b.Text));
        }

        /// <summary>
        /// Toggle a tag
        /// </summary>
        /// <param name="tag">Tag to toggle</param>
        /// <param name="style">Style of the line</param>
        /// <param name="selStart">Selection start</param>
        /// <param name="selEnd">Selection end</param>
        /// <returns></returns>
        public int ToggleTag(string tag, Style? style, int selStart, int selEnd)
        {
            if (selStart > selEnd)
                (selStart, selEnd) = (selEnd, selStart);

            int normSelStart = NormalizePos(selStart);
            int normSelEnd = NormalizePos(selEnd);
            // TIL you can put switch statements inside ternary operators (I love this so much)
            bool state = style != null ? tag switch
            {
                "\\b" => style.Bold,
                "\\i" => style.Italic,
                "\\u" => style.Underline,
                "\\s" => style.Strikeout,
                _ => false
            } : false;

            ParsedEvent parsed = new ParsedEvent(this);
            int blockn = parsed.BlockAt(normSelStart);
            state = parsed.FindTag(blockn, tag, "")?.Parameters[0].GetBool() ?? state;

            int shift = parsed.SetTag(tag, state ? "0" : "1", normSelStart, selStart);
            if (selStart != selEnd)
                parsed.SetTag(tag, state ? "1" : "0", normSelEnd, selEnd + shift);
            return shift;
        }

        #endregion tags

        /// <summary>
        /// Check if this event collides with another event.
        /// Events collide if their timestamps overlap.
        /// </summary>
        /// <param name="other">Event to check against</param>
        /// <returns>True if the events collide</returns>
        public bool CollidesWith(Event other)
        {
            if (other == null) return false;
            return (Start < other.Start) ? (other.Start < End) : (Start < other.End);
        }

        /// <summary>
        /// Replace the text in this line.
        /// Operation is skipped if the input is empty.
        /// </summary>
        /// <param name="blocks"></param>
        public void UpdateText(List<Block> blocks)
        {
            if (blocks.Count == 0) return;
            Text = string.Join("", blocks.Select(b => b.Text));
        }

        public string AsAss()
        {
            string extradatas = LinkedExtradatas.Count > 0 ? $"{{{string.Join("=", LinkedExtradatas)}}}" : "";

            var textContent = Effect.Contains("code") ? TransformCodeToAss() : Text;

            return $"{(Comment ? "Comment" : "Dialogue")}: {Layer},{Start.AsAss()},{End.AsAss()},{Style},{Actor}," +
                $"{Margins.Left},{Margins.Right},{Margins.Vertical},{Effect},{extradatas}" +
                $"{textContent}";
        }

        public string? AsOverride() => null;

        /// <summary>
        /// Make Lua code snippets ass-compliant by
        /// transforming newlines to comments
        /// </summary>
        /// <returns>Ass-compliant text content</returns>
        public string TransformCodeToAss()
        {
            var pattern = @"(\r\n|\r|\n)([\ |\t]*)";
            return Regex.Replace(Text, pattern, m =>
            {
                int spacesCount = m.Groups[2].Value.Length;
                return $"--[[{spacesCount}]]";
            });
        }

        /// <summary>
        /// Convert ass-compliant Lua code snippets
        /// back to Lua with newlines
        /// </summary>
        /// <returns>Normal-looking Lua text content</returns>
        public string TransformAssToCode()
        {
            var pattern = @"--\[\[([0-9]+)\]\]";
            return Regex.Replace(Text, pattern, m =>
            {
                int spacesCount = int.Parse(m.Groups[1].Value);
                return Environment.NewLine + new string(' ', spacesCount);
            });
        }

        /// <summary>
        /// Clone an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="e"></param>
        public Event(int id, Event e)
        {
            _id = id;
            _comment = e.Comment;
            _layer = e.Layer;
            _start = new Time(e.Start);
            _end = new Time(e.End);
            _style = e.Style;
            _actor = e.Actor;
            _margins = new Margins(e.Margins);
            _effect = e.Effect;
            _text = e.Text;
            LinkedExtradatas = new List<int>(e.LinkedExtradatas);
        }

        /// <summary>
        /// Compose a new Event
        /// </summary>
        /// <param name="id"></param>
        public Event(int id)
        {
            _id = id;
            _comment = false;
            _layer = 0;
            _start = Time.FromSeconds(0);
            _end = Time.FromSeconds(5);
            _style = "Default";
            _actor = "";
            _margins = new Margins(0, 0, 0);
            _effect = "";
            _text = "";
            LinkedExtradatas = new List<int>();
        }

        /// <summary>
        /// Load an event from a string
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public Event(int id, string data) : this(id)
        {
            FromAss(data);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!obj.GetType().Equals(typeof(Event))) return false;
            Event e = (Event)obj;
            if (Style == null || e.Style == null) return false;
            return Id == e.Id
                && Comment == e.Comment
                && Layer == e.Layer
                && Start == e.Start
                && End == e.End
                && Style.Equals(e.Style)
                && Actor.Equals(e.Actor)
                && Margins.Equals(e.Margins)
                && Effect.Equals(e.Effect)
                && Text.Equals(e.Text);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Comment);
            hash.Add(Layer);
            hash.Add(Start);
            hash.Add(End);
            hash.Add(Style);
            hash.Add(Actor);
            hash.Add(Margins);
            hash.Add(Effect);
            hash.Add(Text);
            return hash.ToHashCode();
        }

        public Event Clone()
        {
            return new Event(Id, this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
                if (Text[i] == '{') inside = true;
                if (!inside) plainLength++;
                if (Text[i] == '}' && inside) inside = false;
            }
            return plainLength;
        }

        private class ParsedEvent
        {
            Event Line;
            List<Block> Blocks;

            public ParsedEvent(Event line)
            {
                this.Line = line;
                this.Blocks = Line.ParseTags();
            }

            /// <summary>
            /// Find the tag with the given name
            /// </summary>
            /// <param name="blockn">Block number to check</param>
            /// <param name="tagName">Name of the tag</param>
            /// <param name="alt">Alternate name for the tag</param>
            /// <returns>The tag, or null if not found</returns>
            public OverrideTag? FindTag(int blockn, string tagName, string alt)
            {
                foreach (var ovr in Blocks.GetRange(0, blockn + 1).AsEnumerable().Reverse().OfType<OverrideBlock>())
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
            /// <returns></returns>
            public int SetTag(string tag, string value, int normPos, int originPos)
            {
                int blockn = BlockAt(normPos);
                PlainBlock? plain = null;
                OverrideBlock? ovr = null;
                while (blockn >= 0 && plain == null && ovr == null)
                {
                    Block block = Blocks[blockn];
                    switch (block.Type)
                    {
                        case BlockType.PLAIN:
                            plain = (PlainBlock)block;
                            break;
                        case BlockType.DRAWING:
                            --blockn;
                            break;
                        case BlockType.COMMENT:
                            --blockn;
                            originPos = Line.Text.IndexOf('{', originPos);
                            break;
                        case BlockType.OVERRIDE:
                            ovr = (OverrideBlock)block;
                            break;
                    }
                }

                // If there is no suitable block, place it at the beginning of the line
                if (blockn < 0) originPos = 0;

                string insert = tag + value;
                int shift = insert.Length;

                if (plain != null || blockn < 0)
                {
                    Line.Text = Line.Text.Substring(0, originPos) + $"{{{insert}}}" + Line.Text.Substring(originPos);
                    shift += 2;
                    Blocks = Line.ParseTags();
                }
                else if (ovr != null)
                {
                    string alt = string.Empty;
                    if (tag == "\\c") alt = "\\1c";
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
                    if (!found) ovr.AddTag(insert);

                    Line.UpdateText(Blocks);
                }
                else
                {
                    // ?
                }
                return shift;
            }
        }
    }
}
