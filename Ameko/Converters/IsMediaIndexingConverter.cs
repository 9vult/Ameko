// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ameko.Converters;

public class IsMediaIndexingConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (values.Count is < 2 or > 3)
            return false;
        {
            if (values is [bool isMediaIndexing, bool isMediaLoaded])
            {
                return isMediaIndexing && !isMediaLoaded;
            }
        }
        {
            if (values is [bool isMediaIndexing, bool isMediaLoaded, bool isPrereqLoaded])
            {
                return isPrereqLoaded && isMediaIndexing && !isMediaLoaded;
            }
        }

        return false;
    }
}
