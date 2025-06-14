// SPDX-License-Identifier: MPL-2.0

using Shouldly;

namespace AssCS.Tests;

public class EventTests
{
    private const string BasicEvent =
        "Dialogue: 0,0:02:10.57,0:02:13.51,Default,Heiter,0,0,0,,It's the victorious return of the heroes' party.";
    private const string TagEvent =
        @"Dialogue: 0,0:11:34.65,0:11:37.38,Default,Frieren,0,0,0,,You're {\i1}bald{\i0}, there's no point in fussing.";

    [Fact]
    public void FromAss()
    {
        Event e = Event.FromAss(1, BasicEvent);

        e.IsComment.ShouldBeFalse();
        e.Layer.ShouldBe(0);
        e.Start.ShouldBe(Time.FromMillis(130570)); // 2:10.57
        e.End.ShouldBe(Time.FromMillis(133510)); // 2:13.51
        e.Style.ShouldBe("Default");
        e.Actor.ShouldBe("Heiter");
        e.Margins.ShouldBe(new Margins(0, 0, 0));
        e.Effect.ShouldBe(string.Empty);
        e.Text.ShouldBe("It's the victorious return of the heroes' party.");
    }

    [Fact]
    public void FromAss_Malformed()
    {
        Action action = () => Event.FromAss(1, BasicEvent.Replace(',', 'Q'));

        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void AsAss()
    {
        Event e = Event.FromAss(1, BasicEvent);

        e.AsAss().ShouldBe(BasicEvent);
    }

    [Fact]
    public void StripText()
    {
        Event e = Event.FromAss(1, TagEvent);
        e.StripTags();

        e.Text.ShouldBe("You're bald, there's no point in fussing.");
    }

    [Fact]
    public void Clone()
    {
        Event e1 = Event.FromAss(1, BasicEvent);
        Event e2 = e1.Clone();

        e2.ShouldBe(e1);
    }

    [Fact]
    public void CollidesWith()
    {
        Event e1 = new Event(1) { Start = Time.FromSeconds(0), End = Time.FromSeconds(5) };
        Event e2 = new Event(2) { Start = Time.FromSeconds(2.5), End = Time.FromSeconds(7.5) };
        Event e3 = new Event(3) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };

        e1.CollidesWith(e2).ShouldBeTrue();
        e2.CollidesWith(e1).ShouldBeTrue();

        e1.CollidesWith(e3).ShouldBeFalse();
        e2.CollidesWith(e3).ShouldBeFalse();
    }

    [Fact]
    public void TransformCodeToAss()
    {
        var evt = new Event(1) { Text = "Line1\n  Line2" };
        evt.TransformCodeToAss().ShouldContain("--[[2]]");
    }

    [Fact]
    public void TransformAssToCode()
    {
        var evt = new Event(1) { Text = "Line1--[[2]]Line2" };
        evt.TransformAssToCode().ShouldContain(Environment.NewLine + "  Line2");
    }

    [Fact]
    public void SetFields_NoFlags()
    {
        var baseline = Event.FromAss(1, BasicEvent);
        var evt1 = Event.FromAss(1, BasicEvent);
        var evt2 = Event.FromAss(2, TagEvent);
        const EventField fields = EventField.None;

        evt1.SetFields(fields, evt2);

        evt1.IsCongruentWith(baseline).ShouldBeTrue();
    }

    [Fact]
    public void SetFields_AllFlags()
    {
        var baseline = Event.FromAss(1, BasicEvent);
        var evt1 = Event.FromAss(1, BasicEvent);
        var evt2 = Event.FromAss(2, TagEvent);
        const EventField fields =
            EventField.Comment
            | EventField.Layer
            | EventField.StartTime
            | EventField.EndTime
            | EventField.Style
            | EventField.Actor
            | EventField.MarginLeft
            | EventField.MarginRight
            | EventField.MarginVertical
            | EventField.Effect
            | EventField.Text;

        evt1.SetFields(fields, evt2);

        evt1.IsCongruentWith(baseline).ShouldBeFalse();
        evt1.IsCongruentWith(evt2).ShouldBeTrue();
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

        e.Cps.ShouldBe(35);
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

        e.Cps.ShouldBe(35);
    }

