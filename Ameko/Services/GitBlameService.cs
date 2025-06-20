// SPDX-License-Identifier: GPL-3.0-only

using System.Timers;
using Avalonia.Threading;
using Holo.Providers;

namespace Ameko.Services;

public class GitBlameService
{
    private readonly ISolutionProvider _solutionProvider;
    private readonly IGitService _gitService;
    private readonly Timer _timer;

    public GitBlameService(ISolutionProvider solutionProvider, IGitService gitService)
    {
        _solutionProvider = solutionProvider;
        _gitService = gitService;
        _timer = new Timer(20 * 1000) { AutoReset = true, Enabled = true };
        _timer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        var wsp = _solutionProvider.Current.WorkingSpace;

        if (wsp?.SavePath is null)
            return;

        if (!_gitService.IsRepository())
            return;

        var blames = _gitService.Blame(wsp.SavePath);

        // Do this on the UI thread
        Dispatcher.UIThread.Post(() => wsp.Blames = blames);
    }
}
