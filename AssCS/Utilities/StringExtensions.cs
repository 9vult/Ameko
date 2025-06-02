// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Utilities;

public static class StringExtensions
{
    /// <summary>
    /// Naive implementation of Replace-Many
    /// </summary>
    /// <param name="str">String to replace on</param>
    /// <param name="oldValues">Values to be replaced</param>
    /// <param name="newValue">Value to replace with</param>
    /// <returns>String with <paramref name="oldValues"/> replaced with <paramref name="newValue"/></returns>
    public static string ReplaceMany(this string str, string[] oldValues, string newValue)
    {
        return oldValues.Length == 0
            ? str
            : oldValues.Aggregate(str, (current, oldValue) => current.Replace(oldValue, newValue));
    }
}
