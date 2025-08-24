// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Ameko.Converters;

/// <summary>
/// Converter for generating padding in the events grid
/// </summary>
public class EventTextLengthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text)
            return null;
        return text.Length <= 256 ? text : text[..256];
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
