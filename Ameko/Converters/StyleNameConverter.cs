// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Ameko.Services;
using AssCS;
using Avalonia.Data.Converters;
using Holo.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ameko.Converters;

public class StyleNameConverter : IValueConverter
{
    private static readonly ISolutionProvider? SolutionProvider =
        AmekoServiceProvider.Provider?.GetRequiredService<ISolutionProvider>();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string name)
            return null;
        if (
            SolutionProvider?.Current.WorkingSpace?.Document.StyleManager.TryGet(
                name,
                out var style
            ) ?? false
        )
            return style;
        return null;
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
