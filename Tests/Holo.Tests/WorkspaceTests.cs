// SPDX-License-Identifier: MPL-2.0

using AssCS;
using AssCS.History;
using Shouldly;

namespace Holo.Tests;

public class WorkspaceTests
{
    [Fact]
    public void SetSelection_Single()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e = new Event(99);
        wsp.Document.EventManager.AddLast(e);

        wsp.SetSelection(e, CommitType.EventAdd);

        wsp.SelectedEvent.ShouldBe(e);
        wsp.SelectedEventCollection.Count.ShouldBe(1);
        wsp.SelectedEventCollection.ShouldContain(e);
    }

    [Fact]
    public void SetSelection_Multiple()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Document.EventManager.AddLast(e2);

        wsp.SetSelection(e1, [e1, e2], CommitType.EventAdd);

        wsp.SelectedEvent.ShouldBe(e1);
        wsp.SelectedEventCollection.Count.ShouldBe(2);
        wsp.SelectedEventCollection.ShouldContain(e1);
        wsp.SelectedEventCollection.ShouldContain(e2);
    }
}
