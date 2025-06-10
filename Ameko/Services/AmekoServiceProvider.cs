// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Threading.Tasks;
using Ameko.Factories;
using Ameko.ViewModels.Windows;
using Holo;
using Holo.Providers;
using Holo.Scripting;
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
        services.AddSingleton(p => p.GetRequiredService<IHoloContext>().SolutionProvider);
        services.AddSingleton(p => p.GetRequiredService<ILogProvider>().LogEntries);

        // Register additional services
        services.AddSingleton<CultureService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<IIoService, IoService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();

        services.AddSingleton<IScriptService, ScriptService>();
        services.AddSingleton<IDependencyControl, DependencyControl>();

        // Register factories
        services.AddSingleton<ITabFactory, TabFactory>();
        services.AddSingleton<IStylesManagerFactory, StylesManagerFactory>();

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
