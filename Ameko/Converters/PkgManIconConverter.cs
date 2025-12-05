// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Holo.Scripting.Models;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Ameko.Converters;

public class PkgManIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PackageType type)
            return null;
        return type switch
        {
            PackageType.Script => new MaterialIcon { Kind = MaterialIconKind.CodeBlockBraces },
            PackageType.Library => new MaterialIcon { Kind = MaterialIconKind.Bookshelf },
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
