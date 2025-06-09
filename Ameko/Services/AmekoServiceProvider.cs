// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Threading.Tasks;
using Ameko.Providers;
using Ameko.ViewModels.Windows;
using Holo;
using Holo.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ameko.Services;

public static class AmekoServiceProvider
{
    public static IServiceProvider? Provider { get; private set; }

    public static IServiceProvider Build()
    {
        var services = new ServiceCollection();

        // Register primary Holo services
        services.AddSingleton<IHoloContext, HoloContext>();
        services.AddSingleton<ILogProvider, LogProvider>();

        // Register secondary Holo services
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().Configuration);
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().Globals);
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().DependencyControl);
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().SolutionProvider);
        services.AddSingleton(p => p.GetRequiredService<ILogProvider>().LogEntries);

        // Register additional services and providers
        services.AddSingleton<CultureService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<ScriptService>();
        services.AddSingleton<IoService>();

        services.AddSingleton<TabProvider>();
        services.AddSingleton<StylesManagerViewModelProvider>();

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<LogWindowViewModel>();
        services.AddTransient<DepCtrlWindowViewModel>();

        Provider = services.BuildServiceProvider();

        // Load the logger service immediately
        _ = Provider.GetRequiredService<ILogProvider>();

        return Provider;
    }
}
