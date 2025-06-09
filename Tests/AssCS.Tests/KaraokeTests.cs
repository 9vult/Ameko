// SPDX-License-Identifier: MPL-2.0

using AssCS.Overrides;
using Shouldly;

namespace AssCS.Tests;

public class KaraokeTests
{
    // TODO: Verify that all this is correct

    [Fact]
    public void Text_ReturnsSyllableTextWithKTags()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k10}Hi {\k20}there");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);
        karaoke.Text.ShouldBe(@"{\k10}Hi {\k20}there");
    }

    [Fact]
    public void TagType_ReturnsCorrectTag()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\kf10}One");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);
        karaoke.TagType.ShouldBe(@"\kf");
    }

    [Fact]
    public void TagType_Set_UpdatesAllSyllables()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k10}One {\k10}Two");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        karaoke.TagType = @"\kf";
        karaoke.Text.ShouldBe(@"{\kf10}One {\kf10}Two");
    }

    [Fact]
    public void AddSplit_SplitsSyllable()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k20}HelloWorld");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        karaoke.AddSplit(0, 5); // Evenly split "HelloWorld" to "Hello" + "World"
        karaoke.Text.ShouldBe(@"{\k10}Hello{\k10}World");
    }

    [Fact]
    public void RemoveSplit_JoinsSyllables()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k20}Hello {\k20}World");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        karaoke.RemoveSplit(1);
        karaoke.Text.ShouldBe(@"{\k40}Hello World");
    }

    [Fact]
    public void SetStartTime_AdjustsStartProperly()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k10}Hi {\k10}there");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        var newTime = evt.Start + Time.FromCentis(5);
        karaoke.SetStartTime(1, newTime);
        karaoke.Text.ShouldBe(@"{\k5}Hi {\k15}there");
    }

    [Fact]
    public void SetLineTimes_TruncatesCorrectly()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k10}Hi {\k10}there");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        var start = evt.Start + Time.FromMillis(5);
        var end = evt.End - Time.FromMillis(5);
        karaoke.SetLineTimes(start, end);

        karaoke.Text.ShouldBe(@"{\k10}Hi {\k490}there");
    }

    private static Event CreateEventWithSyllables(string content)
    {
        return new Event(1) { Text = content };
    }
}
