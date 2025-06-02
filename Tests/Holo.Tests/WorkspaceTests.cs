// SPDX-License-Identifier: MPL-2.0

using AssCS;
using AssCS.History;
using Shouldly;

namespace Holo.Tests;

public class WorkspaceTests
{
    [Fact]
    public void Commit_Single()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e = new Event(99);
        wsp.Document.EventManager.AddLast(e);

        wsp.Commit(e, CommitType.EventAdd);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        wsp.Document.HistoryManager.LastCommitType.ShouldBe(CommitType.EventAdd);
    }

    [Fact]
    public void Commit_Multiple()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Document.EventManager.AddLast(e2);

        wsp.Commit([e1, e2], CommitType.EventAdd);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        wsp.Document.HistoryManager.LastCommitType.ShouldBe(CommitType.EventAdd);

        var commit = wsp.Document.HistoryManager.Undo() as EventCommit;
        commit.ShouldNotBeNull();
        commit.Targets.Count.ShouldBe(2);
    }

    [Fact]
    public void Commit_Amends_When_Same_Type()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, CommitType.EventAdd);

        var e2 = new Event(100);
        wsp.Document.EventManager.AddLast(e2);
        wsp.Commit(e2, CommitType.EventAdd);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        var commit = wsp.Document.HistoryManager.Undo() as EventCommit;
        commit.ShouldNotBeNull();
        commit.Targets.Count.ShouldBe(2);
    }

    [Fact]
    public void Commit_Does_Not_Amend_When_Different_Type()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, CommitType.EventAdd);

        e1.Text = "test";
        wsp.Commit(wsp.Document.EventManager.Tail, CommitType.EventText);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        var commit = wsp.Document.HistoryManager.Undo() as EventCommit;
        commit.ShouldNotBeNull();
        commit.Targets.Count.ShouldBe(1);
        commit.Targets.First().Target.ShouldBeEquivalentTo(e1);
    }

    [Fact]
    public void Select_Single()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e = new Event(99);
        wsp.Document.EventManager.AddLast(e);

        wsp.SelectionManager.Select(e);

        wsp.SelectionManager.ActiveEvent.ShouldBe(e);
        wsp.SelectionManager.SelectedEventCollection.Count.ShouldBe(1);
    }

    [Fact]
    public void Select_Multiple()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Document.EventManager.AddLast(e2);

        wsp.SelectionManager.Select(e2, [e1, e2]);

        wsp.SelectionManager.ActiveEvent.ShouldBe(e2);
        wsp.SelectionManager.SelectedEventCollection.Count.ShouldBe(2);
    }
}
