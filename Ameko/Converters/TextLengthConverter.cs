// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ameko.Converters;

/// <summary>
/// Converter limiting text length in the grid
/// </summary>
public class TextLengthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text)
            return null;
        return text.Length <= 32 ? text : text[..32];
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
