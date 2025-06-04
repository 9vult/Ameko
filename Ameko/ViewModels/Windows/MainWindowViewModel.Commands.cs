// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Reactive.Linq;
using Ameko.ViewModels.Controls;
using Ameko.Views.Windows;
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
    /// Create a new file
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateOpenSubtitleCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var uris = await OpenSubtitle.Handle(Unit.Default);

            Workspace? latest = null;

            foreach (var uri in uris)
            {
                var doc = new AssParser().Parse(uri.LocalPath);
                latest = Solution.AddWorkspace(doc, uri);
            }

            if (latest is not null)
                Solution.WorkingSpace = latest;
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
