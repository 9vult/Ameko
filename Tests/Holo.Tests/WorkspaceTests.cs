// SPDX-License-Identifier: MPL-2.0

using AssCS;
using FluentAssertions;
using Holo;

namespace HoloTests;

public class WorkspaceTests
{
    [Fact]
    public void SetSelection_Single()
    {
        var wsp = new Workspace(new Document(true), 1, null);
        var e = new Event(99);
        wsp.Document.EventManager.AddLast(e);

        wsp.SetSelection(e);

        wsp.SelectedEvent.Should().Be(e);
        wsp.SelectedEventCollection.Should().HaveCount(1);
        wsp.SelectedEventCollection.Should().Contain(e);
    }

    [Fact]
    public void SetSelection_Multiple()
    {
        var wsp = new Workspace(new Document(true), 1, null);
        var e1 = new Event(99);
        var e2 = new Event(100);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Document.EventManager.AddLast(e2);

        wsp.SetSelection(e1, [e1, e2]);

        wsp.SelectedEvent.Should().Be(e1);
        wsp.SelectedEventCollection.Should().HaveCount(2);
        wsp.SelectedEventCollection.Should().Contain(e1);
        wsp.SelectedEventCollection.Should().Contain(e2);
    }
}
