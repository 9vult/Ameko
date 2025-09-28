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

    /// <summary>
    /// Convert a string representation of a decimal to an integer
    /// </summary>
    /// <param name="value">Numerical string value</param>
    /// <returns>Floored integer</returns>
    /// <remarks>This is needed because Closed Caption Converter exports unholy ASS</remarks>
    public static int ToFlooredInt(this string value)
    {
        return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(Math.Floor(decimal.Parse(value)));
    }
}
