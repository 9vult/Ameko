// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Ameko.Services;
using Ameko.ViewModels.Controls;
using Avalonia.Data.Converters;
using Holo;

namespace Ameko.Converters;

public class WorkspaceViewModelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Workspace wsp)
            return null;

        if (WorkspaceViewModelService.TryGetValue(wsp.Id, out var vm))
            return vm;

        vm = new TabItemViewModel(wsp);
        WorkspaceViewModelService.Register(wsp.Id, vm);
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
