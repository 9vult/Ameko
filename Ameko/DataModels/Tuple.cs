// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;

namespace Ameko.DataModels;

/// <summary>
/// A janky reimplementation of a tuple since binding requires `get;` properties
/// </summary>
/// <param name="item1">First item in the tuple</param>
/// <param name="item2">Second item in the tuple</param>
/// <typeparam name="T1">Type of <paramref name="item1"/></typeparam>
/// <typeparam name="T2">Type of <paramref name="item2"/></typeparam>
internal class Tuple<T1, T2>(T1 item1, T2 item2)
{
    /// <summary>
    /// The first item in the tuple
    /// </summary>
    public T1 Item1 { get; } = item1;

    /// <summary>
    /// The second item in the tuple
    /// </summary>
    public T2 Item2 { get; } = item2;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not Tuple<T1, T2> other)
            return false;
        if (Item1 is not null)
            return Item1.Equals(other.Item1);
        return other.Item1 is null;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Item1 is null ? 0 : EqualityComparer<T1>.Default.GetHashCode(Item1);
    }
}
