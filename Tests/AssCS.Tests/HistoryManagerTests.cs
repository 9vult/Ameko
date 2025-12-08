// SPDX-License-Identifier: MPL-2.0

using AssCS.History;

namespace AssCS.Tests;

public class HistoryManagerTests
{
    [Test]
    public async Task Commit_AddEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(event1);

        hm.Commit(ChangeType.AddEvent);

        await Assert.That(hm.CanUndo).IsTrue();
        await Assert.That(hm.CanRedo).IsFalse();

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.AddEvent);
        await Assert.That(lastCommit.Chain).Contains(event1.Id);
        await Assert.That(lastCommit.Events[event1.Id]).IsEqualTo(event1);
    }

    [Test]
    public async Task Commit_RemoveEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.AddEvent);

        doc.EventManager.Remove(event1.Id);
        hm.Commit(ChangeType.RemoveEvent);

        await Assert.That(hm.CanUndo).IsTrue();
        await Assert.That(hm.CanRedo).IsFalse();

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.RemoveEvent);
        await Assert.That(lastCommit.Chain).DoesNotContain(event1.Id);
        await Assert.That(lastCommit.Events.Keys).DoesNotContain(event1.Id);
    }

    [Test]
    public async Task Commit_ModifyEvent()
    {
        var eventA = new Event(100) { Text = "Holo" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(eventA);
        hm.Commit(ChangeType.AddEvent);

        eventA.Text = "Lawrence";
        hm.Commit(ChangeType.ModifyEventText);

        await Assert.That(hm.CanUndo).IsTrue();
        await Assert.That(hm.CanRedo).IsFalse();

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyEventText);
        await Assert.That(lastCommit.Chain).Contains(eventA.Id);
        await Assert.That(lastCommit.Events[eventA.Id].Text).IsEqualTo("Lawrence");
    }

    [Test]
    public async Task Commit_ModifyEvent_NoCoalesce()
    {
        var eventA = new Event(100) { Text = "Frieren" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(eventA);
        hm.Commit(ChangeType.AddEvent);

        eventA.Text = "Fern";
        hm.Commit(ChangeType.ModifyEventText);

        eventA.Text = "Stark";
        hm.Commit(ChangeType.ModifyEventText, eventA);

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyEventText);
        await Assert.That(lastCommit.Events[eventA.Id].Text).IsEqualTo("Stark");

        hm.Undo();
        lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyEventText); // Make sure there were 2 modify commits
        await Assert.That(lastCommit.Events[eventA.Id].Text).IsEqualTo("Fern");
    }

    [Test]
    public async Task Commit_ModifyEvent_Coalesce()
    {
        var eventA = new Event(100) { Text = "Frieren" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(eventA);
        hm.Commit(ChangeType.AddEvent);

        eventA.Text = "Fern";
        hm.Commit(ChangeType.ModifyEventText);

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyEventText);
        await Assert.That(lastCommit.Events[eventA.Id].Text).IsEqualTo("Fern");

        eventA.Text = "Stark";
        hm.Commit(ChangeType.ModifyEventText, eventA, true);

        lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyEventText);
        await Assert.That(lastCommit.Events[eventA.Id].Text).IsEqualTo("Stark");

        hm.Undo();
        lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.AddEvent); // Make sure there was only 1 modify commit
    }

    [Test]
    public async Task Commit_AddStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);

        hm.Commit(ChangeType.AddStyle);

        await Assert.That(hm.CanUndo).IsTrue();
        await Assert.That(hm.CanRedo).IsFalse();

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.AddStyle);
        await Assert.That(lastCommit.Styles).Contains(style1);
    }

    [Test]
    public async Task Commit_RemoveStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);

        doc.StyleManager.Remove(style1.Name);
        hm.Commit(ChangeType.RemoveStyle);

        await Assert.That(hm.CanUndo).IsTrue();
        await Assert.That(hm.CanRedo).IsFalse();

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.RemoveStyle);
        await Assert.That(lastCommit.Styles).DoesNotContain(style1);
    }

    [Test]
    public async Task Commit_ModifyStyle()
    {
        var style1 = new Style(100) { Name = "Cool Style 1" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);

        style1.Name = "Uncool!";
        hm.Commit(ChangeType.ModifyStyle);

        await Assert.That(hm.CanUndo).IsTrue();
        await Assert.That(hm.CanRedo).IsFalse();

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyStyle);
        await Assert.That(lastCommit.Styles).Contains(style1);
        await Assert.That(lastCommit.Styles[0].Name).IsEqualTo("Uncool!");
    }

    [Test]
    public async Task Commit_ModifyStyle_NoCoalesce()
    {
        var style1 = new Style(100) { Name = "Cool Style 1" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);

        style1.Name = "Uncool!";
        hm.Commit(ChangeType.ModifyStyle);

        style1.Name = "Jinkies!";
        hm.Commit(style1);

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyStyle);
        await Assert.That(lastCommit.Styles[0].Name).IsEqualTo("Jinkies!");

        hm.Undo();
        lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyStyle); // Make sure there were 2 modify commits
        await Assert.That(lastCommit.Styles[0].Name).IsEqualTo("Uncool!");
    }

    [Test]
    public async Task Commit_ModifyStyle_Coalesce()
    {
        var style1 = new Style(100) { Name = "Cool Style 1" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);

        style1.Name = "Uncool!";
        hm.Commit(ChangeType.ModifyStyle);

        var lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyStyle);
        await Assert.That(lastCommit.Styles[0].Name).IsEqualTo("Uncool!");

        style1.Name = "Jinkies!";
        hm.Commit(style1, true);

        lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.ModifyStyle);
        await Assert.That(lastCommit.Styles[0].Name).IsEqualTo("Jinkies!");

        hm.Undo();
        lastCommit = hm.PeekHistory();
        await Assert.That(lastCommit.Type).IsEqualTo(ChangeType.AddStyle); // Make sure there was only 1 modify commit
    }

    [Test]
    public async Task Undo_AddEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.AddEvent);
        await Assert.That(doc.EventManager.Events).Contains(event1);

        hm.Undo();

        await Assert.That(doc.EventManager.Events).DoesNotContain(event1);
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.AddEvent);
    }

    [Test]
    public async Task Undo_RemoveEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.Initial);

        doc.EventManager.Remove(event1.Id);
        hm.Commit(ChangeType.RemoveEvent);
        await Assert.That(doc.EventManager.Events).DoesNotContain(event1);

        hm.Undo();

        await Assert.That(doc.EventManager.Events).Contains(event1);
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.RemoveEvent);
    }

    [Test]
    public async Task Undo_ModifyEvent()
    {
        var event1 = new Event(100) { Text = "Hello" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.Initial);

        event1.Text = "Goodbye";
        hm.Commit(ChangeType.ModifyEventText, event1);
        await Assert.That(doc.EventManager.Head.Text).IsEqualTo("Goodbye");

        hm.Undo();

        await Assert.That(doc.EventManager.Head.Text).IsEqualTo("Hello");
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.ModifyEventText);
    }

    [Test]
    public async Task Undo_AddStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);
        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsTrue();

        hm.Undo();

        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsFalse();
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.AddStyle);
    }

    [Test]
    public async Task Undo_RemoveStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Remove(style1.Name);
        hm.Commit(ChangeType.RemoveStyle);
        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsFalse();

        hm.Undo();

        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsTrue();
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.RemoveStyle);
    }

    [Test]
    public async Task Undo_ModifyStyle()
    {
        var style1 = new Style(100) { Name = "Hello" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.Initial);

        style1.Name = "Goodbye";
        hm.Commit(style1);
        await Assert.That(doc.StyleManager.TryGet("Goodbye", out _)).IsTrue();
        await Assert.That(doc.StyleManager.TryGet("Hello", out _)).IsFalse();

        hm.Undo();

        await Assert.That(doc.StyleManager.TryGet("Goodbye", out _)).IsFalse();
        await Assert.That(doc.StyleManager.TryGet("Hello", out _)).IsTrue();
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.ModifyStyle);
    }

    [Test]
    public async Task Redo_AddEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.AddEvent);
        await Assert.That(doc.EventManager.Events).Contains(event1);

        hm.Undo();
        await Assert.That(doc.EventManager.Events).DoesNotContain(event1);
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.AddEvent);

        hm.Redo();

        await Assert.That(doc.EventManager.Events).Contains(event1);
        await Assert.That(hm.CanRedo).IsFalse();
        commit = hm.PeekHistory();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.AddEvent);
    }

    [Test]
    public async Task Redo_RemoveEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.Initial);

        doc.EventManager.Remove(event1.Id);
        hm.Commit(ChangeType.RemoveEvent);
        await Assert.That(doc.EventManager.Events).DoesNotContain(event1);

        hm.Undo();
        await Assert.That(doc.EventManager.Events).Contains(event1);
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.RemoveEvent);

        hm.Redo();

        await Assert.That(doc.EventManager.Events).DoesNotContain(event1);
        await Assert.That(hm.CanRedo).IsFalse();
        commit = hm.PeekHistory();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.RemoveEvent);
    }

    [Test]
    public async Task Redo_ModifyEvent()
    {
        var event1 = new Event(100) { Text = "Hello" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.Initial);

        event1.Text = "Goodbye";
        hm.Commit(ChangeType.ModifyEventText, event1);
        await Assert.That(doc.EventManager.Head.Text).IsEqualTo("Goodbye");

        hm.Undo();
        await Assert.That(doc.EventManager.Head.Text).IsEqualTo("Hello");
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.ModifyEventText);

        hm.Redo();

        await Assert.That(doc.EventManager.Head.Text).IsEqualTo("Goodbye");
        await Assert.That(hm.CanRedo).IsFalse();
        commit = hm.PeekHistory();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.ModifyEventText);
    }

    [Test]
    public async Task Redo_AddStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);
        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsTrue();

        hm.Undo();
        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsFalse();
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.AddStyle);

        hm.Redo();

        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsTrue();
        await Assert.That(hm.CanRedo).IsFalse();
        commit = hm.PeekHistory();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.AddStyle);
    }

    [Test]
    public async Task Redo_RemoveStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Remove(style1.Name);
        hm.Commit(ChangeType.RemoveStyle);
        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsFalse();

        hm.Undo();
        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsTrue();
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.RemoveStyle);

        hm.Redo();

        await Assert.That(doc.StyleManager.TryGet(style1.Name, out _)).IsFalse();
        await Assert.That(hm.CanRedo).IsFalse();
        commit = hm.PeekHistory();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.RemoveStyle);
    }

    [Test]
    public async Task Redo_ModifyStyle()
    {
        var style1 = new Style(100) { Name = "Hello" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.Initial);

        style1.Name = "Goodbye";
        hm.Commit(style1);
        await Assert.That(doc.StyleManager.TryGet("Goodbye", out _)).IsTrue();
        await Assert.That(doc.StyleManager.TryGet("Hello", out _)).IsFalse();

        hm.Undo();
        await Assert.That(doc.StyleManager.TryGet("Goodbye", out _)).IsFalse();
        await Assert.That(doc.StyleManager.TryGet("Hello", out _)).IsTrue();
        await Assert.That(hm.CanRedo).IsTrue();
        var commit = hm.PeekFuture();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.ModifyStyle);

        hm.Redo();

        await Assert.That(doc.StyleManager.TryGet("Goodbye", out _)).IsTrue();
        await Assert.That(doc.StyleManager.TryGet("Hello", out _)).IsFalse();
        await Assert.That(hm.CanRedo).IsFalse();
        commit = hm.PeekHistory();
        await Assert.That(commit.Type).IsEqualTo(ChangeType.ModifyStyle);
    }
}
