// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using AssCS;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Ameko.Converters;

/// <summary>
/// Converter for converting null UpDown values to default
/// </summary>
public class NumericUpDownConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value ?? 0;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return value ?? 0;
    }
}
