// SPDX-License-Identifier: MPL-2.0

using AssCS.History;
using FluentAssertions;

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
        var message = "Added event";

        hm.Commit(message, CommitType.EventAdd, ref event1, null, false);

        hm.CanUndo.Should().BeTrue();
        hm.CanRedo.Should().BeFalse();

        var lastCommit = hm.Undo();
        lastCommit.Should().BeOfType<EventCommit>();
        lastCommit.Message.Should().Be(message);
    }

    [Fact]
    public void Style_Commit()
    {
        var hm = new HistoryManager();
        var message = "Added style";

        hm.Commit(message, CommitType.StyleAdd, ref style1);

        hm.CanUndo.Should().BeTrue();
        hm.CanRedo.Should().BeFalse();

        var lastCommit = hm.Undo();
        lastCommit.Should().BeOfType<StyleCommit>();
        lastCommit.Message.Should().Be(message);
    }

    [Fact]
    public void Event_Commit_Amend()
    {
        var hm = new HistoryManager();
        var initial = "Added event";
        var amended = "Amended event";
        hm.Commit(initial, CommitType.EventAdd, ref event1, null, false);

        hm.Commit(amended, CommitType.EventAdd, ref event2, null, true);

        hm.CanUndo.Should().BeTrue();
        hm.CanRedo.Should().BeFalse();

        var lastCommit = (EventCommit)hm.Undo();
        lastCommit.Message.Should().Be($"{initial};{amended}");
        lastCommit.Targets.Should().HaveCount(2);
    }

    [Fact]
    public void Event_Commit_Amend_Invalid()
    {
        var hm = new HistoryManager();
        hm.Commit("Added style", CommitType.StyleAdd, ref style1);

        Action amendToStyleCommit = () =>
            hm.Commit("Invalid amend", CommitType.EventText, ref event1, null, true);

        amendToStyleCommit
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot amend an Event commit to a non-Event commit");
    }

    [Fact]
    public void Undo()
    {
        var hm = new HistoryManager();
        hm.Commit("Test", CommitType.EventAdd, ref event1, null, false);
        var c = hm.Undo();

        c.Should().NotBeNull();
        c.Should().BeOfType<EventCommit>();
        hm.CanUndo.Should().Be(false);
        hm.CanRedo.Should().Be(true);
    }

    [Fact]
    public void Undo_Empty()
    {
        var hm = new HistoryManager();

        Action undoAction = () => hm.Undo();

        undoAction
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot undo, no commits in the undo stack!");
    }

    [Fact]
    public void Redo()
    {
        var hm = new HistoryManager();
        hm.Commit("Test", CommitType.EventAdd, ref event1, null, false);
        var cU = hm.Undo();
        var cR = hm.Redo();

        cR.Should().NotBeNull();
        cR.Should().BeOfType<EventCommit>();
        hm.CanRedo.Should().Be(false);
        hm.CanUndo.Should().Be(true);

        cU.Should().BeSameAs(cR);
    }

    [Fact]
    public void Redo_Empty()
    {
        var hm = new HistoryManager();

        Action redoAction = () => hm.Redo();

        redoAction
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot redo, no commits in the redo stack!");
    }
}
