// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Windows;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.Plugins;
using Holo.Scripting;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using SkiaSharp;
using MainWindow = Ameko.Views.Windows.MainWindow;

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

        // Activate the culture and theme services
        _ = provider.GetRequiredService<CultureService>();
        _ = provider.GetRequiredService<ThemeService>();

        // Set up the tab item template
        Resources["WorkspaceTabTemplate"] = new WorkspaceTabTemplate(provider);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>(),
            };
        }
        // TODO: TESTING AREA

        var mizuki = new MizukiSource();
        mizuki.Initialize();
        var loaded = mizuki.LoadVideo(
            @"C:\Users\ame\Projects\Anime\9volt\Tsurezure Children\LazyMux\[LazyMux] Tsuredure Children - 01 (BD 1080p Main10p FLAC) [A25A8443].mkv"
        );
        var allocated = mizuki.AllocateFrame();
        var gotten = mizuki.GetFrame(25, out var frame);

        int size = frame.Pitch * frame.Height;
        byte[] managedData = new byte[size];
        Marshal.Copy(frame.FrameData, managedData, 0, size);

        unsafe
        {
            var info = new SKImageInfo(frame.Width, frame.Height, SKColorType.Bgra8888);
            using var bitmap = new SKBitmap();
            var success = bitmap.InstallPixels(info, frame.FrameData, frame.Pitch, null, null);

            using var image = SKImage.FromBitmap(bitmap);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
            var result = Convert.ToBase64String(encoded.ToArray());
            ;
        }

        base.OnFrameworkInitializationCompleted();

        // Start long process loading in the background after GUI finishes loading
        Dispatcher.UIThread.Post(() =>
        {
            InitializeKeybindService(provider);
            InitializeScriptService(provider);
            InitializeDependencyControl(provider);
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
        _ = provider.GetRequiredService<KeybindService>();
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
    /// Initialize <see cref="DependencyControl"/>
    /// </summary>
    /// <param name="provider">Service Provider</param>
    private static void InitializeDependencyControl(IServiceProvider provider)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var depCtrl = provider.GetRequiredService<IDependencyControl>();
                await depCtrl.SetUpBaseRepository();
                await depCtrl.AddAdditionalRepositories(
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
