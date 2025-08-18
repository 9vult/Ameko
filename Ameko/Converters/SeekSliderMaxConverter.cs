// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ameko.Converters;

/// <summary>
/// Converter for enums
/// </summary>
public class SeekSliderMaxConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int val)
            return null;

        return val - 1;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is not int val)
            return null;

        return val + 1;
    }
}
