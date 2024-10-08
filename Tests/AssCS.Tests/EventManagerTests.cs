// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;

namespace AssCS.Tests;

public class EventManagerTests
{
    [Fact]
    public void AddAfter()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.GetAfter(1).Should().BeNull();

        em.AddAfter(1, new Event(2));
        em.GetAfter(1).Should().NotBeNull();
    }

    [Fact]
    public void AddBefore()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.GetBefore(1).Should().BeNull();

        em.AddBefore(1, new Event(2));
        em.GetBefore(1).Should().NotBeNull();
    }

    [Fact]
    public void AddListAfter()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddAfter(1, [new Event(2), new Event(3), new Event(4)]);
        em.Tail.Id.Should().Be(4);
    }

    [Fact]
    public void AddListBefore_Asc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], true);
        em.Head.Id.Should().Be(4);
    }

    [Fact]
    public void AddListBefore_Desc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], false);
        em.Head.Id.Should().Be(2);
    }

    [Fact]
    public void AddLast()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], false);

        em.AddLast(new Event(5));
        em.Tail.Id.Should().Be(5);
    }

    [Fact]
    public void AddFirst()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst(new Event(2));
        em.Head.Id.Should().Be(2);
    }

    [Fact]
    public void AddListLast()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddLast([new Event(2), new Event(3), new Event(4)]);
        em.Tail.Id.Should().Be(4);
    }

    [Fact]
    public void AddListFirst_Asc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst([new Event(2), new Event(3), new Event(4)], true);
        em.Head.Id.Should().Be(4);
    }

    [Fact]
    public void AddListFirst_Desc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst([new Event(2), new Event(3), new Event(4)], false);
        em.Head.Id.Should().Be(2);
    }

    [Fact]
    public void Replace()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.AddAfter(1, new Event(2));
        em.AddAfter(2, new Event(3));

        em.Replace(2, new Event(4));

        var resultA = em.TryGetAfter(1, out var after);
        resultA.Should().BeTrue();
        if (resultA)
            after!.Id.Should().Be(4);

        var resultB = em.TryGetBefore(3, out var before);
        resultB.Should().BeTrue();
        if (resultB)
            before!.Id.Should().Be(4);
    }

    [Fact]
    public void ReplaceInplace()
    {
        var em = new EventManager();
        var eA = new Event(1) { Text = "A" };
        var eB = new Event(1) { Text = "B" };
        em.AddFirst(eA);

        em.ReplaceInplace(eB);

        var result = em.TryGet(eA.Id, out var outA);
        result.Should().BeTrue();
        if (result)
            outA!.Id.Should().Be(eB.Id);
    }

    [Fact]
    public void ReplaceInplaceList()
    {
        var em = new EventManager();
        var eA = new Event(1) { Text = "A" };
        var eB = new Event(1) { Text = "B" };
        var eC = new Event(2) { Text = "C" };
        var eD = new Event(2) { Text = "D" };
        em.AddFirst(eA);
        em.AddAfter(eA.Id, eC);

        em.ReplaceInplace([eB, eD]);

        var resultA = em.TryGet(eA.Id, out var outA);
        resultA.Should().BeTrue();
        if (resultA)
            outA!.Id.Should().Be(eB.Id);

        var resultB = em.TryGet(eC.Id, out var outC);
        resultB.Should().BeTrue();
        if (resultB)
            outC!.Id.Should().Be(eD.Id);
    }

    [Fact]
    public void Remove()
    {
        var em = new EventManager();
        em.AddFirst([new Event(1), new Event(2), new Event(3)], false);

        em.Count.Should().Be(3);
        em.Remove(2);
        em.Count.Should().Be(2);

        var result = em.TryGetAfter(1, out var after);
        result.Should().BeTrue();
        if (result)
            after!.Id.Should().Be(3);
    }

    [Fact]
    public void RemoveList()
    {
        var em = new EventManager();
        em.AddFirst([new Event(1), new Event(2), new Event(3)], false);

        em.Count.Should().Be(3);
        em.Remove([1, 3]);

        em.Count.Should().Be(1);
        em.Head.Id.Should().Be(em.Tail.Id);
    }

    [Fact]
    public void Has()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.Has(1).Should().BeTrue();
        em.Has(2).Should().BeFalse();
    }

    [Fact]
    public void ChangeStyle()
    {
        var em = new EventManager();
        em.AddFirst(
            [
                new Event(1) { Style = "A" },
                new Event(2) { Style = "B" },
                new Event(3) { Style = "A" },
            ],
            false
        );

        em.ChangeStyle("A", "C");

        em.Get(1)!.Style.Should().Be("C");
        em.Get(2)!.Style.Should().Be("B");
        em.Get(3)!.Style.Should().Be("C");
    }

    [Fact]
    public void Duplicate()
    {
        var em = new EventManager();
        var original = new Event(1) { Text = "Original" };
        em.AddFirst(original);

        var dupe = em.Duplicate(original);
        dupe.Id.Should().NotBe(original.Id);

        var result = em.TryGetAfter(original.Id, out var after);
        result.Should().BeTrue();
        if (result)
            after!.Text.Should().Be(original.Text);
    }

    [Fact]
    public void InsertBefore_NoOverlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst(e1);

        var e2 = em.InsertBefore(e1);
        (e2.End - e2.Start).TotalSeconds.Should().Be(5);
    }

    [Fact]
    public void InsertBefore_Overlap()
    {
        var em = new EventManager();
        var e1 = new Event(2) { Start = Time.FromSeconds(5), End = Time.FromSeconds(7) };
        var e2 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst([e1, e2], false);

        var e3 = em.InsertBefore(e2);
        (e3.End - e3.Start).TotalSeconds.Should().Be(3);
    }

    [Fact]
    public void InsertAfter_NoOverlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst(e1);

        var e2 = em.InsertAfter(e1);
        (e2.End - e2.Start).TotalSeconds.Should().Be(5);
    }

    [Fact]
    public void InsertAfter_Overlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(5), End = Time.FromSeconds(7) };
        var e2 = new Event(2) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst([e1, e2], false);

        var e3 = em.InsertAfter(e1);
        (e3.End - e3.Start).TotalSeconds.Should().Be(3);
    }
}
