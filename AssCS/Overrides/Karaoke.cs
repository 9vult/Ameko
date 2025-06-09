﻿// SPDX-License-Identifier: MPL-2.0

using AssCS.Overrides.Blocks;

namespace AssCS.Overrides;

/// <summary>
/// A verse of karaoke
/// </summary>
public class Karaoke
{
    private readonly List<Syllable> _syllables = [];

    /// <summary>
    /// Text content of the verse
    /// </summary>
    public string Text => string.Join("", _syllables.Select(s => s.GetFormattedText(true)));

    /// <summary>
    /// Karaoke tag type
    /// </summary>
    /// <remarks>Returns an empty string if there are no syllables</remarks>
    public string TagType
    {
        get => _syllables.FirstOrDefault()?.TagType ?? string.Empty;
        set
        {
            foreach (var syl in _syllables)
                syl.TagType = value;
        }
    }

    /// <summary>
    /// Set the syllables for a line
    /// </summary>
    /// <param name="line">Line to set the syllables of</param>
    /// <param name="autoSplit">Whether to split on spaces</param>
    /// <param name="normalize">Whether to normalize syllable durations</param>
    public void SetLine(Event line, bool autoSplit, bool normalize)
    {
        _syllables.Clear();
        Syllable syl = new()
        {
            Start = Time.FromTime(line.Start),
            Duration = 0,
            TagType = @"\k",
        };
        ParseSyllables(line, syl);

        if (normalize)
        {
            var lineEnd = line.End;
            var lastEnd = syl.Start + Time.FromMillis(syl.Duration);

            if (lastEnd > lineEnd)
            {
                foreach (var s in _syllables)
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

        if (autoSplit && _syllables.Count == 1)
        {
            int pos;
            while ((pos = _syllables.Last().Text.IndexOf(' ')) != 1)
            {
                AddSplit(_syllables.Count - 1, pos + 1);
            }
        }
    }

    /// <summary>
    /// Add a split
    /// </summary>
    /// <param name="index">Syllable index</param>
    /// <param name="position">Position in the text</param>
    public void AddSplit(int index, int position)
    {
        var preSyl = _syllables[index];
        var newSyl = new Syllable();
        _syllables.Insert(index + 1, newSyl);

        if (position < preSyl.Text.Length)
        {
            newSyl.Text = preSyl.Text[position..];
            preSyl.Text = preSyl.Text[..position];
        }

        if (newSyl.Text == string.Empty)
            newSyl.Duration = 0;
        else if (preSyl.Text == string.Empty)
        {
            newSyl.Duration = preSyl.Duration;
            preSyl.Duration = 0;
        }
        else
        {
            newSyl.Duration =
                (
                    preSyl.Duration * newSyl.Text.Length / (preSyl.Text.Length + newSyl.Text.Length)
                    + 5
                )
                / 10
                * 10; // lol
            preSyl.Duration -= newSyl.Duration;
        }

        if (preSyl.Duration < 0)
            return;

        newSyl.Start = preSyl.Start + Time.FromMillis(preSyl.Duration);
        newSyl.TagType = new string(preSyl.TagType);

        int len = preSyl.Text.Length;
        foreach (var pair in preSyl.OverrideTags)
        {
            if (pair.Key < len)
                continue;

            newSyl.OverrideTags[pair.Key - len] = pair.Value;
            preSyl.OverrideTags.Remove(pair.Key);
        }
    }

    /// <summary>
    /// Remove a split
    /// </summary>
    /// <param name="index">Index to remove the split at</param>
    /// <remarks>First syllable can not be removed</remarks>
    public void RemoveSplit(int index)
    {
        if (index == 0)
            return;

        var syl = _syllables[index];
        var pre = _syllables[index - 1];

        pre.Duration += syl.Duration;
        foreach (var tag in syl.OverrideTags)
            pre.OverrideTags[tag.Key + pre.Text.Length] = tag.Value;

        pre.Text += syl.Text;

        _syllables.RemoveAt(index);
    }

    /// <summary>
    /// Set the start time of a syllable
    /// </summary>
    /// <param name="index">Index to set the time of</param>
    /// <param name="time">Time to set</param>
    /// <remarks>First syllable cannot be set</remarks>
    public void SetStartTime(int index, Time time)
    {
        if (index == 0)
            return;

        var syl = _syllables[index];
        var pre = _syllables[index - 1];

        if (time < pre.Start)
            return;
        if (time > syl.Start + Time.FromMillis(syl.Duration))
            return;

        var delta = time.TotalMilliseconds - syl.Start.TotalMilliseconds;
        syl.Start = Time.FromTime(time);
        syl.Duration -= delta;
        pre.Duration += delta;
    }

    /// <summary>
    /// Contain syllables within line start and end times
    /// </summary>
    /// <param name="start">New line start time</param>
    /// <param name="end">New line end time</param>
    /// <remarks>Syllables outside the new times will be truncated</remarks>
    public void SetLineTimes(Time start, Time end)
    {
        if (end < start)
            return;
        int index = 0;

        // Chop off any portion of syllables starting before the new start time
        do
        {
            var delta = start.TotalMilliseconds - _syllables[index].Start.TotalMilliseconds;
            _syllables[index].Start = Time.FromTime(start);
            _syllables[index].Duration = Math.Max(0, _syllables[index].Duration - delta);
        } while (++index < _syllables.Count && _syllables[index].Start < start);

        // Truncate syllables ending after the new end time
        index = _syllables.Count - 1;

        while (_syllables[index].Start > end)
        {
            _syllables[index].Start = Time.FromTime(end);
            _syllables[index].Duration = 0;
            --index;
        }

        _syllables[index].Duration =
            end.TotalMilliseconds - _syllables[index].Start.TotalMilliseconds;
    }

    /// <summary>
    /// Parse the syllables in an event
    /// </summary>
    /// <param name="line">Event to parse</param>
    /// <param name="syl">Syllable</param>
    private void ParseSyllables(Event line, Syllable syl)
    {
        foreach (var block in line.ParseTags())
        {
            var text = block.Text;
            switch (block.Type)
            {
                case BlockType.Plain:
                    syl.Text += text;
                    break;
                case BlockType.Comment:
                case BlockType.Drawing:
                    syl.OverrideTags[syl.Text.Length] += text;
                    break;
                case BlockType.Override:
                    var b = (OverrideBlock)block;
                    var inTag = false;

                    foreach (var tag in b.Tags)
                    {
                        if (
                            tag.IsValid
                            && tag.Name.StartsWith(
                                @"\k",
                                StringComparison.InvariantCultureIgnoreCase
                            )
                        )
                        {
                            if (inTag)
                            {
                                syl.OverrideTags[syl.Text.Length] += '}';
                                inTag = false;
                            }
                            // Convert \K to \kf for convenience
                            if (tag.Name == @"\K")
                                tag.Name = @"\kf";

                            // Exclude zero syllables
                            if (syl.Duration > 0 || syl.Text.Length != 0)
                            {
                                _syllables.Add(syl);
                                syl = new Syllable
                                {
                                    Start = syl.Start,
                                    Duration = syl.Duration,
                                    TagType = syl.TagType,
                                    Text = string.Empty,
                                };
                            }

                            syl.TagType = tag.Name;
                            syl.Start += Time.FromMillis(syl.Duration);
                            syl.Duration = tag.Parameters[0].GetInt() * 10;
                        }
                        else
                        {
                            // Get or create the override tag string at the index of syl.Text.Length
                            if (!syl.OverrideTags.TryGetValue(syl.Text.Length, out var oText))
                                syl.OverrideTags[syl.Text.Length] = oText = string.Empty;

                            text = text.TrimEnd('}');

                            if (!inTag)
                                oText += "{";

                            inTag = true;
                            oText += tag;

                            // Save the updated string back
                            syl.OverrideTags[syl.Text.Length] = oText;
                        }
                    }
                    if (inTag)
                        syl.OverrideTags[syl.Text.Length] += '}';
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(syl));
            }
        }
        _syllables.Add(syl);
    }
}
