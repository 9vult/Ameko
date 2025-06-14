// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using AssCS;
using Avalonia.Data.Converters;
using Holo.Models;

namespace Ameko.Converters;

/// <summary>
/// Converter for the flags enum <see cref="EventField"/>
/// </summary>
public class EventFieldConverter : IValueConverter
{
    private EventField? _target;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null || value is null)
            return null;

        var mask = (EventField)parameter;
        _target = (EventField)value;
        return (mask & _target) != 0;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (parameter is null)
            return null;
        _target ^= (EventField)parameter;
        return _target;
    }
}
