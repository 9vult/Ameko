// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Runtime.CompilerServices;

namespace Ameko.Utilities;

/// <summary>
/// Select functions from Avalonia's deprecated MathUtilities class
/// </summary>
public static class MathUtils
{
    // smallest such that 1.0+DoubleEpsilon != 1.0
    private const double DoubleEpsilon = 2.2204460492503131e-016;
    private const float FloatEpsilon = 1.192092896e-07F;

    /// <summary>
    /// Returns whether two doubles are "close".
    /// That is, whether they are within epsilon of each other.
    /// </summary>
    /// <param name="value1">The first double to compare</param>
    /// <param name="value2">The second double to compare</param>
    public static bool AreClose(double value1, double value2)
    {
        //in case they are Infinities (then epsilon check does not work)
        if (value1.Equals(value2))
            return true;
        var eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DoubleEpsilon;
        var delta = value1 - value2;
        return -eps < delta && eps > delta;
    }

    /// <summary>
    /// Returns whether the first double is less than the second double.
    /// That is, whether the first is strictly less than *and* not within epsilon of
    /// the other number.
    /// </summary>
    /// <param name="value1">The first double to compare</param>
    /// <param name="value2">The second double to compare</param>
    public static bool LessThan(double value1, double value2)
    {
        return value1 < value2 && !AreClose(value1, value2);
    }

    /// <summary>
    /// Returns whether the first double is greater than the second double.
    /// That is, whether the first is strictly greater than *and* not within epsilon of
    /// the other number.
    /// </summary>
    /// <param name="value1">The first double to compare</param>
    /// <param name="value2">The second double to compare</param>
    public static bool GreaterThan(double value1, double value2)
    {
        return value1 > value2 && !AreClose(value1, value2);
    }

    /// <summary>
    /// Returns whether the first double is greater than or close to
    /// the second double.  That is, whether the first is strictly greater than or within
    /// epsilon of the other number.
    /// </summary>
    /// <param name="value1">The first double to compare</param>
    /// <param name="value2">The second double to compare</param>
    public static bool GreaterThanOrClose(double value1, double value2)
    {
        return value1 > value2 || AreClose(value1, value2);
    }

    /// <summary>
    /// Clamps a value between a minimum and maximum value
    /// </summary>
    /// <param name="val">The value</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <returns>The clamped value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Clamp(double val, double min, double max)
    {
        if (min > max)
        {
            ThrowCannotBeGreaterThanException(min, max);
        }

        if (val < min)
        {
            return min;
        }

        return val > max ? max : val;
    }

    private static void ThrowCannotBeGreaterThanException<T>(T min, T max)
    {
        throw new ArgumentException($"{min} cannot be greater than {max}.");
    }
}
