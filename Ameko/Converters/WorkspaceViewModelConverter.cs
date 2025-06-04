// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Globalization;
using Ameko.ViewModels.Controls;
using Avalonia.Data.Converters;
using Holo;

namespace Ameko.Converters;

public class WorkspaceViewModelConverter : IValueConverter
{
    private static readonly Dictionary<int, TabItemViewModel> VMs = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Workspace wsp)
            return null;

        if (VMs.TryGetValue(wsp.Id, out var vm))
            return vm;

        vm = new TabItemViewModel(wsp);
        VMs.Add(wsp.Id, vm);
        return vm;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return (value as TabItemViewModel)?.Workspace;
    }
}
