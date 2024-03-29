﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssCS
{
    public class Karaoke
    {
        private List<Syllable> syllables;

        public string Text => string.Join("", syllables.Select(s => s.GetFormattedText(true)));
        public string TagType
        {
            get => syllables[0].TagType;
            set { foreach (var syl in syllables) syl.TagType = value; }
        }

        private void ParseSyllables(Event line, Syllable syl)
        {
            foreach (var block in line.ParseTags())
            {
                var text = block.Text;
                switch (block.Type)
                {
                    case BlockType.PLAIN:
                        syl.Text += text;
                        break;
                    case BlockType.COMMENT:
                    case BlockType.DRAWING:
                        syl.OverrideTags[syl.Text.Length] += text;
                        break;
                    case BlockType.OVERRIDE:
                        var b = (OverrideBlock)block;
                        bool inTag = false;
                        foreach (var tag in b.Tags)
                        {
                            if (tag.Valid && tag.Name.StartsWith("\\k"))
                            {
                                if (inTag)
                                {
                                    syl.OverrideTags[syl.Text.Length] += "}";
                                    inTag = false;
                                }
                                // Convert \K to \kf for convenience
                                if (tag.Name.Equals("\\K")) tag.Name = "\\kf";

                                // Exclude Zero duration-zero length
                                if (syl.Duration > 0 || !(syl.Text.Length == 0))
                                {
                                    syllables.Add(syl);
                                    syl.Text = string.Empty;
                                    syl.OverrideTags.Clear();
                                }

                                syl.TagType = tag.Name;
                                syl.Start += Time.FromMillis(syl.Duration);
                                syl.Duration = tag.Parameters[0].GetInt() * 10;
                            }
                            else
                            {
                                var otext = syl.OverrideTags[syl.Text.Length];
                                // Merge adjacent tags
                                if (text.EndsWith('}')) text = text.Substring(0, text.Length - 1);
                                if (!inTag) otext += '{';
                                inTag = true;
                                otext += tag;
                            }
                        }
                        if (inTag) syl.OverrideTags[syl.Text.Length] += '}';
                        break;
                }
            }
            syllables.Add(syl);
        }

        public void SetLine(Event line, bool autoSplit, bool normalize)
        {
            syllables.Clear();
            Syllable syl = new Syllable
            {
                Start = Time.FromTime(line.Start),
                Duration = 0,
                TagType = "\\k",
                OverrideTags = new Dictionary<int, string>()
            };
            ParseSyllables(line, syl);

            if (normalize)
            {
                Time lineEnd = line.End;
                Time lastEnd = syl.Start + Time.FromMillis(syl.Duration);

                if (lastEnd < lineEnd) syllables.Last().Duration += (lineEnd - lastEnd).TotalMilliseconds;
                else if (lastEnd > lineEnd)
                {
                    foreach (var s in syllables)
                    {
                        if (s.Start > lineEnd)
                        {
                            s.Start = Time.FromTime(lineEnd);
                            s.Duration = 0;
                        }
                        else
                        {
                            s.Duration = Math.Min(s.Duration, (lineEnd - s.Start).TotalMilliseconds);
                        }
                    }
                }
            }
            if (autoSplit && syllables.Count == 1)
            {
                int pos;
                while ((pos = syllables.Last().Text!.IndexOf(' ')) != -1)
                {
                    AddSplit(syllables.Count - 1, pos + 1);
                }
            }
        }

        public void AddSplit(int idx, int pos)
        {
            var preSyl = syllables[idx];
            var newSyl = new Syllable();
            syllables.Insert(idx + 1, newSyl);

            if (pos < preSyl.Text.Length)
            {
                newSyl.Text = preSyl.Text.Substring(pos);
                preSyl.Text = preSyl.Text.Substring(0, pos);
            }

            if (newSyl.Text.Equals(string.Empty)) newSyl.Duration = 0;
            else if (preSyl.Text.Equals(string.Empty))
            {
                newSyl.Duration = preSyl.Duration;
                preSyl.Duration = 0;
            }
            else
            {
                newSyl.Duration = (preSyl.Duration * newSyl.Text.Length / (preSyl.Text.Length + newSyl.Text.Length) + 5) / 10 * 10; // lol wut
                preSyl.Duration -= newSyl.Duration;
            }

            if (preSyl.Duration < 0) return;

            newSyl.Start = preSyl.Start + Time.FromMillis(preSyl.Duration);
            newSyl.TagType = string.Copy(preSyl.TagType);

            int len = preSyl.Text.Length;
            foreach (var it in preSyl.OverrideTags)
            {
                if (it.Key < len) continue;
                else
                {
                    newSyl.OverrideTags[it.Key - len] = it.Value;
                    preSyl.OverrideTags.Remove(it.Key);
                }
            }
        }

        public void RemoveSplit(int idx)
        {
            // Cannot remove the first syl
            if (idx == 0) return;
            var syl = syllables[idx];
            var prev = syllables[idx - 1];

            prev.Duration += syl.Duration;
            foreach (var tag in syl.OverrideTags) prev.OverrideTags[tag.Key + prev.Text.Length] = tag.Value;
            prev.Text += syl.Text;

            syllables.RemoveAt(idx);
        }

        public void SetStartTime(int idx, Time time)
        {
            if (idx == 0) return;

            var syl = syllables[idx];
            var prev = syllables[idx - 1];

            if (time < prev.Start) return;
            if (time > syl.Start + Time.FromMillis(syl.Duration)) return;

            var delta = (time - syl.Start).TotalMilliseconds;
            syl.Start = Time.FromTime(time);
            syl.Duration -= delta;
            prev.Duration += delta;
        }

        public void SetLineTimes(Time start, Time end)
        {
            if (end < start) return;
            int idx = 0;

            // Chop off any portion of syllables starting before the new start
            do
            {
                long delta = (start - syllables[idx].Start).TotalMilliseconds;
                syllables[idx].Start = Time.FromTime(start);
                syllables[idx].Duration = Math.Max(0, syllables[idx].Duration - delta);
            }
            while (++idx < syllables.Count && syllables[idx].Start <  start);

            // Truncate syllables ending after the new end time
            idx = syllables.Count - 1;
            while (syllables[idx].Start > end)
            {
                syllables[idx].Start = Time.FromTime(end);
                syllables[idx].Duration = 0;
                --idx;
            }
            syllables[idx].Duration = (end - syllables[idx].Start).TotalMilliseconds;
        }

        public Karaoke(Event line, bool autoSplit, bool normalize)
        {
            syllables = new List<Syllable>();
            if (line != null) SetLine(line, autoSplit, normalize);
        }
    }

    public class Syllable
    {
        /// <summary>
        /// Time relative to time zero (file-time), not line start
        /// </summary>
        public Time Start;
        /// <summary>
        /// Duration of the line in milliseconds
        /// </summary>
        public long Duration;
        public string TagType;
        public string Text;
        public Dictionary<int, string> OverrideTags;
        public string GetFormattedText(bool includeKTag)
        {
            string result = string.Empty;
            if (includeKTag) result = $"{TagType}{(Duration + 5) / 10}";

            int i = 0;
            if (OverrideTags == null) throw new ArgumentNullException("Syllable overrides not initialized");
            foreach (var pair in OverrideTags)
            {
                result += Text?.Substring(i, pair.Key - i);
                result += pair.Value;
                i = pair.Key;
            }
            result += Text?.Substring(i);
            return result;
        }

        public Syllable()
        {
            Start = new Time();
            TagType = string.Empty;
            Text = string.Empty;
            OverrideTags = new Dictionary<int, string>();
        }
    }
}
