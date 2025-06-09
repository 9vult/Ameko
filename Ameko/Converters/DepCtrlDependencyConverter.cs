// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Holo.Scripting.Models;

namespace Ameko.Converters;

public class DepCtrlDependencyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Module module
            ? string.Join(Environment.NewLine, module.Dependencies)
            : I18N.Resources.DepCtrl_Info_NoSelection;
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
