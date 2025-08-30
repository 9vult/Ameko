// SPDX-License-Identifier: MPL-2.0

using AssCS.History;
using Shouldly;

namespace AssCS.Tests;

public class HistoryManagerTests
{
    private Event event1 = new(1);
    private Event event2 = new(2);
    private Style style1 = new(1);

    [Fact]
    public void Event_Commit()
    {
        var hm = new HistoryManager();

        hm.Commit(ChangeType.Add, event1, null, false);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = hm.Undo();
        lastCommit.ShouldBeOfType<EventCommit>();
    }

    [Fact]
    public void Style_Commit()
    {
        var hm = new HistoryManager();

        hm.Commit(ChangeType.Add, style1);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = hm.Undo();
        lastCommit.ShouldBeOfType<StyleCommit>();
        var styleCommit = lastCommit as StyleCommit;
        styleCommit?.Type.ShouldBe(ChangeType.Add);
    }

    [Fact]
    public void Event_Commit_Amend()
    {
        var hm = new HistoryManager();
        hm.Commit(ChangeType.Add, event1, null, false);

        hm.Commit(ChangeType.Add, event2, null, true);

        hm.CanUndo.ShouldBeTrue();
        hm.CanRedo.ShouldBeFalse();

        var lastCommit = (EventCommit)hm.Undo();
        lastCommit.Deltas.Count.ShouldBe(2);
    }

    [Fact]
    public void Event_Commit_Amend_Invalid()
    {
        var hm = new HistoryManager();
        hm.Commit(ChangeType.Add, style1);

        Action amendToStyleCommit = () => hm.Commit(ChangeType.Add, event1, null, true);

        amendToStyleCommit
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Cannot amend to a non-Event commit");
    }

    [Fact]
    public void Event_Commit_Amend_Empty()
    {
        var hm = new HistoryManager();

        Action amendToStyleCommit = () => hm.Commit(ChangeType.Add, event1, null, true);

        amendToStyleCommit
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Cannot amend, no commits in the undo stack!");
    }

    [Fact]
    public void Undo()
    {
        var hm = new HistoryManager();
        hm.Commit(ChangeType.Add, event1, null, false);
        var c = hm.Undo();

        c.ShouldNotBeNull();
        c.ShouldBeOfType<EventCommit>();
        hm.CanUndo.ShouldBe(false);
        hm.CanRedo.ShouldBe(true);
    }

    [Fact]
    public void Undo_Empty()
    {
        var hm = new HistoryManager();

        Action undoAction = () => hm.Undo();

        undoAction
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Cannot undo, no commits in the undo stack!");
    }

    [Fact]
    public void Redo()
    {
        var hm = new HistoryManager();
        hm.Commit(ChangeType.Add, event1, null, false);
        var cU = hm.Undo();
        var cR = hm.Redo();

        cR.ShouldNotBeNull();
        cR.ShouldBeOfType<EventCommit>();
        hm.CanRedo.ShouldBe(false);
        hm.CanUndo.ShouldBe(true);

        cU.ShouldBeSameAs(cR);
    }

    [Fact]
    public void Redo_Empty()
    {
        var hm = new HistoryManager();

        Action redoAction = () => hm.Redo();

        redoAction
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Cannot redo, no commits in the redo stack!");
    }
}
