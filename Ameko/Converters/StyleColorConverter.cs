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
        if (parameter is not bool opacity)
            opacity = false;

        Color output;
        if (opacity)
            output = new Color(
                (byte)(255 - input.Alpha),
                (byte)(255 - input.Red),
                (byte)(255 - input.Green),
                (byte)(255 - input.Blue)
            );
        else
            output = new Color(255, input.Red, input.Green, input.Blue);

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
