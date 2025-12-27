// SPDX-License-Identifier: MPL-2.0

using AssCS.Overrides;

namespace AssCS.Tests;

public class KaraokeTests
{
    // TODO: Verify that all this is correct    [Test]
    public async Task Text_ReturnsSyllableTextWithKTags()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k10}Hi {\k20}there");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);
        await Assert.That(karaoke.Text).IsEqualTo(@"{\k10}Hi {\k20}there");
    }

    [Test]
    public async Task TagType_ReturnsCorrectTag()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\kf10}One");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);
        await Assert.That(karaoke.TagType).IsEqualTo(@"kf");
    }

    [Test]
    public async Task TagType_Set_UpdatesAllSyllables()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k10}One {\k10}Two");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        karaoke.TagType = @"kf";
        await Assert.That(karaoke.Text).IsEqualTo(@"{\kf10}One {\kf10}Two");
    }

    [Test]
    public async Task AddSplit_SplitsSyllable()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k20}HelloWorld");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        karaoke.AddSplit(0, 5); // Evenly split "HelloWorld" to "Hello" + "World"
        await Assert.That(karaoke.Text).IsEqualTo(@"{\k10}Hello{\k10}World");
    }

    [Test]
    public async Task RemoveSplit_JoinsSyllables()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k20}Hello {\k20}World");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        karaoke.RemoveSplit(1);
        await Assert.That(karaoke.Text).IsEqualTo(@"{\k40}Hello World");
    }

    [Test]
    public async Task SetStartTime_AdjustsStartProperly()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k10}Hi {\k10}there");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        var newTime = evt.Start + Time.FromCentis(5);
        karaoke.SetStartTime(1, newTime);
        await Assert.That(karaoke.Text).IsEqualTo(@"{\k5}Hi {\k15}there");
    }

    [Test]
    public async Task SetLineTimes_TruncatesCorrectly()
    {
        var karaoke = new Karaoke();
        var evt = CreateEventWithSyllables(@"{\k10}Hi {\k10}there");
        karaoke.SetLine(evt, autoSplit: false, normalize: false);

        var start = evt.Start + Time.FromMillis(5);
        var end = evt.End - Time.FromMillis(5);
        karaoke.SetLineTimes(start, end);

        await Assert.That(karaoke.Text).IsEqualTo(@"{\k10}Hi {\k490}there");
    }

    private static Event CreateEventWithSyllables(string content)
    {
        return new Event(1) { Text = content };
    }
}
