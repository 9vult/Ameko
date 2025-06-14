// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Holo.Models;

namespace Ameko.Converters;

/// <summary>
/// Converter for the flags enum <see cref="PasteOverField"/>
/// </summary>
public class PasteOverFieldConverter : IValueConverter
{
    private PasteOverField? _target;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null || value is null)
            return null;

        var mask = (PasteOverField)parameter;
        _target = (PasteOverField)value;
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
        _target ^= (PasteOverField)parameter;
        return _target;
    }
}
