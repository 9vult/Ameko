// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ameko.Converters;

/// <summary>
/// Converter for the <see cref="AssCS.Style.BorderStyle"/> property
/// </summary>
public class StyleBorderBoxConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
            return null;

        return System.Convert.ToInt32(value) == System.Convert.ToInt32(parameter);
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return System.Convert.ToInt32(parameter);
    }
}
