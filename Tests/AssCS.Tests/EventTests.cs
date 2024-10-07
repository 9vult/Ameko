// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;

namespace AssCS.Tests;

public class EventTests
{
    private const string BasicEvent =
        "Dialogue: 0,0:02:10.57,0:02:13.51,Default,Heiter,0,0,0,,It's the victorious return of the heroes' party.";
    private const string TagEvent =
        "Dialogue: 0,0:11:34.65,0:11:37.38,Default,Frieren,0,0,0,,You're {\\i1}bald{\\i0}, there's no point in fussing.";

    [Fact]
    public void FromAss()
    {
        Event e = Event.FromAss(1, BasicEvent);

        e.IsComment.Should().BeFalse();
        e.Layer.Should().Be(0);
        e.Start.Should().Be(Time.FromMillis(130570)); // 2:10.57
        e.End.Should().Be(Time.FromMillis(133510)); // 2:13.51
        e.Style.Should().Be("Default");
        e.Actor.Should().Be("Heiter");
        e.Margins.Should().Be(new Margins(0, 0, 0));
        e.Effect.Should().Be(string.Empty);
        e.Text.Should().Be("It's the victorious return of the heroes' party.");
    }

    [Fact]
    public void FromAss_Malformed()
    {
        Action action = () => Event.FromAss(1, BasicEvent.Replace(',', 'Q'));

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AsAss()
    {
        Event e = Event.FromAss(1, BasicEvent);

        e.AsAss().Should().Be(BasicEvent);
    }

    [Fact]
    public void StripText()
    {
        Event e = Event.FromAss(1, TagEvent);
        e.StripTags();

        e.Text.Should().Be("You're bald, there's no point in fussing.");
    }

    [Fact]
    public void Clone()
    {
        Event e1 = Event.FromAss(1, BasicEvent);
        Event e2 = e1.Clone();

        e2.Should().Be(e1);
    }

    [Fact]
    public void CollidesWith()
    {
        Event e1 = new Event(1) { Start = Time.FromSeconds(0), End = Time.FromSeconds(5) };
        Event e2 = new Event(2) { Start = Time.FromSeconds(2.5), End = Time.FromSeconds(7.5) };
        Event e3 = new Event(3) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };

        e1.CollidesWith(e2).Should().BeTrue();
        e2.CollidesWith(e1).Should().BeTrue();

        e1.CollidesWith(e3).Should().BeFalse();
        e2.CollidesWith(e3).Should().BeFalse();
    }

    #region Cps & Line Width

    [Fact]
    public void Cps_NoTags()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "The quick brown fox jumps over the lazy dog",
        };

        e.Cps.Should().Be(35);
    }

    [Fact]
    public void Cps_Newline()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "The quick brown fox\\Njumps over the lazy dog",
        };

        e.Cps.Should().Be(35);
    }

    [Fact]
    public void Cps_Tags()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text =
                "The quick {\\i1}brown{\\i0} fox{it could be an artic fox} jumps over the lazy dog",
        };

        e.Cps.Should().Be(35);
    }

    [Fact]
    public void MaxLineWidth_SingleLine()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "The quick brown fox jumps over the lazy dog",
        };

        e.MaxLineWidth.Should().Be(35);
    }

    [Fact]
    public void MaxLineWidth_SingleLine_Tags()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text =
                "The quick {\\i1}brown{\\i0} fox{it could be an artic fox} jumps over the lazy dog",
        };

        e.MaxLineWidth.Should().Be(35);
    }

    [Fact]
    public void MaxLineWidth_MultipleLines()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "The quick brown fox\\Njumps over the lazy dog",
        };

        e.MaxLineWidth.Should().Be(19);
    }

    [Fact]
    public void MaxLineWidth_MultipeLines_Tags()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text =
                "The quick {\\i1}brown{\\i0} fox{it could be an artic fox}\\Njumps over the lazy dog",
        };

        e.MaxLineWidth.Should().Be(19);
    }

    #endregion Cps & Line Width

    #region GetStrippedText

    [Fact]
    public void GetStrippedText_Normal()
    {
        Event e = Event.FromAss(1, TagEvent);

        e.GetStrippedText().Should().Be("You're bald, there's no point in fussing.");
    }

    [Fact]
    public void GetStrippedText_Empty()
    {
        Event e = new Event(1);

        e.GetStrippedText().Should().Be(string.Empty);
    }

    [Fact]
    public void GetStrippedText_OnlyTag()
    {
        Event e = new Event(1) { Text = "{\\q2}" };

        e.GetStrippedText().Should().Be(string.Empty);
    }

    [Fact]
    public void GetStrippedText_Short_After()
    {
        Event e = new Event(1) { Text = "{\\q2}A" };

        e.GetStrippedText().Should().Be("A");
    }

    [Fact]
    public void GetStrippedText_Short_Before()
    {
        Event e = new Event(1) { Text = "A{\\q2}" };

        e.GetStrippedText().Should().Be("A");
    }

    #endregion GetStrippedText

    #region ToggleTag

    [Fact]
    public void ToggleTag_ToggleTagPair()
    {
        Event e = Event.FromAss(1, TagEvent);
        Style s = new Style(1); // Default style

        e.ToggleTag("\\i", s, 12, 16); // Inside the tag

        e.Text.Should().Be("You're {\\i0}bald{\\i1}, there's no point in fussing.");
    }

    [Fact]
    public void ToggleTag_AddToExistingPair()
    {
        Event e = Event.FromAss(1, TagEvent);
        Style s = new Style(1); // Default style

        e.ToggleTag("\\b", s, 12, 16); // Inside the tag

        e.Text.Should().Be("You're {\\i1\\b1}bald{\\i0\\b0}, there's no point in fussing.");
    }

    [Fact]
    public void ToggleTag_AddTag()
    {
        Event e = Event.FromAss(1, BasicEvent);
        Style s = new Style(1); // Default style

        e.ToggleTag("\\i", s, 0, 0);

        e.Text.Should().Be("{\\i1}It's the victorious return of the heroes' party.");
    }

    #endregion ToggleTag
}
