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

            var data = str.AsSpan();
            data = data.Trim(); // Trim whitespace

            // Truncate multiple points
            if (data.Count('.') > 1)
            {
                var first = data.IndexOf('.') + 1;
                var second = data[first..].IndexOf('.') + first;
                data = data[..second];
            }

            // Truncate at first invalid char
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] is >= '0' and <= '9' or '.' or 'e' or '+' or '-')
                    continue;
                if (i > 0 && data[i] is 'x' && data[i - 1] is '0')
                    continue;

                data = data[..i];
                break;
            }

            if (data.IsEmpty)
                return 0;

            // Hex number
            if (data.Contains("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                var sign = data[0] == '-' ? -1 : 1;
                if (data[0] == '+' || data[0] == '-')
                    data = data[3..]; // -0x
                else
                    data = data[2..]; // 0x

                if (
                    int.TryParse(
                        data,
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture,
                        out var hex
                    )
                )
                    return hex * sign;
                return 0;
            }

            // Exponential or Decimal number
            if (
                double.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out var flt)
            )
                return flt;
            return 0;
        }
    }
}
