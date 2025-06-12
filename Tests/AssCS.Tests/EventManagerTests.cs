// SPDX-License-Identifier: MPL-2.0

using Shouldly;

namespace AssCS.Tests;

public class EventManagerTests
{
    [Fact]
    public void AddAfter()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.GetAfter(1).ShouldBeNull();

        em.AddAfter(1, new Event(2));
        em.GetAfter(1).ShouldNotBeNull();
    }

    [Fact]
    public void AddBefore()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.GetBefore(1).ShouldBeNull();

        em.AddBefore(1, new Event(2));
        em.GetBefore(1).ShouldNotBeNull();
    }

    [Fact]
    public void AddListAfter()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddAfter(1, [new Event(2), new Event(3), new Event(4)]);
        em.Tail.Id.ShouldBe(4);
    }

    [Fact]
    public void AddListBefore_Asc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], true);
        em.Head.Id.ShouldBe(4);
    }

    [Fact]
    public void AddListBefore_Desc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], false);
        em.Head.Id.ShouldBe(2);
    }

    [Fact]
    public void AddLast()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], false);

        em.AddLast(new Event(5));
        em.Tail.Id.ShouldBe(5);
    }

    [Fact]
    public void AddFirst()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst(new Event(2));
        em.Head.Id.ShouldBe(2);
    }

    [Fact]
    public void AddListLast()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddLast([new Event(2), new Event(3), new Event(4)]);
        em.Tail.Id.ShouldBe(4);
    }

    [Fact]
    public void AddListFirst_Asc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst([new Event(2), new Event(3), new Event(4)], true);
        em.Head.Id.ShouldBe(4);
    }

    [Fact]
    public void AddListFirst_Desc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst([new Event(2), new Event(3), new Event(4)], false);
        em.Head.Id.ShouldBe(2);
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
        resultA.ShouldBeTrue();
        if (resultA)
            after!.Id.ShouldBe(4);

        var resultB = em.TryGetBefore(3, out var before);
        resultB.ShouldBeTrue();
        if (resultB)
            before!.Id.ShouldBe(4);
    }

    [Fact]
    public void ReplaceInplace()
    {
        var em = new EventManager();
        var eA = new Event(1) { Text = "A" };
        var eB = new Event(1) { Text = "B" };
        em.AddFirst(eA);

        em.ReplaceInPlace(eB);

        var result = em.TryGet(eA.Id, out var outA);
        result.ShouldBeTrue();
        if (result)
            outA!.Id.ShouldBe(eB.Id);
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

        em.ReplaceInPlace([eB, eD]);

        var resultA = em.TryGet(eA.Id, out var outA);
        resultA.ShouldBeTrue();
        if (resultA)
            outA!.Id.ShouldBe(eB.Id);

        var resultB = em.TryGet(eC.Id, out var outC);
        resultB.ShouldBeTrue();
        if (resultB)
            outC!.Id.ShouldBe(eD.Id);
    }

    [Fact]
    public void Remove()
    {
        var em = new EventManager();
        em.AddFirst([new Event(1), new Event(2), new Event(3)], false);

        em.Count.ShouldBe(3);
        em.Remove(2);
        em.Count.ShouldBe(2);

        var result = em.TryGetAfter(1, out var after);
        result.ShouldBeTrue();
        if (result)
            after!.Id.ShouldBe(3);
    }

    [Fact]
    public void RemoveList()
    {
        var em = new EventManager();
        em.AddFirst([new Event(1), new Event(2), new Event(3)], false);

        em.Count.ShouldBe(3);
        em.Remove([1, 3]);

        em.Count.ShouldBe(1);
        em.Head.Id.ShouldBe(em.Tail.Id);
    }

    [Fact]
    public void Has()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.Has(1).ShouldBeTrue();
        em.Has(2).ShouldBeFalse();
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

        em.Get(1).Style.ShouldBe("C");
        em.Get(2).Style.ShouldBe("B");
        em.Get(3).Style.ShouldBe("C");
    }

    [Fact]
    public void Duplicate()
    {
        var em = new EventManager();
        var original = new Event(1) { Text = "Original" };
        em.AddFirst(original);

        var dupe = em.Duplicate(original);
        dupe.Id.ShouldNotBe(original.Id);

        var result = em.TryGetAfter(original.Id, out var after);
        result.ShouldBeTrue();
        if (result)
            after!.Text.ShouldBe(original.Text);
    }

    [Fact]
    public void InsertBefore_NoOverlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst(e1);

        var e2 = em.InsertBefore(e1);
        (e2.End - e2.Start).TotalSeconds.ShouldBe(5);
    }

    [Fact]
    public void InsertBefore_Overlap()
    {
        var em = new EventManager();
        var e1 = new Event(2) { Start = Time.FromSeconds(5), End = Time.FromSeconds(7) };
        var e2 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst([e1, e2], false);

        var e3 = em.InsertBefore(e2);
        (e3.End - e3.Start).TotalSeconds.ShouldBe(3);
    }

    [Fact]
    public void InsertAfter_NoOverlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst(e1);

        var e2 = em.InsertAfter(e1);
        (e2.End - e2.Start).TotalSeconds.ShouldBe(5);
    }

    [Fact]
    public void InsertAfter_Overlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(5), End = Time.FromSeconds(7) };
        var e2 = new Event(2) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst([e1, e2], false);

        var e3 = em.InsertAfter(e1);
        (e3.End - e3.Start).TotalSeconds.ShouldBe(3);
    }

    [Fact]
    public void Split_MultipleSegments()
    {
        var em = new EventManager();
        var testEvent = new Event(em.NextId)
        {
            Text = "Short\\NLonger segment",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(3000),
        };
        em.AddFirst(testEvent);

        var result = em.Split(testEvent.Id).ToList();

        em.TryGet(testEvent.Id, out _).ShouldBeFalse();
        result.Count.ShouldBe(2);
        result[0].End.TotalMilliseconds.ShouldBeLessThan(result[1].End.TotalMilliseconds);
        result[1].Start.ShouldBe(result[0].End);
        result.Last().End.ShouldBe(testEvent.End);

        foreach (var e in result)
        {
            em.TryGet(e.Id, out var addedEvent).ShouldBeTrue();
            addedEvent.ShouldBeEquivalentTo(e);
        }
    }

    [Fact]
    public void Split_SingleSegment()
    {
        var em = new EventManager();
        var testEvent = new Event(em.NextId)
        {
            Text = "No newlines here",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(3000),
        };
        em.AddFirst(testEvent);

        var result = em.Split(testEvent.Id).ToList();

        em.TryGet(testEvent.Id, out _).ShouldBeFalse();
        result.Count.ShouldBe(1);
        result[0].Text.ShouldBe("No newlines here");
        result[0].Start.ShouldBe(testEvent.Start);
        result[0].End.ShouldBe(testEvent.End);
    }

    [Fact]
    public void Split_NotFound()
    {
        var em = new EventManager();
        var result = em.Split(999);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void Split_Empty()
    {
        var em = new EventManager();
        var testEvent = new Event(em.NextId)
        {
            Text = string.Empty,
            Start = Time.FromMillis(0),
            End = Time.FromMillis(3000),
        };
        em.AddFirst(testEvent);

        var result = em.Split(testEvent.Id);
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Merge_AreAdjacent_Forward()
    {
        var em = new EventManager();
        var eventA = new Event(5)
        {
            Text = "Hello",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(1000),
        };
        var eventB = new Event(10)
        {
            Text = "World",
            Start = eventA.End,
            End = Time.FromMillis(2000),
        };
        em.AddFirst(eventA);
        em.AddAfter(eventA.Id, eventB);

        var result = em.Merge(eventA.Id, eventB.Id);

        result.ShouldNotBeNull();
        result.Text.ShouldBe("Hello\\NWorld");
        result.Start.ShouldBe(eventA.Start);
        result.End.ShouldBe(eventB.End);
    }

    [Fact]
    public void Merge_AreAdjacent_Backward()
    {
        var em = new EventManager();
        var eventA = new Event(5)
        {
            Text = "Hello",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(1000),
        };
        var eventB = new Event(10)
        {
            Text = "World",
            Start = eventA.End,
            End = Time.FromMillis(2000),
        };
        em.AddFirst(eventA);
        em.AddAfter(eventA.Id, eventB);

        var result = em.Merge(eventB.Id, eventA.Id);

        result.ShouldNotBeNull();
        result.Text.ShouldBe("Hello\\NWorld");
        result.Start.ShouldBe(eventA.Start);
        result.End.ShouldBe(eventB.End);
    }

    [Fact]
    public void Merge_UseSoftLinebreaks()
    {
        var em = new EventManager();
        var eventA = new Event(5)
        {
            Text = "Hello",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(1000),
        };
        var eventB = new Event(10)
        {
            Text = "World",
            Start = eventA.End,
            End = Time.FromMillis(2000),
        };
        em.AddFirst(eventA);
        em.AddAfter(eventA.Id, eventB);

        var result = em.Merge(eventA.Id, eventB.Id, true);

        result.ShouldNotBeNull();
        result.Text.ShouldBe("Hello\\nWorld");
        result.Start.ShouldBe(eventA.Start);
        result.End.ShouldBe(eventB.End);
    }

    [Fact]
    public void Merge_NotAdjacent()
    {
        var em = new EventManager();
        var eventA = new Event(5)
        {
            Text = "Hello",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(1000),
        };
        var eventB = new Event(10)
        {
            Text = "Cool",
            Start = eventA.End,
            End = Time.FromMillis(2000),
        };
        var eventC = new Event(15)
        {
            Text = "World",
            Start = eventB.End,
            End = Time.FromMillis(3000),
        };
        em.AddFirst(eventA);
        em.AddAfter(eventA.Id, eventB);
        em.AddAfter(eventB.Id, eventC);

        var result = em.Merge(eventA.Id, eventC.Id);

        result.ShouldBeNull();
    }

    [Fact]
    public void Merge_NotExist()
    {
        var em = new EventManager();
        var result = em.Merge(999, 1);

        result.ShouldBeNull();
    }

    [Fact]
    public void Merge_OriginalsRemoved()
    {
        var em = new EventManager();
        var eventA = new Event(5)
        {
            Text = "Hello",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(1000),
        };
        var eventB = new Event(10)
        {
            Text = "World",
            Start = eventA.End,
            End = Time.FromMillis(2000),
        };
        em.AddFirst(eventA);
        em.AddAfter(eventA.Id, eventB);

        em.Merge(eventA.Id, eventB.Id);

        em.TryGet(eventA.Id, out _).ShouldBeFalse();
        em.TryGet(eventB.Id, out _).ShouldBeFalse();
    }
}
