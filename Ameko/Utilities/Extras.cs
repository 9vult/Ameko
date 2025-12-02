// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;

namespace Ameko.Utilities;

public static class Extras
{
    /// <summary>
    /// Enumerate over the set flags in a [Flags] enum
    /// </summary>
    /// <param name="value">Enum to evaluate</param>
    /// <returns>Enumerable</returns>
    public static IEnumerable<T> GetFlags<T>(this T value)
        where T : Enum
    {
        var bits = Convert.ToUInt64(value);
        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagValue = Convert.ToUInt64(flag);
            if (flagValue != 0 && (bits & flagValue) == flagValue)
                yield return flag;
        }
    }
}
