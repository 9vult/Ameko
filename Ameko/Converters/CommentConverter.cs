// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using AssCS;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Ameko.Converters;

/// <summary>
/// Converter for identifying comments in the grid
/// </summary>
public class CommentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not true)
            return Brushes.Transparent;

        return new SolidColorBrush(Colors.LightGray, 0.3);
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
