// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Holo.Scripting.Models;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Ameko.Converters;

public class DepCtrlIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ModuleType type)
            return null;
        return type switch
        {
            ModuleType.Script => new MaterialIcon { Kind = MaterialIconKind.CodeBlockBraces },
            ModuleType.Library => new MaterialIcon { Kind = MaterialIconKind.Bookshelf },
            _ => null,
        };
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
