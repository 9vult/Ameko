// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Utilities;

public static class EnumExtensions
{
    /// <summary>
    /// Enumerate over the set flags in a [Flags] enum
    /// </summary>
    /// <param name="e">Enum instance</param>
    /// <returns>Set flags</returns>
    public static IEnumerable<Enum> GetSetFlags(this Enum e)
    {
        return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
    }
}
