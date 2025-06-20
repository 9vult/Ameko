// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Holo;

namespace Ameko.Converters;

public class BlameInfoConverter : IMultiValueConverter
{
    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (
            values is [int line, Workspace { Blames: not null } workspace, not null]
            && workspace.Blames.TryGetValue(line, out var info)
        )
        {
            return info.Author;
        }

        return "You";
    }
}
