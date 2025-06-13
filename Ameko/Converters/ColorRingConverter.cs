// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Ameko.Converters;

/// <summary>
/// Converter for displaying a color ring instead of a color box
/// </summary>
public class ColorRingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool useRing)
            return false;

        return useRing ? ColorSpectrumShape.Ring : ColorSpectrumShape.Box;
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
