// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ameko.Providers;
using Ameko.Services.Interfaces;
using AssCS.IO;
using Holo;
using Holo.Providers;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;
using ReactiveUI;

namespace Ameko.Services;

public class IoService(ISolutionProvider solutionProvider, TabProvider tabProvider) : IIoService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <inheritdoc cref="IIoService.SaveSubtitle"/>
    public async Task<bool> SaveSubtitle(Interaction<string, Uri?> interaction, Workspace wsp)
    {
        Log.Info($"Preparing to save subtitle file {wsp.Title}");
        var uri = wsp.SavePath ?? await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        writer.Write(uri);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        Log.Info($"Saved subtitle file {wsp.Title}");
        return true;
    }

    /// <inheritdoc cref="IIoService.SaveSubtitleAs"/>
    public async Task<bool> SaveSubtitleAs(Interaction<string, Uri?> interaction, Workspace wsp)
    {
        Log.Info($"Preparing to save subtitle file {wsp.Title}");
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        writer.Write(uri);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        Log.Info($"Saved subtitle file {wsp.Title}");
        return true;
    }

    /// <inheritdoc cref="IIoService.ExportSubtitle"/>
    public async Task<bool> ExportSubtitle(Interaction<string, Uri?> interaction, Workspace wsp)
    {
        Log.Info($"Preparing to export subtitle file {wsp.Title}");
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Log.Info("Export operation cancelled");
            return false;
        }

        var writer = new TxtWriter(wsp.Document, ConsumerService.AmekoInfo);
        writer.Write(uri, true);
        Log.Info($"Exported {wsp.Title}");
        return true;
    }

    /// <inheritdoc cref="IIoService.SafeCloseWorkspace"/>
    public async Task<bool> SafeCloseWorkspace(
        Workspace wsp,
        Interaction<string, Uri?> saveAs,
        bool replaceIfLast = true
    )
    {
        Log.Trace($"Closing workspace {wsp.Title}");
        var sln = solutionProvider.Current;

        if (wsp.IsSaved)
        {
            sln.CloseDocument(wsp.Id, replaceIfLast);
            tabProvider.Release(wsp);
            return true;
        }

        Log.Trace($"Displaying message box because workspace {wsp.Title} is not saved");

        var box = MessageBoxManager.GetMessageBoxStandard(
            title: I18N.Resources.MsgBox_Save_Title,
            text: string.Format(I18N.Resources.MsgBox_Save_Body, wsp.Title),
            ButtonEnum.YesNoCancel
        );
        var boxResult = await box.ShowAsync();

        switch (boxResult)
        {
            case ButtonResult.Yes:
                var saved = await SaveSubtitle(saveAs, wsp);
                if (!saved)
                {
                    Log.Info("Tab close operation aborted");
                    return false;
                }
                goto case ButtonResult.No; // lol
            case ButtonResult.No:
                sln.CloseDocument(wsp.Id, replaceIfLast);
                tabProvider.Release(wsp);
                return true;
            default:
                return false;
        }
    }

    /// <inheritdoc cref="IIoService.SaveSolution"/>
    public async Task<bool> SaveSolution(Interaction<string, Uri?> interaction, Solution sln)
    {
        Log.Info($"Preparing to save solution file {sln.Title}");
        var uri = sln.SavePath ?? await interaction.Handle(sln.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        sln.SavePath = uri;
        sln.Save();

        Log.Info($"Saved solution file {sln.Title}");
        return true;
    }
}
