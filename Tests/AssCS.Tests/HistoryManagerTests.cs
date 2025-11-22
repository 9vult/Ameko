// SPDX-License-Identifier: MPL-2.0

using AssCS.History;
using Shouldly;

namespace AssCS.Tests;

public class HistoryManagerTests
{
    [Fact]
    public void Commit_AddEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(event1);

        hm.Commit(ChangeType.AddEvent);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.AddEvent);
        lastCommit.Chain.ShouldContain(event1.Id);
        lastCommit.Events[event1.Id].ShouldBe(event1);
    }

    [Fact]
    public void Commit_RemoveEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.AddEvent);

        doc.EventManager.Remove(event1.Id);
        hm.Commit(ChangeType.RemoveEvent);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.RemoveEvent);
        lastCommit.Chain.ShouldNotContain(event1.Id);
        lastCommit.Events.Keys.ShouldNotContain(event1.Id);
    }

    [Fact]
    public void Commit_ModifyEvent()
    {
        var eventA = new Event(100) { Text = "Holo" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(eventA);
        hm.Commit(ChangeType.AddEvent);

        eventA.Text = "Lawrence";
        hm.Commit(ChangeType.ModifyEventText);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.ModifyEventText);
        lastCommit.Chain.ShouldContain(eventA.Id);
        lastCommit.Events[eventA.Id].Text.ShouldBe("Lawrence");
    }

    [Fact]
    public void Commit_ModifyEvent_NoCoalesce()
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
        lastCommit.Type.ShouldBe(ChangeType.ModifyEventText);
        lastCommit.Events[eventA.Id].Text.ShouldBe("Stark");

        hm.Undo();
        lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.ModifyEventText); // Make sure there were 2 modify commits
        lastCommit.Events[eventA.Id].Text.ShouldBe("Fern");
    }

    [Fact]
    public void Commit_ModifyEvent_Coalesce()
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
        lastCommit.Type.ShouldBe(ChangeType.ModifyEventText);
        lastCommit.Events[eventA.Id].Text.ShouldBe("Fern");

        eventA.Text = "Stark";
        hm.Commit(ChangeType.ModifyEventText, eventA, true);

        lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.ModifyEventText);
        lastCommit.Events[eventA.Id].Text.ShouldBe("Stark");

        hm.Undo();
        lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.AddEvent); // Make sure there was only 1 modify commit
    }

    [Fact]
    public void Commit_AddStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);

        hm.Commit(ChangeType.AddStyle);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.AddStyle);
        lastCommit.Styles.ShouldContain(style1);
    }

    [Fact]
    public void Commit_RemoveStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);

        doc.StyleManager.Remove(style1.Name);
        hm.Commit(ChangeType.RemoveStyle);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.RemoveStyle);
        lastCommit.Styles.ShouldNotContain(style1);
    }

    [Fact]
    public void Commit_ModifyStyle()
    {
        var style1 = new Style(100) { Name = "Cool Style 1" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);

        style1.Name = "Uncool!";
        hm.Commit(ChangeType.ModifyStyle);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.ModifyStyle);
        lastCommit.Styles.ShouldContain(style1);
        lastCommit.Styles[0].Name.ShouldBe("Uncool!");
    }

    [Fact]
    public void Commit_ModifyStyle_NoCoalesce()
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
        lastCommit.Type.ShouldBe(ChangeType.ModifyStyle);
        lastCommit.Styles[0].Name.ShouldBe("Jinkies!");

        hm.Undo();
        lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.ModifyStyle); // Make sure there were 2 modify commits
        lastCommit.Styles[0].Name.ShouldBe("Uncool!");
    }

    [Fact]
    public void Commit_ModifyStyle_Coalesce()
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
        lastCommit.Type.ShouldBe(ChangeType.ModifyStyle);
        lastCommit.Styles[0].Name.ShouldBe("Uncool!");

        style1.Name = "Jinkies!";
        hm.Commit(style1, true);

        lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.ModifyStyle);
        lastCommit.Styles[0].Name.ShouldBe("Jinkies!");

        hm.Undo();
        lastCommit = hm.PeekHistory();
        lastCommit.Type.ShouldBe(ChangeType.AddStyle); // Make sure there was only 1 modify commit
    }

    [Fact]
    public void Undo_AddEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.AddEvent);
        doc.EventManager.Events.ShouldContain(event1);

        hm.Undo();

        doc.EventManager.Events.ShouldNotContain(event1);
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.AddEvent);
    }

    [Fact]
    public void Undo_RemoveEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.Initial);

        doc.EventManager.Remove(event1.Id);
        hm.Commit(ChangeType.RemoveEvent);
        doc.EventManager.Events.ShouldNotContain(event1);

        hm.Undo();

        doc.EventManager.Events.ShouldContain(event1);
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.RemoveEvent);
    }

    [Fact]
    public void Undo_ModifyEvent()
    {
        var event1 = new Event(100) { Text = "Hello" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.Initial);

        event1.Text = "Goodbye";
        hm.Commit(ChangeType.ModifyEventText, event1);
        doc.EventManager.Head.Text.ShouldBe("Goodbye");

        hm.Undo();

        doc.EventManager.Head.Text.ShouldBe("Hello");
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.ModifyEventText);
    }

    [Fact]
    public void Undo_AddStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);
        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeTrue();

        hm.Undo();

        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeFalse();
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.AddStyle);
    }

    [Fact]
    public void Undo_RemoveStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Remove(style1.Name);
        hm.Commit(ChangeType.RemoveStyle);
        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeFalse();

        hm.Undo();

        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeTrue();
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.RemoveStyle);
    }

    [Fact]
    public void Undo_ModifyStyle()
    {
        var style1 = new Style(100) { Name = "Hello" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.Initial);

        style1.Name = "Goodbye";
        hm.Commit(style1);
        doc.StyleManager.TryGet("Goodbye", out _).ShouldBeTrue();
        doc.StyleManager.TryGet("Hello", out _).ShouldBeFalse();

        hm.Undo();

        doc.StyleManager.TryGet("Goodbye", out _).ShouldBeFalse();
        doc.StyleManager.TryGet("Hello", out _).ShouldBeTrue();
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.ModifyStyle);
    }

    [Fact]
    public void Redo_AddEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.AddEvent);
        doc.EventManager.Events.ShouldContain(event1);

        hm.Undo();
        doc.EventManager.Events.ShouldNotContain(event1);
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.AddEvent);

        hm.Redo();

        doc.EventManager.Events.ShouldContain(event1);
        hm.CanRedo.ShouldBeFalse();
        commit = hm.PeekHistory();
        commit.Type.ShouldBe(ChangeType.AddEvent);
    }

    [Fact]
    public void Redo_RemoveEvent()
    {
        var event1 = new Event(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.Initial);

        doc.EventManager.Remove(event1.Id);
        hm.Commit(ChangeType.RemoveEvent);
        doc.EventManager.Events.ShouldNotContain(event1);

        hm.Undo();
        doc.EventManager.Events.ShouldContain(event1);
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.RemoveEvent);

        hm.Redo();

        doc.EventManager.Events.ShouldNotContain(event1);
        hm.CanRedo.ShouldBeFalse();
        commit = hm.PeekHistory();
        commit.Type.ShouldBe(ChangeType.RemoveEvent);
    }

    [Fact]
    public void Redo_ModifyEvent()
    {
        var event1 = new Event(100) { Text = "Hello" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.EventManager.AddFirst(event1);
        hm.Commit(ChangeType.Initial);

        event1.Text = "Goodbye";
        hm.Commit(ChangeType.ModifyEventText, event1);
        doc.EventManager.Head.Text.ShouldBe("Goodbye");

        hm.Undo();
        doc.EventManager.Head.Text.ShouldBe("Hello");
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.ModifyEventText);

        hm.Redo();

        doc.EventManager.Head.Text.ShouldBe("Goodbye");
        hm.CanRedo.ShouldBeFalse();
        commit = hm.PeekHistory();
        commit.Type.ShouldBe(ChangeType.ModifyEventText);
    }

    [Fact]
    public void Redo_AddStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.AddStyle);
        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeTrue();

        hm.Undo();
        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeFalse();
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.AddStyle);

        hm.Redo();

        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();
        commit = hm.PeekHistory();
        commit.Type.ShouldBe(ChangeType.AddStyle);
    }

    [Fact]
    public void Redo_RemoveStyle()
    {
        var style1 = new Style(100);
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.Initial);

        doc.StyleManager.Remove(style1.Name);
        hm.Commit(ChangeType.RemoveStyle);
        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeFalse();

        hm.Undo();
        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeTrue();
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.RemoveStyle);

        hm.Redo();

        doc.StyleManager.TryGet(style1.Name, out _).ShouldBeFalse();
        hm.CanRedo.ShouldBeFalse();
        commit = hm.PeekHistory();
        commit.Type.ShouldBe(ChangeType.RemoveStyle);
    }

    [Fact]
    public void Redo_ModifyStyle()
    {
        var style1 = new Style(100) { Name = "Hello" };
        var doc = new Document(false);
        var hm = doc.HistoryManager;
        doc.StyleManager.Add(style1);
        hm.Commit(ChangeType.Initial);

        style1.Name = "Goodbye";
        hm.Commit(style1);
        doc.StyleManager.TryGet("Goodbye", out _).ShouldBeTrue();
        doc.StyleManager.TryGet("Hello", out _).ShouldBeFalse();

        hm.Undo();
        doc.StyleManager.TryGet("Goodbye", out _).ShouldBeFalse();
        doc.StyleManager.TryGet("Hello", out _).ShouldBeTrue();
        hm.CanRedo.ShouldBeTrue();
        var commit = hm.PeekFuture();
        commit.Type.ShouldBe(ChangeType.ModifyStyle);

        hm.Redo();

        doc.StyleManager.TryGet("Goodbye", out _).ShouldBeTrue();
        doc.StyleManager.TryGet("Hello", out _).ShouldBeFalse();
        hm.CanRedo.ShouldBeFalse();
        commit = hm.PeekHistory();
        commit.Type.ShouldBe(ChangeType.ModifyStyle);
    }
}
