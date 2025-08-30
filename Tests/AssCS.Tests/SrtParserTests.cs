// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using AssCS.IO;
using Shouldly;

namespace AssCS.Tests;

using static Utilities.TestUtils;

public class SrtParserTests
{
    [Fact]
    public void Parse()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.srt");
        fs.AddFile(path.LocalPath, new MockFileData(File1));

        var sp = new SrtParser();

        var doc = sp.Parse(fs, path);

        doc.ShouldNotBeNull();
        doc.StyleManager.Styles.Count.ShouldBe(1);
        doc.EventManager.Events.Count.ShouldBe(5);

        var last = doc.EventManager.Tail;
        last.ShouldNotBeNull();
        last.Start.TotalMilliseconds.ShouldBe(17260);
        last.End.TotalMilliseconds.ShouldBe(18100);
        last.Text.ShouldBe("（風太郎）あっ");

        var three = doc.EventManager.Get(2);
        three.ShouldNotBeNull();
        three.Text.ShouldBe(@"（スタッフ）\N起きてください 新郎様");
    }

    [Fact]
    public void ParseWithTags()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.srt");
        fs.AddFile(path.LocalPath, new MockFileData(File2));

        var sp = new SrtParser();

        var doc = sp.Parse(fs, path);

        doc.ShouldNotBeNull();
        doc.EventManager.Events.Count.ShouldBe(1);
        var last = doc.EventManager.Tail;

        last.Text.ShouldBe(
            @"Hey {\i1}dear {\b1}my{\b0\i0} {\u1}friends{\u0}\N{\fnArial\fs32\c&H4400FF&}So cool{\fn\fs\c}"
        );
    }

    [Fact]
    public void ExpectedSubtitleIndex()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.srt");
        fs.AddFile(path.LocalPath, new MockFileData("Hello"));

        var sp = new SrtParser();

        Action action = () => sp.Parse(fs, path);

        action.ShouldThrow<FormatException>().Message.ShouldBe("Expected subtitle index at line 1");
    }

    [Fact]
    public void ExpectedTimestamps()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.srt");
        fs.AddFile(path.LocalPath, new MockFileData("1\nHello"));

        var sp = new SrtParser();

        Action action = () => sp.Parse(fs, path);

        action.ShouldThrow<FormatException>().Message.ShouldBe("Expected timestamps at line 2");
    }

    [Fact]
    public void UnexpectedEndOfFile()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.srt");
        fs.AddFile(path.LocalPath, new MockFileData("1\n00:00:00,620 --> 00:00:05,630"));

        var sp = new SrtParser();

        Action action = () => sp.Parse(fs, path);

        action.ShouldThrow<FormatException>().Message.ShouldBe("Unexpected end of SRT file");
    }

    private const string File1 = """
        1
        00:00:00,620 --> 00:00:05,630
        （教会の鐘の音）

        2
        00:00:07,130 --> 00:00:09,010
        （風太郎(ふうたろう)）夢を見ていた

        3
        00:00:11,420 --> 00:00:13,680
        （スタッフ）
        起きてください 新郎様

        4
        00:00:13,800 --> 00:00:16,260
        新婦様の準備が整いましたよ

        5
        00:00:17,260 --> 00:00:18,100
        （風太郎）あっ
        """;

    private const string File2 = """
        1
        00:00:00,620 --> 00:00:05,630
        Hey <i>dear <b>my</b></i> <u>friends</u>
        <font face="Arial" size="32" color='#FF0044'>So cool</font>
        """;
}
