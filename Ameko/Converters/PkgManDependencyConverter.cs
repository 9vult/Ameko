// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Holo.Scripting.Models;

namespace Ameko.Converters;

public class PkgManDependencyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Module module
            ? string.Join(Environment.NewLine, module.Dependencies)
            : I18N.PkgMan.PkgMan_Info_NoSelection;
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
