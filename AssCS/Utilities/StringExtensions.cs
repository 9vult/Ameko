// SPDX-License-Identifier: MPL-2.0

using System.Globalization;

namespace AssCS.Utilities;

internal static class StringExtensions
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
    /// Parse an integer from a string
    /// </summary>
    /// <param name="value">String to parse</param>
    /// <returns>Integer</returns>
    /// <remarks>Positive & negative decimal, hexadecimal, and exponential numbers are supported</remarks>
    public static int ParseAssInt(this string value)
    {
        return Convert.ToInt32(Math.Truncate(ParseAssDouble(value)));
    }

    /// <summary>
    /// Parse a double from a string
    /// </summary>
    /// <param name="value">String to parse</param>
    /// <returns>Double</returns>
    /// <remarks>Positive & negative decimal, hexadecimal, and exponential numbers are supported</remarks>
    public static double ParseAssDouble(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        // Hex number
        if (value.Contains("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            var sign = value[0] == '-' ? -1 : 1;
            if (value[0] == '+' || value[0] == '-')
                value = value[3..]; // -0x
            else
                value = value[2..]; // 0x

            if (
                int.TryParse(
                    value,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out var hex
                )
            )
                return hex * sign;
            return 0;
        }

        // Exponential or Decimal number
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var flt))
            return flt;
        return 0;
    }
}
