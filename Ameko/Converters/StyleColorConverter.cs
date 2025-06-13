// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Color = Avalonia.Media.Color;

namespace Ameko.Converters;

/// <summary>
/// Converter for <see cref="AssCS.Style"/> <see cref="AssCS.Color"/> properties
/// </summary>
public class StyleColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AssCS.Color input)
            return null;
        var opacity = System.Convert.ToBoolean(parameter);

        var output = opacity
            ? new Color((byte)(255 - input.Alpha), input.Red, input.Green, input.Blue)
            : new Color(255, input.Red, input.Green, input.Blue);

        return new SolidColorBrush(output);
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
