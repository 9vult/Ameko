// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using AssCS.IO;
using Shouldly;
using static AssCS.Tests.Utilities.TestUtils;

namespace AssCS.Tests;

public class AssWriterTests
{
    [Fact]
    public void Write()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.ass");
        var consumer = new ConsumerInfo("Test Suite", "1.0", "testsuite.com");
        var aw = new AssWriter(CreateDoc(), consumer);

        var result = aw.Write(fs, path);

        result.ShouldBeTrue();
        fs.FileExists(path.LocalPath).ShouldBeTrue();

        // Validate the written file
        var recreation = new AssParser().Parse(fs, path);

        recreation.ShouldNotBeNull();
        recreation
            .StyleManager.Get("Test")
            .IsCongruentWith(CreateDoc().StyleManager.Get("Test"))
            .ShouldBeTrue();
        recreation.EventManager.Head.IsCongruentWith(CreateDoc().EventManager.Head).ShouldBeTrue();
        recreation.GarbageManager.GetAll().Count.ShouldBe(1);
    }

    [Fact]
    public void Write_Export()
    {
        var fs = new MockFileSystem();
        var path = MakeTestableUri(fs, "test.ass");
        var consumer = new ConsumerInfo("Test Suite", "1.0", "testsuite.com");
        var aw = new AssWriter(CreateDoc(), consumer);

        var result = aw.Write(fs, path, true);

        result.ShouldBeTrue();
        fs.FileExists(path.LocalPath).ShouldBeTrue();

        // Validate the written file
        var recreation = new AssParser().Parse(fs, path);

        recreation.ShouldNotBeNull();
        recreation
            .StyleManager.Get("Test")
            .IsCongruentWith(CreateDoc().StyleManager.Get("Test"))
            .ShouldBeTrue();
        recreation.EventManager.Head.IsCongruentWith(CreateDoc().EventManager.Head).ShouldBeTrue();
        recreation.GarbageManager.GetAll().Count.ShouldBe(0);
    }

    private static Document CreateDoc()
    {
        var doc = new Document(false);
        var s = new Style(1)
        {
            Name = "Test",
            FontFamily = "Cooper Black",
            FontSize = 14,
            Alignment = 8,
        };
        var e1 = new Event(1) { Actor = "Joe", Text = "Mama" };

        doc.StyleManager.Add(s);
        doc.EventManager.AddFirst(e1);
        doc.GarbageManager.Set("test_garbage", 25m);

        return doc;
    }
}
