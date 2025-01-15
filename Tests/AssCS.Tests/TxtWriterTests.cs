// SPDX-License-Identifier: MPL-2.0

using AssCS.IO;
using Shouldly;

namespace AssCS.Tests;

public class TxtWriterTests
{
    [Fact]
    public void Write()
    {
        var consumer = new ConsumerInfo("Test Suite", "1.0", "testsuite.com");
        var tw = new TxtWriter(CreateDoc(), consumer);

        var sw = new StringWriter();
        tw.Write(sw);
        var lines = sw.ToString().Split('\n');
        lines.Length.ShouldBe(4 + 1); // Empty line at the end, so +1
        lines[1].ShouldStartWith("Joe: ");
        lines[2].ShouldStartWith("# Joe: ");
    }

    [Fact]
    public void Write_NoComments()
    {
        var consumer = new ConsumerInfo("Test Suite", "1.0", "testsuite.com");
        var tw = new TxtWriter(CreateDoc(), consumer, includeComments: false);

        var sw = new StringWriter();
        tw.Write(sw);
        var lines = sw.ToString().Split('\n');
        lines.Length.ShouldBe(3 + 1);
        lines[1].ShouldStartWith("Joe: ");
        lines[2].ShouldStartWith("Tim: ");
    }

    [Fact]
    public void Write_NoActors()
    {
        var consumer = new ConsumerInfo("Test Suite", "1.0", "testsuite.com");
        var tw = new TxtWriter(CreateDoc(), consumer, includeActors: false);

        var sw = new StringWriter();
        tw.Write(sw);
        var lines = sw.ToString().Split('\n');
        lines.Length.ShouldBe(4 + 1);
        lines[1].ShouldStartWith("Mama");
        lines[2].ShouldStartWith("# Mama");
    }

    [Fact]
    public void Write_NoComments_NoActors()
    {
        var consumer = new ConsumerInfo("Test Suite", "1.0", "testsuite.com");
        var tw = new TxtWriter(CreateDoc(), consumer, includeComments: false, includeActors: false);

        var sw = new StringWriter();
        tw.Write(sw);
        var lines = sw.ToString().Split('\n');
        lines.Length.ShouldBe(3 + 1);
        lines[1].ShouldStartWith("Mama");
        lines[2].ShouldStartWith("Bits SO COOL");
    }

    [Fact]
    public void StripNewlines_Big_NoSpace()
    {
        var input = @"This is the first line.\NThis is the second line.";
        var expected = @"This is the first line. This is the second line.";

        TxtWriter.StripNewlines(input).ShouldBe(expected);
    }

    [Fact]
    public void StripNewlines_Big_LeftSpace()
    {
        var input = @"This is the first line. \NThis is the second line.";
        var expected = @"This is the first line. This is the second line.";

        TxtWriter.StripNewlines(input).ShouldBe(expected);
    }

    [Fact]
    public void StripNewlines_Big_RightSpace()
    {
        var input = @"This is the first line.\N This is the second line.";
        var expected = @"This is the first line. This is the second line.";

        TxtWriter.StripNewlines(input).ShouldBe(expected);
    }

    [Fact]
    public void StripNewlines_Big_ManySpace()
    {
        var input = @"This is the first line.    \N  This is the second line.";
        var expected = @"This is the first line. This is the second line.";

        TxtWriter.StripNewlines(input).ShouldBe(expected);
    }

    [Fact]
    public void StripNewlines_Small_NoSpace()
    {
        var input = @"This is the first line.\nThis is the second line.";
        var expected = @"This is the first line. This is the second line.";

        TxtWriter.StripNewlines(input).ShouldBe(expected);
    }

    [Fact]
    public void StripNewlines_Small_LeftSpace()
    {
        var input = @"This is the first line. \nThis is the second line.";
        var expected = @"This is the first line. This is the second line.";

        TxtWriter.StripNewlines(input).ShouldBe(expected);
    }

    [Fact]
    public void StripNewlines_Small_RightSpace()
    {
        var input = @"This is the first line.\n This is the second line.";
        var expected = @"This is the first line. This is the second line.";

        TxtWriter.StripNewlines(input).ShouldBe(expected);
    }

    [Fact]
    public void StripNewlines_Small_ManySpace()
    {
        var input = @"This is the first line.    \n  This is the second line.";
        var expected = @"This is the first line. This is the second line.";

        TxtWriter.StripNewlines(input).ShouldBe(expected);
    }

    private static Document CreateDoc()
    {
        var doc = new Document(false);
        var e1 = new Event(1) { Actor = "Joe", Text = "Mama" };
        var e2 = new Event(2)
        {
            Actor = "Joe",
            Text = "Mama",
            IsComment = true,
        };
        var e3 = new Event(3) { Actor = "Tim", Text = "Bits\\NSO COOL!" };

        doc.EventManager.AddLast([e1, e2, e3]);

        return doc;
    }
}
