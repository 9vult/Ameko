// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.Services;
using Ameko.ViewModels.Controls;
using Ameko.Views.Windows;
using AssCS;
using AssCS.IO;
using Holo;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Create a new file
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateNewCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var wsp = Solution.AddWorkspace();
            Solution.WorkingSpace = wsp;
        });
    }

    /// <summary>
    /// Display the Open Subtitle dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var uris = await OpenSubtitle.Handle(Unit.Default);

            Workspace? latest = null;

            foreach (var uri in uris)
            {
                var doc = Path.GetExtension(uri.LocalPath) switch
                {
                    ".ass" => new AssParser().Parse(uri.LocalPath),
                    ".txt" => new TxtParser().Parse(uri.LocalPath),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                latest = Solution.AddWorkspace(doc, uri);
                latest.IsSaved = true;
            }

            if (latest is not null)
                Solution.WorkingSpace = latest;
        });
    }

    /// <summary>
    /// Display either the Save Subtitle or Save Subtitle As dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var wsp = Solution.WorkingSpace;
            var uri = wsp.SavePath ?? await SaveSubtitleAs.Handle(wsp.Title);

            if (uri is null)
                return;

            var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
            writer.Write(uri.LocalPath);
            wsp.SavePath = uri;
            wsp.IsSaved = true;
        });
    }

    /// <summary>
    /// Display the Save Subtitle As dialog
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSaveSubtitleAsCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var wsp = Solution.WorkingSpace;
            var uri = await SaveSubtitleAs.Handle(wsp.Title);

            if (uri is null)
                return;

            var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
            writer.Write(uri.LocalPath);
            wsp.SavePath = uri;
            wsp.IsSaved = true;
        });
    }

    /// <summary>
    /// Display the Save As dialog for exporting
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateExportSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var wsp = Solution.WorkingSpace;
            var uri = await ExportSubtitle.Handle(wsp.Title);

            if (uri is null)
                return;

            var writer = new TxtWriter(wsp.Document, ConsumerService.AmekoInfo);
            writer.Write(uri.LocalPath, true);
        });
    }

    /// <summary>
    /// Display the <see cref="LogWindow"/>
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShowLogWindowCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new LogWindowViewModel();
            await ShowLogWindow.Handle(vm);
        });
    }

    /// <summary>
    /// Display the <see cref="AboutWindow"/>
    /// </summary>
    /// <returns></returns>
    private ReactiveCommand<Unit, Unit> CreateShowAboutWindowCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new AboutWindowViewModel();
            await ShowAboutWindow.Handle(vm);
        });
    }
}
