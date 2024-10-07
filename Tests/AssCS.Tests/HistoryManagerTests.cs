// SPDX-License-Identifier: MPL-2.0

using AssCS.History;
using FluentAssertions;

namespace AssCS.Tests;

public class HistoryManagerTests
{
    private Event event1 = new(1);
    private Event event2 = new(2);

    [Fact]
    public void Commit_NoAmmend()
    {
        var hm = new HistoryManager();

        hm.CanUndo.Should().BeFalse();
        hm.Commit("Test", CommitType.EventAdd, ref event1, null, false);
        hm.CanUndo.Should().BeTrue();
    }

    [Fact]
    public void Commit_Ammend()
    {
        var hm = new HistoryManager();

        hm.CanUndo.Should().BeFalse();
        int c1 = hm.Commit("Test", CommitType.EventAdd, ref event1, null, false);
        int c2 = hm.Commit("Ammend", CommitType.EventAdd, ref event2, null, true);
        hm.CanUndo.Should().BeTrue();
        c1.Should().Be(c2);

        // Get the commit and check the ammend worked
        var c = hm.Undo();
        ((EventCommit)c).Targets.Count.Should().Be(2);
    }

    [Fact]
    public void Undo()
    {
        var hm = new HistoryManager();
        hm.Commit("Test", CommitType.EventAdd, ref event1, null, false);
        var c = hm.Undo();

        c.Should().NotBe(null);
        hm.CanUndo.Should().Be(false);
        hm.CanRedo.Should().Be(true);
    }

    [Fact]
    public void Redo()
    {
        var hm = new HistoryManager();
        hm.Commit("Test", CommitType.EventAdd, ref event1, null, false);
        var cU = hm.Undo();
        var cR = hm.Redo();

        cR.Should().NotBe(null);
        hm.CanRedo.Should().Be(false);
        hm.CanUndo.Should().Be(true);

        cU.Should().BeSameAs(cR);
    }
}
