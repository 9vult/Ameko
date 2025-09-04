// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Ameko.Converters;

/// <summary>
/// Converter for warning if CPS goes above the threshold
/// </summary>
public class CpsWarnConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        const int threshold = 18; // TODO: get this from the config/project
        if (value is not double cps)
            return null;

        if (cps <= threshold)
            return Brushes.Transparent;

        const int maxSteps = 5;
        var steps = Math.Min(maxSteps, Math.Max(0, Math.Ceiling((cps - threshold) / 2.0)) + 1);
        var red = (100 - (20 * maxSteps) + (steps * 20)) / 100;

        return new SolidColorBrush(Colors.Red, red);
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
