// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Tests;

public class EventManagerTests
{
    [Test]
    public async Task AddAfter()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        await Assert.That(em.GetAfter(1)).IsNull();

        em.AddAfter(1, new Event(2));
        await Assert.That(em.GetAfter(1)).IsNotNull();
    }

    [Test]
    public async Task AddBefore()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        await Assert.That(em.GetBefore(1)).IsNull();

        em.AddBefore(1, new Event(2));
        await Assert.That(em.GetBefore(1)).IsNotNull();
    }

    [Test]
    public async Task AddListAfter()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddAfter(1, [new Event(2), new Event(3), new Event(4)]);
        await Assert.That(em.Tail.Id).IsEqualTo(4);
    }

    [Test]
    public async Task AddListBefore_Asc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], true);
        await Assert.That(em.Head.Id).IsEqualTo(4);
    }

    [Test]
    public async Task AddListBefore_Desc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], false);
        await Assert.That(em.Head.Id).IsEqualTo(2);
    }

    [Test]
    public async Task AddLast()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.AddBefore(1, [new Event(2), new Event(3), new Event(4)], false);

        em.AddLast(new Event(5));
        await Assert.That(em.Tail.Id).IsEqualTo(5);
    }

    [Test]
    public async Task AddFirst()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst(new Event(2));
        await Assert.That(em.Head.Id).IsEqualTo(2);
    }

    [Test]
    public async Task AddListLast()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddLast([new Event(2), new Event(3), new Event(4)]);
        await Assert.That(em.Tail.Id).IsEqualTo(4);
    }

    [Test]
    public async Task AddListFirst_Asc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst([new Event(2), new Event(3), new Event(4)], true);
        await Assert.That(em.Head.Id).IsEqualTo(4);
    }

    [Test]
    public async Task AddListFirst_Desc()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        em.AddFirst([new Event(2), new Event(3), new Event(4)], false);
        await Assert.That(em.Head.Id).IsEqualTo(2);
    }

    [Test]
    public async Task Replace()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));
        em.AddAfter(1, new Event(2));
        em.AddAfter(2, new Event(3));

        em.Replace(2, new Event(4));

        var resultA = em.TryGetAfter(1, out var after);
        await Assert.That(resultA).IsTrue();
        if (resultA)
            await Assert.That(after!.Id).IsEqualTo(4);

        var resultB = em.TryGetBefore(3, out var before);
        await Assert.That(resultB).IsTrue();
        if (resultB)
            await Assert.That(before!.Id).IsEqualTo(4);
    }

    [Test]
    public async Task ReplaceInplace()
    {
        var em = new EventManager();
        var eA = new Event(1) { Text = "A" };
        var eB = new Event(1) { Text = "B" };
        em.AddFirst(eA);

        em.ReplaceInPlace(eB);

        var result = em.TryGet(eA.Id, out var outA);
        await Assert.That(result).IsTrue();
        if (result)
            await Assert.That(outA!.Id).IsEqualTo(eB.Id);
    }

    [Test]
    public async Task ReplaceInplaceList()
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
        await Assert.That(resultA).IsTrue();
        if (resultA)
            await Assert.That(outA!.Id).IsEqualTo(eB.Id);

        var resultB = em.TryGet(eC.Id, out var outC);
        await Assert.That(resultB).IsTrue();
        if (resultB)
            await Assert.That(outC!.Id).IsEqualTo(eD.Id);
    }

    [Test]
    public async Task Remove()
    {
        var em = new EventManager();
        em.AddFirst([new Event(1), new Event(2), new Event(3)], false);

        await Assert.That(em.Count).IsEqualTo(3);
        em.Remove(2);
        await Assert.That(em.Count).IsEqualTo(2);

        var result = em.TryGetAfter(1, out var after);
        await Assert.That(result).IsTrue();
        if (result)
            await Assert.That(after!.Id).IsEqualTo(3);
    }

    [Test]
    public async Task RemoveList()
    {
        var em = new EventManager();
        em.AddFirst([new Event(1), new Event(2), new Event(3)], false);

        await Assert.That(em.Count).IsEqualTo(3);
        em.Remove([1, 3]);

        await Assert.That(em.Count).IsEqualTo(1);
        await Assert.That(em.Head.Id).IsEqualTo(em.Tail.Id);
    }

    [Test]
    public async Task Has()
    {
        var em = new EventManager();
        em.AddFirst(new Event(1));

        await Assert.That(em.Has(1)).IsTrue();
        await Assert.That(em.Has(2)).IsFalse();
    }

    [Test]
    public async Task ChangeStyle()
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

        await Assert.That(em.Get(1).Style).IsEqualTo("C");
        await Assert.That(em.Get(2).Style).IsEqualTo("B");
        await Assert.That(em.Get(3).Style).IsEqualTo("C");
    }

    [Test]
    public async Task Duplicate()
    {
        var em = new EventManager();
        var original = new Event(1) { Text = "Original" };
        em.AddFirst(original);

        var dupe = em.Duplicate(original);
        await Assert.That(dupe.Id).IsNotEqualTo(original.Id);

        var result = em.TryGetAfter(original.Id, out var after);
        await Assert.That(result).IsTrue();
        if (result)
            await Assert.That(after!.Text).IsEqualTo(original.Text);
    }

    [Test]
    public async Task InsertBefore_NoOverlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst(e1);

        var e2 = em.InsertBefore(e1);
        await Assert.That((e2.End - e2.Start).TotalSeconds).IsEqualTo(5);
    }

    [Test]
    public async Task InsertBefore_Overlap()
    {
        var em = new EventManager();
        var e1 = new Event(2) { Start = Time.FromSeconds(5), End = Time.FromSeconds(7) };
        var e2 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst([e1, e2], false);

        var e3 = em.InsertBefore(e2);
        await Assert.That((e3.End - e3.Start).TotalSeconds).IsEqualTo(3);
    }

    [Test]
    public async Task InsertAfter_NoOverlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst(e1);

        var e2 = em.InsertAfter(e1);
        await Assert.That((e2.End - e2.Start).TotalSeconds).IsEqualTo(5);
    }

    [Test]
    public async Task InsertAfter_Overlap()
    {
        var em = new EventManager();
        var e1 = new Event(1) { Start = Time.FromSeconds(5), End = Time.FromSeconds(7) };
        var e2 = new Event(2) { Start = Time.FromSeconds(10), End = Time.FromSeconds(15) };
        em.AddFirst([e1, e2], false);

        var e3 = em.InsertAfter(e1);
        await Assert.That((e3.End - e3.Start).TotalSeconds).IsEqualTo(3);
    }

    [Test]
    public async Task Split_MultipleSegments()
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

        await Assert.That(em.TryGet(testEvent.Id, out _)).IsFalse();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert
            .That(result[0].End.TotalMilliseconds)
            .IsLessThan(result[1].End.TotalMilliseconds);
        await Assert.That(result[1].Start).IsEqualTo(result[0].End);
        await Assert.That(result.Last().End).IsEqualTo(testEvent.End);

        foreach (var e in result)
        {
            await Assert.That(em.TryGet(e.Id, out var addedEvent)).IsTrue();
            await Assert.That(addedEvent).IsEqualTo(e);
        }
    }

    [Test]
    public async Task Split_SingleSegment()
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

        await Assert.That(em.TryGet(testEvent.Id, out _)).IsFalse();
        await Assert.That(result.Count).IsEqualTo(1);
        await Assert.That(result[0].Text).IsEqualTo("No newlines here");
        await Assert.That(result[0].Start).IsEqualTo(testEvent.Start);
        await Assert.That(result[0].End).IsEqualTo(testEvent.End);
    }

    [Test]
    public async Task Split_NotFound()
    {
        var em = new EventManager();
        var result = em.Split(999);

        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task Split_Empty()
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
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task Split_KeepTimes()
    {
        var em = new EventManager();
        var testEvent = new Event(em.NextId)
        {
            Text = "Short\\NLonger segment",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(3000),
        };
        em.AddFirst(testEvent);

        var result = em.Split(testEvent.Id, keepTimes: true).ToList();

        await Assert.That(em.TryGet(testEvent.Id, out _)).IsFalse();
        await Assert.That(result.Count).IsEqualTo(2);

        await Assert.That(result[0].Start).IsEqualTo(testEvent.Start);
        await Assert.That(result[0].End).IsEqualTo(testEvent.End);
        await Assert.That(result[1].Start).IsEqualTo(testEvent.Start);
        await Assert.That(result[1].End).IsEqualTo(testEvent.End);

        foreach (var e in result)
        {
            await Assert.That(em.TryGet(e.Id, out var addedEvent)).IsTrue();
            await Assert.That(addedEvent).IsEqualTo(e);
        }
    }

    [Test]
    public async Task Split_AtCursor()
    {
        var em = new EventManager();
        var testEvent = new Event(em.NextId)
        {
            Text = "Short Longer segment",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(3000),
        };
        em.AddFirst(testEvent);

        var result = em.Split(testEvent.Id, 6).ToList();

        await Assert.That(em.TryGet(testEvent.Id, out _)).IsFalse();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert
            .That(result[0].End.TotalMilliseconds)
            .IsLessThan(result[1].End.TotalMilliseconds);
        await Assert.That(result[1].Start).IsEqualTo(result[0].End);
        await Assert
            .That(result.Last().End.TotalMilliseconds)
            .IsEqualTo(testEvent.End.TotalMilliseconds);

        foreach (var e in result)
        {
            await Assert.That(em.TryGet(e.Id, out var addedEvent)).IsTrue();
            await Assert.That(addedEvent).IsEqualTo(e);
        }

        await Assert.That(result[0].Text).IsEqualTo("Short ");
        await Assert.That(result[1].Text).IsEqualTo("Longer segment");
    }

    [Test]
    public async Task Split_AtCursor_WithSplitTime()
    {
        var em = new EventManager();
        var testEvent = new Event(em.NextId)
        {
            Text = "Short Longer segment",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(3000),
        };
        em.AddFirst(testEvent);

        var splitTime = Time.FromMillis(1500);

        var result = em.Split(testEvent.Id, 6, false, splitTime).ToList();

        await Assert.That(em.TryGet(testEvent.Id, out _)).IsFalse();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert
            .That(result[0].End.TotalMilliseconds)
            .IsLessThan(result[1].End.TotalMilliseconds);
        await Assert.That(result[1].Start).IsEqualTo(result[0].End);
        await Assert
            .That(result.Last().End.TotalMilliseconds)
            .IsEqualTo(testEvent.End.TotalMilliseconds);

        await Assert.That(result[0].End).IsEqualTo(splitTime);
        await Assert.That(result[1].Start).IsEqualTo(splitTime);

        foreach (var e in result)
        {
            await Assert.That(em.TryGet(e.Id, out var addedEvent)).IsTrue();
            await Assert.That(addedEvent).IsEqualTo(e);
        }

        await Assert.That(result[0].Text).IsEqualTo("Short ");
        await Assert.That(result[1].Text).IsEqualTo("Longer segment");
    }

    [Test]
    public async Task Split_AtCursor_KeepTimes()
    {
        var em = new EventManager();
        var testEvent = new Event(em.NextId)
        {
            Text = "Short Longer segment",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(3000),
        };
        em.AddFirst(testEvent);

        var result = em.Split(testEvent.Id, 6, keepTimes: true).ToList();

        await Assert.That(em.TryGet(testEvent.Id, out _)).IsFalse();
        await Assert.That(result.Count).IsEqualTo(2);

        await Assert.That(result[0].Start).IsEqualTo(testEvent.Start);
        await Assert.That(result[0].End).IsEqualTo(testEvent.End);
        await Assert.That(result[1].Start).IsEqualTo(testEvent.Start);
        await Assert.That(result[1].End).IsEqualTo(testEvent.End);

        foreach (var e in result)
        {
            await Assert.That(em.TryGet(e.Id, out var addedEvent)).IsTrue();
            await Assert.That(addedEvent).IsEqualTo(e);
        }

        await Assert.That(result[0].Text).IsEqualTo("Short ");
        await Assert.That(result[1].Text).IsEqualTo("Longer segment");
    }

    [Test]
    public async Task Split_AtCursor_KeepTimes_And_SplitTime()
    {
        var em = new EventManager();
        var testEvent = new Event(em.NextId)
        {
            Text = "Short Longer segment",
            Start = Time.FromMillis(0),
            End = Time.FromMillis(3000),
        };
        em.AddFirst(testEvent);

        var splitTime = Time.FromMillis(1500);

        await Assert
            .That(() => em.Split(testEvent.Id, 6, true, splitTime))
            .Throws<ArgumentException>()
            .WithMessage("Can't keep times with a specified split time!");
    }

    [Test]
    public async Task Merge()
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

        var result = em.Merge([eventA, eventB]);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Text).IsEqualTo("Hello\\NWorld");
        await Assert.That(result.Start).IsEqualTo(eventA.Start);
        await Assert.That(result.End).IsEqualTo(eventB.End);
    }

    [Test]
    public async Task Merge_UseSoftLinebreaks()
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

        var result = em.Merge([eventA, eventB], true);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Text).IsEqualTo("Hello\\nWorld");
        await Assert.That(result.Start).IsEqualTo(eventA.Start);
        await Assert.That(result.End).IsEqualTo(eventB.End);
    }

    [Test]
    public async Task Merge_Empty()
    {
        var em = new EventManager();
        var result = em.Merge([]);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Merge_OriginalsRemoved()
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

        em.Merge([eventA, eventB]);

        await Assert.That(em.TryGet(eventA.Id, out _)).IsFalse();
        await Assert.That(em.TryGet(eventB.Id, out _)).IsFalse();
    }
}
