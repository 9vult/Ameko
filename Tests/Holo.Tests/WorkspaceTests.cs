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

        wsp.Commit(e, ChangeType.Add);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        wsp.Document.HistoryManager.LastCommitType.ShouldBe(ChangeType.Add);
    }

    [Fact]
    public void Commit_Multiple()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Document.EventManager.AddLast(e2);

        wsp.Commit([e1, e2], ChangeType.Add);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        wsp.Document.HistoryManager.LastCommitType.ShouldBe(ChangeType.Add);

        var commit = wsp.Document.HistoryManager.Undo() as EventCommit;
        commit.ShouldNotBeNull();
        commit.Deltas.Count.ShouldBe(2);
    }

    [Fact]
    public void Commit_Amends_When_Same_Type()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.Add);

        wsp.Document.HistoryManager.BeginTransaction([e1]);
        e1.Text = "Hello";
        wsp.Commit(e1, ChangeType.Modify);

        e1.Text = "Hello World";
        wsp.Commit(e1, ChangeType.Modify);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        var commit = wsp.Document.HistoryManager.Undo() as EventCommit;
        commit.ShouldNotBeNull();
        commit.Deltas.Count.ShouldBe(2);
    }

    [Fact]
    public void Commit_Does_Not_Amend_When_Different_Type()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.Add);

        e1.Text = "test";
        wsp.Commit(wsp.Document.EventManager.Tail, ChangeType.Modify);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        var commit = wsp.Document.HistoryManager.Undo() as EventCommit;
        commit.ShouldNotBeNull();
        commit.Deltas.Count.ShouldBe(1);
        commit.Deltas.First().NewEvent.ShouldBeEquivalentTo(e1);
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

    [Fact]
    public void Undo_AddEvent()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);

        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.Add);
        wsp.Document.EventManager.AddLast(e2);
        wsp.Commit(e2, ChangeType.Add);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        wsp.Undo();
        wsp.Document.EventManager.Events.ShouldNotContain(e2);
        wsp.Document.EventManager.Events.ShouldContain(e1);

        wsp.Document.HistoryManager.CanRedo.ShouldBeTrue();
        wsp.Undo();
        wsp.Document.EventManager.Events.ShouldNotContain(e2);
        wsp.Document.EventManager.Events.ShouldNotContain(e1);
    }

    [Fact]
    public void Redo_AddEvent()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);

        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.Add);
        wsp.Document.EventManager.AddLast(e2);
        wsp.Commit(e2, ChangeType.Add);
        wsp.Undo();
        wsp.Undo();

        wsp.Document.EventManager.Events.ShouldNotContain(e2);
        wsp.Document.EventManager.Events.ShouldNotContain(e1);
        wsp.Document.HistoryManager.CanUndo.ShouldBeFalse();
        wsp.Document.HistoryManager.CanRedo.ShouldBeTrue();

        wsp.Redo();
        wsp.Document.EventManager.Events.ShouldContain(e1);
        wsp.Document.EventManager.Events.ShouldNotContain(e2);

        wsp.Redo();
        wsp.Document.EventManager.Events.ShouldContain(e1);
        wsp.Document.EventManager.Events.ShouldContain(e2);

        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();
        wsp.Document.HistoryManager.CanRedo.ShouldBeFalse();
    }

    [Fact]
    public void Undo_RemoveEvent()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);

        wsp.Document.EventManager.Remove(e1.Id);
        wsp.Commit(e1, ChangeType.Remove);

        wsp.Document.EventManager.Events.ShouldNotContain(e1);
        wsp.Document.HistoryManager.CanUndo.ShouldBeTrue();

        wsp.Undo();
        wsp.Document.EventManager.Events.ShouldContain(e1);
    }

    [Fact]
    public void Redo_RemoveEvent()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);

        wsp.Document.EventManager.Remove(e1.Id);
        wsp.Commit(e1, ChangeType.Remove);
        wsp.Undo();
        wsp.Document.EventManager.Events.ShouldContain(e1);
        wsp.Document.HistoryManager.CanRedo.ShouldBeTrue();

        wsp.Redo();
        wsp.Document.EventManager.Events.ShouldNotContain(e1);
    }

    [Fact]
    public void Undo_ModifyEvent_Once()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = wsp.Document.EventManager.Head;

        wsp.Document.HistoryManager.BeginTransaction(e1);
        e1.Text = "Hello!";
        wsp.Commit(e1, ChangeType.Modify);

        wsp.Undo();
        wsp.Document.EventManager.Head.Text.ShouldBe(string.Empty);
    }

    [Fact]
    public void Redo_ModifyEvent_Once()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = wsp.Document.EventManager.Head;

        wsp.Document.HistoryManager.BeginTransaction(e1);
        e1.Text = "Hello!";
        wsp.Commit(e1, ChangeType.Modify);
        wsp.Undo();

        wsp.Document.HistoryManager.CanRedo.ShouldBeTrue();
        wsp.Redo();

        wsp.Document.EventManager.Head.Text.ShouldBe("Hello!");
    }

    [Fact]
    public void Undo_ModifyEvent_Multiple()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = wsp.Document.EventManager.Head;

        wsp.Document.HistoryManager.BeginTransaction([e1]);
        e1.Text = "Hello!";
        wsp.Commit(e1, ChangeType.Modify);
        e1.Text = "Hello! World!";
        wsp.Commit(e1, ChangeType.Modify);

        wsp.Undo();
        wsp.Document.EventManager.Head.Text.ShouldBe(string.Empty);
    }

    [Fact]
    public void Redo_ModifyEvent_Multiple()
    {
        var wsp = new Workspace(new Document(true), 1);
        var e1 = wsp.Document.EventManager.Head;

        wsp.Document.HistoryManager.BeginTransaction([e1]);
        e1.Text = "Hello!";
        wsp.Commit(e1, ChangeType.Modify);
        e1.Text = "Hello! World!";
        wsp.Commit(e1, ChangeType.Modify);
        wsp.Undo();

        wsp.Document.HistoryManager.CanRedo.ShouldBeTrue();
        wsp.Redo();

        wsp.Document.EventManager.Head.Text.ShouldBe("Hello! World!");
    }
}
