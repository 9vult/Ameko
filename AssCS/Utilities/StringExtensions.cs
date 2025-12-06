// SPDX-License-Identifier: MPL-2.0

using System.Globalization;

namespace AssCS.Utilities;

public static class StringExtensions
{
    /// <param name="str">String to replace on</param>
    extension(string str)
    {
        /// <summary>
        /// Naive implementation of Replace-Many
        /// </summary>
        /// <param name="oldValues">Values to be replaced</param>
        /// <param name="newValue">Value to replace with</param>
        /// <returns>String with <paramref name="oldValues"/> replaced with <paramref name="newValue"/></returns>
        public string ReplaceMany(string[] oldValues, string newValue)
        {
            return oldValues.Length == 0
                ? str
                : oldValues.Aggregate(
                    str,
                    (current, oldValue) => current.Replace(oldValue, newValue)
                );
        }

        /// <summary>
        /// Parse an integer from a string
        /// </summary>
        /// <returns>Integer</returns>
        /// <remarks>Positive & negative decimal, hexadecimal, and exponential numbers are supported</remarks>
        public int ParseAssInt()
        {
            return Convert.ToInt32(Math.Truncate(ParseAssDouble(str)));
        }

        /// <summary>
        /// Parse a double from a string
        /// </summary>
        /// <returns>Double</returns>
        /// <remarks>Positive & negative decimal, hexadecimal, and exponential numbers are supported</remarks>
        public double ParseAssDouble()
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0;

            // Hex number
            if (str.Contains("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                var sign = str[0] == '-' ? -1 : 1;
                if (str[0] == '+' || str[0] == '-')
                    str = str[3..]; // -0x
                else
                    str = str[2..]; // 0x

                if (
                    int.TryParse(
                        str,
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture,
                        out var hex
                    )
                )
                    return hex * sign;
                return 0;
            }

            // Exponential or Decimal number
            if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var flt))
                return flt;
            return 0;
        }
    }
}