    [Fact]
    public void Cps_Random_Backslash()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "The quick brown fox\\Njumps \\over the lazy dog",
        };

        e.Cps.ShouldBe(35);
    }

    [Fact]
    public void Cps_Floating_Newline()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "The quick brown fox \\N jumps over the lazy dog",
        };

        e.Cps.ShouldBe(35);
    }

    [Fact]
    public void Cps_Hard_Space()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "The quick brown fox\\hjumps over the lazy dog",
        };

        e.Cps.ShouldBe(35);
    }

    [Fact]
    public void Cps_Terminating_Newline()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "The quick brown fox jumps over the lazy dog\\N",
        };

        e.Cps.ShouldBe(35);
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

        e.Cps.ShouldBe(35);
    }

    [Fact]
    public void Cps_Kanji()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "この番組は、ご覧のスポンサーの提供でお送りします。",
        };

        e.Cps.ShouldBe(23);
    }

    [Fact]
    public void Cps_Decomposed_Hangul()
    {
        Event e1 = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "\u1100\u1161\u11a8", // 각
        };

        Event e2 = new Event(2)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "\uac01", // 각
        };

        e1.Cps.ShouldBe(e2.Cps);
    }

    [Fact]
    public void Cps_Ffi_Ligature()
    {
        Event e1 = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "ﬃ",
        };

        Event e2 = new Event(2)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "ffi",
        };

        e1.Cps.ShouldNotBe(e2.Cps);
    }

    [Fact]
    public void Cps_Multilingual()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text =
                "He said, \"Hello world!\"\\N彼は「おはよう世界！」と言いました。\\nОн сказал: «Здравствуй, мир!»",
        };

        e.Cps.ShouldBe(51);
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

        e.MaxLineWidth.ShouldBe(35);
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

        e.MaxLineWidth.ShouldBe(35);
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

        e.MaxLineWidth.ShouldBe(19);
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

        e.MaxLineWidth.ShouldBe(19);
    }

    [Fact]
    public void MaxLineWidth_Kanji()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "この番組は、\\nご覧のスポンサーの提供でお送りします。",
        };

        e.MaxLineWidth.ShouldBe(19);
    }

    [Fact]
    public void MaxLineWidth_Decomposed_Hangul()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "\u1100\u1161\u11a8\\N\uac01", // 각 \n 각
        };

        e.MaxLineWidth.ShouldBe(1);
    }

    [Fact]
    public void MaxLineWidth_Ffi_Ligature()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text = "ﬃ\\Nffi",
        };

        e.MaxLineWidth.ShouldBe(3);
    }

    [Fact]
    public void MaxLineWidth_Multilingual()
    {
        Event e = new Event(1)
        {
            Start = Time.FromSeconds(0),
            End = Time.FromSeconds(1),
            Text =
                "He said, \"Hello world!\"\\N彼は「おはよう世界！」と言いました。\\nОн сказал: «Здравствуй, мир!»",
        };

        e.MaxLineWidth.ShouldBe(26);
    }

    #endregion Cps & Line Width

    #region GetStrippedText

    [Fact]
    public void GetStrippedText_Normal()
    {
        Event e = Event.FromAss(1, TagEvent);

        e.GetStrippedText().ShouldBe("You're bald, there's no point in fussing.");
    }

    [Fact]
    public void GetStrippedText_Empty()
    {
        Event e = new Event(1);

        e.GetStrippedText().ShouldBe(string.Empty);
    }

    [Fact]
    public void GetStrippedText_OnlyTag()
    {
        Event e = new Event(1) { Text = "{\\q2}" };

        e.GetStrippedText().ShouldBe(string.Empty);
    }

    [Fact]
    public void GetStrippedText_Short_After()
    {
        Event e = new Event(1) { Text = "{\\q2}A" };

        e.GetStrippedText().ShouldBe("A");
    }

    [Fact]
    public void GetStrippedText_Short_Before()
    {
        Event e = new Event(1) { Text = "A{\\q2}" };

        e.GetStrippedText().ShouldBe("A");
    }

    #endregion GetStrippedText

    #region ToggleTag

    [Fact]
    public void ToggleTag_ToggleTagPair()
    {
        Event e = Event.FromAss(1, TagEvent);
        Style s = new Style(1); // Default style

        e.ToggleTag("\\i", s, 12, 16); // Inside the tag

        e.Text.ShouldBe("You're {\\i0}bald{\\i1}, there's no point in fussing.");
    }

    [Fact]
    public void ToggleTag_AddToExistingPair()
    {
        Event e = Event.FromAss(1, TagEvent);
        Style s = new Style(1); // Default style

        e.ToggleTag("\\b", s, 12, 16); // Inside the tag

        e.Text.ShouldBe("You're {\\i1\\b1}bald{\\i0\\b0}, there's no point in fussing.");
    }

    [Fact]
    public void ToggleTag_AddTag()
    {
        Event e = Event.FromAss(1, BasicEvent);
        Style s = new Style(1); // Default style

        e.ToggleTag("\\i", s, 0, 0);

        e.Text.ShouldBe("{\\i1}It's the victorious return of the heroes' party.");
    }

    #endregion ToggleTag
}
