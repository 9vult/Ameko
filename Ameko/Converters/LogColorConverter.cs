// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Extensions.Logging;

namespace Ameko.Converters;

/// <summary>
/// Converter for identifying comments in the grid
/// </summary>
public class LogColorConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (values.Count != 2)
            return null;

        if (values[0] is not LogLevel level || values[1] is not Window window)
            return null;

        switch (level)
        {
            case LogLevel.Warning:
                return new SolidColorBrush(Colors.Yellow);
            case LogLevel.Error:
            case LogLevel.Critical:
                return new SolidColorBrush(Colors.Red);
            case LogLevel.Debug:
            case LogLevel.Trace:
                return new SolidColorBrush(Colors.LightGray);
            default:
                return window.Foreground;
        }
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
