// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using AssCS;
using Avalonia.Data.Converters;
using Holo;

namespace Ameko.Converters;

public class StyleNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string name
            ? null
            : HoloContext.Instance.Solution.WorkingSpace?.Document.StyleManager.Get(name);
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return (value as Style)?.Name;
    }
}
