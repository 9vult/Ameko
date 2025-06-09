// SPDX-License-Identifier: GPL-3.0-only

using System;
using Ameko.ViewModels.Windows;
using Holo;
using Holo.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ameko.Services;

public static class HoloServiceProvider
{
    public static IServiceProvider Build()
    {
        var services = new ServiceCollection();

        // Register primary services
        services.AddSingleton<IHoloContext, HoloContext>();
        services.AddSingleton<ILogProvider, LogProvider>();

        // Register secondary services
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().Configuration);
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().Globals);
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().DependencyControl);
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().SolutionProvider);
        services.AddSingleton(p => p.GetRequiredService<ILogProvider>().LogEntries);

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<LogWindowViewModel>();

        var provider = services.BuildServiceProvider();

        // Load the logger service immediately
        _ = provider.GetRequiredService<ILogProvider>();

        return provider;
    }
}
