// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Ameko.Services;
using AssCS.Utilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Holo.IO;
using Holo.Providers;
using Microsoft.Extensions.Logging;
using NLog;
using ReactiveUI.Avalonia;
#if !DEBUG
using System.Threading.Tasks;
using System.Reactive;
using ReactiveUI;
#endif

namespace Ameko;

sealed class Program
{
    internal static string[] Args { get; private set; } = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Args = args;

#if !DEBUG
        // Handle non-UI-thread exceptions
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
            HandleUnhandledException("Non-UI", (Exception)ex.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, ex) =>
            HandleUnhandledException("Task", ex.Exception);

        // Avoid everything being wrapped with ReactiveUI.UnhandledErrorException
        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
        {
            HandleUnhandledException(
                "UI",
                ex is UnhandledErrorException { InnerException: { } inner } ? inner : ex
            );
        });
#endif
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
        }
#if !DEBUG
        catch (Exception ex)
        {
            HandleUnhandledException("Application", ex);
            throw;
        }
#else
        catch
        {
            throw;
        }
#endif
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI()
            .With(new MacOSPlatformOptions { DisableDefaultApplicationMenuItems = true });

    private static void HandleUnhandledException(string category, Exception ex)
    {
        try
        {
            // Write log and hope for the best
            var logger = StaticLoggerFactory.GetLogger<Program>();
            logger.LogCritical(ex, "Unhandled exception");

            // Write crash report
            var wittyComments = new StreamReader(
                AssetLoader.Open(new Uri("avares://Ameko/Assets/Text/WittyComments.txt"))
            )
                .ReadToEnd()
                .Split(Environment.NewLine);
            var wittyComment = wittyComments[new Random().Next(wittyComments.Length)];
            var time = DateTime.UtcNow.ToString("o");

            var report = new StringBuilder();
            report.AppendLine("----- Ameko Crash Report -----");
            report.AppendLine($"// {wittyComment}");
            report.AppendLine(string.Empty);
            report.AppendLine($"Time: {time}");
            report.AppendLine($"Version: Ameko {VersionService.FullLabel}");
            report.AppendLine($"OS: {RuntimeInformation.OSDescription}");
            report.AppendLine($"Platform: {SystemService.Platform}");
            report.AppendLine($"Platform Architecture: {RuntimeInformation.OSArchitecture}");
            report.AppendLine($"Desktop Environment: {SystemService.DesktopEnvironment}");
            report.AppendLine($"Display Server: {SystemService.WindowManager}");
            report.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");
            report.AppendLine($"Category: {category}");
            report.AppendLine(string.Empty);
            report.AppendLine(ex.ToString());

            // Try to write the report to disk
            try
            {
                var dir = Path.Combine(Directories.DataHome, "crash-reports");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using var fs = new FileStream(
                    Path.Combine(dir, $"crash-{time.Replace(":", ".")}.log"),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None
                );
                using var writer = new StreamWriter(fs);
                writer.Write(report.ToString());
                writer.Flush();
            }
            catch (IOException) { } // Ignore, what are we going to do, throw up another error box? XD

            var crashArgs =
                $"--display-crash-report \"{StringEncoder.Base64Encode(report.ToString())}\"";

            // Restart, passing the report as an arg
            if (File.Exists(Environment.ProcessPath))
            {
                Process.Start(
                    new ProcessStartInfo(Environment.ProcessPath, crashArgs)
                    {
                        UseShellExecute = true,
                    }
                );
            }
        }
        finally
        {
            Environment.Exit(-1);
        }
    }
}
