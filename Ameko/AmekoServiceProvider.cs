// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO.Abstractions;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Controls;
using Ameko.ViewModels.Dialogs;
using Ameko.ViewModels.Windows;
using Ameko.Views.Windows;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.IO;
using Holo.Providers;
using Holo.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Ameko;

public class AmekoServiceProvider
{
    public static IServiceProvider? Provider { get; private set; }

    public static IServiceProvider Build()
    {
        var services = new ServiceCollection();

        // --- Infrastructure ---
        services.AddHttpClient("default");
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<ILogProvider, LogProvider>();
        services.AddLogging(p =>
        {
            p.ClearProviders();
            p.SetMinimumLevel(LogLevel.Debug);
            p.AddNLog();

            // Filter out HttpClient debug logs
            p.AddFilter("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogLevel.Warning);
        });

        // --- Configuration ---
        services.AddSingleton<IConfiguration, Configuration>(p =>
            Configuration.Parse(
                p.GetRequiredService<IFileSystem>(),
                p.GetRequiredService<ILogger<Configuration>>()
            )
        );
        services.AddSingleton<IPersistence, Persistence>(p =>
            Persistence.Parse(
                p.GetRequiredService<IFileSystem>(),
                p.GetRequiredService<ILogger<Persistence>>()
            )
        );
        services.AddSingleton<IGlobals, Globals>(p =>
            Globals.Parse(
                p.GetRequiredService<IFileSystem>(),
                p.GetRequiredService<ILogger<Globals>>()
            )
        );
        services.AddSingleton<ICommandRegistrar, CommandRegistrar>();
        services.AddSingleton<IKeybindRegistrar, KeybindRegistrar>();
        services.AddSingleton<IKeybindService, KeybindService>();
        services.AddSingleton<AssOptionsBinder>();

        // --- Application Services ---
        services.AddSingleton<Directories>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<AutosaveService>();
        services.AddSingleton<DiscordRpcService>();
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<ILayoutProvider, LayoutProvider>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<IProjectProvider, ProjectProvider>();
        services.AddSingleton<IDictionaryService, DictionaryService>();
        services.AddSingleton<ISpellcheckService, SpellcheckService>();

        // --- Presentation ---
        services.AddSingleton<ThemeService>();
        services.AddSingleton<IIoService, IoService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<ICultureService, CultureService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();

        // --- Factories ---
        services.AddSingleton<StaticLoggerFactory>();
        services.AddSingleton<ITabFactory, TabFactory>();
        services.AddSingleton<IStylesManagerFactory, StylesManagerFactory>();

        // --- ViewModels ---
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<GitToolboxViewModel>();
        services.AddTransient<LogWindowViewModel>();
        services.AddTransient<HelpWindowViewModel>();
        services.AddTransient<PkgManWindowViewModel>();
        services.AddTransient<KeybindsDialogViewModel>();
        services.AddTransient<PlaygroundWindowViewModel>();

        // --- Scripting ---
        services.AddSingleton<IScriptService, ScriptService>();
        services.AddSingleton<IPackageManager, PackageManager>();
        services.AddSingleton<IScriptConfigurationService, ScriptConfigurationService>();
        services.AddSingleton<ScriptServiceLocator>();

        // --- Main Window ---
        services.AddSingleton<MainWindow>();

        // Extra config
        services.ConfigureHttpClientDefaults(p => p.RemoveAllLoggers()); // disable HttpClient logging

        Provider = services.BuildServiceProvider();

        // Set up logging
        _ = Provider.GetRequiredService<ILogProvider>();
        _ = Provider.GetRequiredService<StaticLoggerFactory>();
        Provider
            .GetRequiredService<ILogger<AmekoServiceProvider>>()
            .LogInformation("Starting Ameko {Version}", VersionService.FullLabel);

        // Load in other key services
        _ = Provider.GetRequiredService<Directories>();
        _ = Provider.GetRequiredService<ScriptServiceLocator>();
        _ = Provider.GetRequiredService<AssOptionsBinder>();

        return Provider;
    }
}
