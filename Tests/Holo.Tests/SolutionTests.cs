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
        sln.StyleManager.Add(new AssCS.Style(sln.StyleManager.NextId));
        sln.Cps = 21;
        sln.UseSoftLinebreaks = false;

        var sw = new StringWriter();
        var isSaved = sln.Save(sw, new Uri(@"file:///c/"));
        var result = sw.ToString();

        isSaved.Should().BeTrue();
    }
}
