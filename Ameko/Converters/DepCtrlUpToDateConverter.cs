// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Ameko.Services;
using Avalonia.Data.Converters;
using Holo;
using Holo.Scripting;
using Holo.Scripting.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Ameko.Converters;

public class DepCtrlUpToDateConverter : IValueConverter
{
    /// <summary>
    /// Returns <see langword="true"/> if the module is NOT up to date
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Module module)
            return false;
        return !AmekoServiceProvider
                .Provider?.GetRequiredService<DependencyControl>()
                .IsModuleUpToDate(module) ?? true;
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
