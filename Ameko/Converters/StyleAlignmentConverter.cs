// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ameko.Converters;

/// <summary>
/// Converter for the <see cref="AssCS.Style.Alignment"/> property
/// </summary>
public class StyleAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int input || parameter is not int param)
            return null;

        return input == param;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return null;
    }
}
