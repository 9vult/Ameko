// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;

namespace AssCS.Tests;

public class EventTests
{
    private const string BasicEvent =
        "Dialogue: 0,0:02:10.57,0:02:13.51,Default,Heiter,0,0,0,,It's the victorious return of the heroes' party.";
    private const string MultilineEvent =
        "Dialogue: 0,0:08:52.91,0:08:57.00,Default,Frieren,0,0,0,,I see. That's a problem;\\NI need it for a summoning.";
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
    public void AsAss()
    {
        Event e = Event.FromAss(1, BasicEvent);

        e.AsAss().Should().Be(BasicEvent);
    }

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
