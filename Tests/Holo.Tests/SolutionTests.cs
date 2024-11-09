// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;
using Holo;

namespace HoloTests;

public class SolutionTests
{
    [Fact]
    public void Save()
    {
        var sln = new Solution();
        var style = new AssCS.Style(sln.StyleManager.NextId);
        sln.StyleManager.Add(style);
        sln.Cps = 21;

        var sw = new StringWriter();
        var isSaved = sln.Save(sw, new Uri(@"file:///c/"));
        var result = sw.ToString();

        // Can't easily verify contents because of safe encoding
        isSaved.Should().BeTrue();
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse()
    {
        const string input =
            "{\"Version\":1,\"ReferencedDocuments\":[],\"Styles\":[],\"Cps\":21,\"UseSoftLinebreaks\":null}";

        var sr = new StringReader(input);
        var sln = Solution.Parse(sr, new Uri("file:///c/"));

        sln.Cps.Should().Be(21);
        sln.UseSoftLinebreaks.Should().BeNull();
        sln.ReferencedDocuments.Count.Should().Be(1);
        sln.StyleManager.Styles.Count.Should().Be(0);
    }
}
