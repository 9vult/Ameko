// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO.Abstractions;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Windows;
using Holo.Configuration;
using Holo.IO;
using Holo.Providers;
using Holo.Scripting;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Ameko;

public static class AmekoServiceProvider
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public static IServiceProvider? Provider { get; private set; }

    public static IServiceProvider Build()
    {
        var services = new ServiceCollection();

        // --- Infrastructure ---
        services.AddHttpClient();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<ILogProvider, LogProvider>();
        services.AddSingleton(p => p.GetRequiredService<ILogProvider>().LogEntries); // Log entries are part of logging infrastructure

        // --- Configuration ---
        services.AddSingleton<IConfiguration, Configuration>(p =>
            Configuration.Parse(p.GetRequiredService<IFileSystem>())
        );
        services.AddSingleton<IGlobals, Globals>(p =>
            Globals.Parse(p.GetRequiredService<IFileSystem>())
        );

        // --- Application Services ---
        // Core business logic and application-specific operations
        services.AddSingleton<DirectoryService>();
        services.AddSingleton<ISolutionProvider, SolutionProvider>();
        services.AddSingleton<IScriptService, ScriptService>();
        services.AddSingleton<IDependencyControl, DependencyControl>();

        // --- Presentation Services ---
        services.AddSingleton<CultureService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<IIoService, IoService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();

        // --- Factories ---
        services.AddSingleton<ITabFactory, TabFactory>();
        services.AddSingleton<IStylesManagerFactory, StylesManagerFactory>();

        // --- ViewModels ---
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<LogWindowViewModel>();
        services.AddTransient<DepCtrlWindowViewModel>();

        Provider = services.BuildServiceProvider();

        // Load the logger service immediately
        _ = Provider.GetRequiredService<ILogProvider>();

        Logger.Info("Ameko and Holo are ready to go!");

        return Provider;
    }
}
