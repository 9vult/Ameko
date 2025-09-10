// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Ameko.Converters;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Windows;
using Ameko.Views.Windows;
using AssCS.Utilities;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.Providers;
using Holo.Scripting;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Ameko;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var provider = AmekoServiceProvider.Build();

        // Activate some key services
        _ = provider.GetRequiredService<CultureService>();
        _ = provider.GetRequiredService<ThemeService>();
        // May have to move this if it gets too resource-intensive
        provider.GetRequiredService<ILayoutProvider>().Reload();

        // Queue up welcome message before other services start adding messages
        provider
            .GetRequiredService<IMessageService>()
            .Enqueue(I18N.Resources.Message_Welcome, TimeSpan.FromSeconds(7));

        // Set up the tab item template
        Resources["WorkspaceTabTemplate"] = new WorkspaceTabTemplate(provider);

        // Set up the CPS warn converter
        Resources["CpsWarnConverter"] = new CpsWarnConverter(
            provider.GetRequiredService<IProjectProvider>(),
            provider.GetRequiredService<IConfiguration>()
        );

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            if (Program.Args.Length == 2 && Program.Args[0] == "--display-crash-report")
            {
                var vm = new CrashReporterWindowViewModel(
                    StringEncoder.Base64Decode(Program.Args[1])
                );
                desktop.MainWindow = new CrashReporterWindow { DataContext = vm };
            }
            else // Normal operation
            {
                var vm = provider.GetRequiredService<MainWindowViewModel>();

                // Check if there's anything to open
                // TODO: Probably should defer the actual opening part until after the GUI loads?
                if (Program.Args.Length > 0)
                {
                    var fs = provider.GetRequiredService<IFileSystem>();
                    List<Uri> subs = [];
                    List<Uri> projects = [];
                    foreach (var arg in Program.Args)
                    {
                        var path = arg;
                        if (!Path.IsPathRooted(path)) // De-relative any relative paths
                        {
                            path = Path.Combine(fs.Directory.GetCurrentDirectory(), arg);
                        }
                        if (fs.File.Exists(path))
                        {
                            switch (Path.GetExtension(path))
                            {
                                case ".ass":
                                case ".srt":
                                    subs.Add(new Uri(path));
                                    break;
                                case ".aproj":
                                    projects.Add(new Uri(path));
                                    break;
                                default:
                                    continue;
                            }
                        }
                    }

                    if (projects.Count > 0)
                    {
                        vm.OpenProjectNoGuiCommand.Execute(projects.First());
                    }
                    else
                    {
                        foreach (var uri in subs)
                            vm.OpenSubtitleNoGuiCommand.Execute(uri);
                    }
                }

                desktop.MainWindow = provider.GetRequiredService<MainWindow>();
                DataContext = vm;
                desktop.MainWindow.DataContext = vm;
            }
        }

        base.OnFrameworkInitializationCompleted();

        // Start long process loading in the background after GUI finishes loading
        Dispatcher.UIThread.Post(() =>
        {
            InitializeKeybindService(provider);
            InitializeScriptService(provider);
            InitializePackageManager(provider);
            InitializeDiscordRpcService(provider);
        });
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    /// <summary>
    /// Initialize the <see cref="KeybindService"/> (and <see cref="KeybindRegistrar"/>)
    /// </summary>
    /// <param name="provider"></param>
    private static void InitializeKeybindService(IServiceProvider provider)
    {
        // Commence keybind registration
        _ = provider.GetRequiredService<IKeybindService>();
    }

    /// <summary>
    /// Initialize the Discord rich presence service
    /// </summary>
    /// <param name="provider"></param>
    private static void InitializeDiscordRpcService(IServiceProvider provider)
    {
        _ = provider.GetRequiredService<DiscordRpcService>();
    }

    /// <summary>
    /// Initialize the <see cref="ScriptService"/>
    /// </summary>
    /// <param name="provider">Service Provider</param>
    private static void InitializeScriptService(IServiceProvider provider)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await provider.GetRequiredService<IScriptService>().Reload(isManual: false);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "ScriptService failed to initialize.");
            }
        });
    }

    /// <summary>
    /// Initialize <see cref="PackageManager"/>
    /// </summary>
    /// <param name="provider">Service Provider</param>
    private static void InitializePackageManager(IServiceProvider provider)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var pkgMan = provider.GetRequiredService<IPackageManager>();
                await pkgMan.SetUpBaseRepository();
                await pkgMan.AddAdditionalRepositories(
                    provider.GetRequiredService<IConfiguration>().RepositoryUrls
                );
            }
            catch (Exception ex)
            {
                LogManager
                    .GetCurrentClassLogger()
                    .Error(ex, "Dependency Control failed to initialize.");
            }
        });
    }
}
