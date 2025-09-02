// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO.Abstractions;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Controls;
using Ameko.ViewModels.Windows;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.IO;
using Holo.Providers;
using Holo.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        services.AddSingleton(p => p.GetRequiredService<ILogProvider>().LogEntries);

        // --- Configuration ---
        services.AddSingleton<IConfiguration, Configuration>(p =>
            Configuration.Parse(p.GetRequiredService<IFileSystem>())
        );
        services.AddSingleton<IPersistence, Persistence>(p =>
            Persistence.Parse(p.GetRequiredService<IFileSystem>())
        );
        services.AddSingleton<IGlobals, Globals>(p =>
            Globals.Parse(p.GetRequiredService<IFileSystem>())
        );
        services.AddSingleton<IKeybindRegistrar, KeybindRegistrar>();
        services.AddSingleton<IKeybindService, KeybindService>();
        services.AddSingleton<AssOptionsBinder>();

        // --- Application Services ---
        services.AddSingleton<Directories>();
        services.AddSingleton<DiscordRpcService>();
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<ILayoutProvider, LayoutProvider>();
        services.AddSingleton<IProjectProvider, ProjectProvider>();
        services.AddSingleton<IDictionaryService, DictionaryService>();
        services.AddSingleton<ISpellcheckService, SpellcheckService>();

        // --- Presentation ---
        services.AddSingleton<CultureService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<IIoService, IoService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();

        // --- Factories ---
        services.AddSingleton<ITabFactory, TabFactory>();
        services.AddSingleton<IStylesManagerFactory, StylesManagerFactory>();

        // --- ViewModels ---
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<GitToolboxViewModel>();
        services.AddTransient<LogWindowViewModel>();
        services.AddTransient<PkgManWindowViewModel>();
        services.AddTransient<KeybindsWindowViewModel>();
        services.AddTransient<PlaygroundWindowViewModel>();

        // --- Scripting ---
        services.AddSingleton<IScriptService, ScriptService>();
        services.AddSingleton<IPackageManager, PackageManager>();
        services.AddSingleton<IScriptConfigurationService, ScriptConfigurationService>();
        services.AddSingleton<ScriptServiceLocator>();

        Provider = services.BuildServiceProvider();

        // Load the logger and locator services immediately
        _ = Provider.GetRequiredService<ILogProvider>();
        _ = Provider.GetRequiredService<ScriptServiceLocator>();
        _ = Provider.GetRequiredService<DiscordRpcService>();
        _ = Provider.GetRequiredService<AssOptionsBinder>();

        Logger.Info("Ameko and Holo are ready to go!");

        return Provider;
    }
}
