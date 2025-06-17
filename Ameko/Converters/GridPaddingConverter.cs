// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Ameko.Converters;

/// <summary>
/// Converter for generating padding in the events grid
/// </summary>
public class GridPaddingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not uint verticalPadding)
            return false;

        const int horizontalPadding = 4;

        return new Thickness(horizontalPadding, verticalPadding);
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
