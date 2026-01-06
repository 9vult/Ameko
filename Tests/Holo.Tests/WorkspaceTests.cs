// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using AssCS;
using AssCS.History;
using Holo.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TestingUtils;

namespace Holo.Tests;

public class WorkspaceTests
{
    private static Workspace CreateWorkspace(Document document, int id)
    {
        var persist = new Persistence(new MockFileSystem(), NullLogger<Persistence>.Instance);
        var config = new Configuration.Configuration(
            new MockFileSystem(),
            NullLogger<Configuration.Configuration>.Instance
        );

        return new Workspace(
            document,
            id,
            null,
            NullLogger<Workspace>.Instance,
            config,
            new MediaController(
                new NullSourceProvider(),
                NullLogger<MediaController>.Instance,
                persist
            )
        );
    }

    [Test]
    public async Task Commit_Single()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e = new Event(99);
        wsp.Document.EventManager.AddLast(e);

        wsp.Commit(e, ChangeType.AddEvent);

        await Assert.That(wsp.Document.HistoryManager.CanUndo).IsTrue();
        await Assert
            .That(wsp.Document.HistoryManager.LastCommitType)
            .IsEqualTo(ChangeType.AddEvent);
    }

    [Test]
    public async Task Commit_Multiple()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Document.EventManager.AddLast(e2);

        wsp.Commit([e1, e2], ChangeType.AddEvent);

        await Assert.That(wsp.Document.HistoryManager.CanUndo).IsTrue();
        await Assert
            .That(wsp.Document.HistoryManager.LastCommitType)
            .IsEqualTo(ChangeType.AddEvent);
    }

    [Test]
    public async Task Commit_Amends_When_Same_Type()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.AddEvent);

        e1.Text = "Hello";
        wsp.Commit(e1, ChangeType.ModifyEventText);

        e1.Text = "Hello World";
        wsp.Commit(e1, ChangeType.ModifyEventText);

        await Assert.That(wsp.Document.HistoryManager.CanUndo).IsTrue();
    }

    [Test]
    public async Task Commit_Does_Not_Amend_When_Different_Type()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.AddEvent);

        e1.Text = "test";
        wsp.Commit(wsp.Document.EventManager.Tail, ChangeType.ModifyEventText);

        await Assert.That(wsp.Document.HistoryManager.CanUndo).IsTrue();
    }

    [Test]
    public async Task Select_Single()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        wsp.SelectionManager.EndSelectionChange();
        var e = new Event(99);
        wsp.Document.EventManager.AddLast(e);

        wsp.SelectionManager.Select(e);

        await Assert.That(wsp.SelectionManager.ActiveEvent).IsEqualTo(e);
        await Assert.That(wsp.SelectionManager.SelectedEventCollection.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Select_Multiple()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        wsp.SelectionManager.EndSelectionChange();
        var e1 = new Event(99);
        var e2 = new Event(100);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Document.EventManager.AddLast(e2);

        wsp.SelectionManager.Select(e2, [e1, e2]);

        await Assert.That(wsp.SelectionManager.ActiveEvent).IsEqualTo(e2);
        await Assert.That(wsp.SelectionManager.SelectedEventCollection.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Undo_AddEvent()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);

        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.AddEvent);
        wsp.Document.EventManager.AddLast(e2);
        wsp.Commit(e2, ChangeType.AddEvent);

        await Assert.That(wsp.Document.HistoryManager.CanUndo).IsTrue();
        wsp.Undo();
        await Assert.That(wsp.Document.EventManager.Events).DoesNotContain(e2);
        await Assert.That(wsp.Document.EventManager.Events).Contains(e1);

        await Assert.That(wsp.Document.HistoryManager.CanRedo).IsTrue();
        wsp.Undo();
        await Assert.That(wsp.Document.EventManager.Events).DoesNotContain(e2);
        await Assert.That(wsp.Document.EventManager.Events).DoesNotContain(e1);
    }

    [Test]
    public async Task Redo_AddEvent()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = new Event(99);
        var e2 = new Event(100);

        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.AddEvent);
        wsp.Document.EventManager.AddLast(e2);
        wsp.Commit(e2, ChangeType.AddEvent);
        wsp.Undo();
        wsp.Undo();

        await Assert.That(wsp.Document.EventManager.Events).DoesNotContain(e2);
        await Assert.That(wsp.Document.EventManager.Events).DoesNotContain(e1);
        await Assert.That(wsp.Document.HistoryManager.CanUndo).IsFalse();
        await Assert.That(wsp.Document.HistoryManager.CanRedo).IsTrue();

        wsp.Redo();
        await Assert.That(wsp.Document.EventManager.Events).Contains(e1);
        await Assert.That(wsp.Document.EventManager.Events).DoesNotContain(e2);

        wsp.Redo();
        await Assert.That(wsp.Document.EventManager.Events).Contains(e1);
        await Assert.That(wsp.Document.EventManager.Events).Contains(e2);

        await Assert.That(wsp.Document.HistoryManager.CanUndo).IsTrue();
        await Assert.That(wsp.Document.HistoryManager.CanRedo).IsFalse();
    }

    [Test]
    public async Task Undo_RemoveEvent()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.AddEvent);

        wsp.Document.EventManager.Remove(e1.Id);
        wsp.Commit(e1, ChangeType.RemoveEvent);

        await Assert.That(wsp.Document.EventManager.Events).DoesNotContain(e1);
        await Assert.That(wsp.Document.HistoryManager.CanUndo).IsTrue();

        wsp.Undo();
        await Assert.That(wsp.Document.EventManager.Events).Contains(e1);
    }

    [Test]
    public async Task Redo_RemoveEvent()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = new Event(99);
        wsp.Document.EventManager.AddLast(e1);
        wsp.Commit(e1, ChangeType.AddEvent);

        wsp.Document.EventManager.Remove(e1.Id);
        wsp.Commit(e1, ChangeType.RemoveEvent);
        wsp.Undo();
        await Assert.That(wsp.Document.EventManager.Events).Contains(e1);
        await Assert.That(wsp.Document.HistoryManager.CanRedo).IsTrue();

        wsp.Redo();
        await Assert.That(wsp.Document.EventManager.Events).DoesNotContain(e1);
    }

    [Test]
    public async Task Undo_ModifyEvent_Once()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = wsp.Document.EventManager.Head;

        e1.Text = "Hello!";
        wsp.Commit(e1, ChangeType.ModifyEventText);

        wsp.Undo();
        await Assert.That(wsp.Document.EventManager.Head.Text).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Redo_ModifyEvent_Once()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = wsp.Document.EventManager.Head;

        e1.Text = "Hello!";
        wsp.Commit(e1, ChangeType.ModifyEventText);
        wsp.Undo();

        await Assert.That(wsp.Document.HistoryManager.CanRedo).IsTrue();
        wsp.Redo();

        await Assert.That(wsp.Document.EventManager.Head.Text).IsEqualTo("Hello!");
    }

    [Test]
    public async Task Undo_ModifyEvent_Multiple()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = wsp.Document.EventManager.Head;

        e1.Text = "Hello!";
        wsp.Commit(e1, ChangeType.ModifyEventText);
        e1.Text = "Hello! World!";
        wsp.Commit(e1, ChangeType.ModifyEventText, true);

        wsp.Undo();
        await Assert.That(wsp.Document.EventManager.Head.Text).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Redo_ModifyEvent_Multiple()
    {
        var wsp = CreateWorkspace(new Document(true), 1);
        var e1 = wsp.Document.EventManager.Head;

        e1.Text = "Hello!";
        wsp.Commit(e1, ChangeType.ModifyEventText);
        e1.Text = "Hello! World!";
        wsp.Commit(e1, ChangeType.ModifyEventText);
        wsp.Undo();

        await Assert.That(wsp.Document.HistoryManager.CanRedo).IsTrue();
        wsp.Redo();

        await Assert.That(wsp.Document.EventManager.Head.Text).IsEqualTo("Hello! World!");
    }
}
