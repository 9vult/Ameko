// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ameko.Utilities;
using AssCS.IO;
using Holo;
using Holo.Models;
using Holo.Providers;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;
using ReactiveUI;

namespace Ameko.Services;

public class IoService(
    IProjectProvider projectProvider,
    ITabFactory tabFactory,
    IFileSystem fileSystem
) : IIoService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <inheritdoc />
    public async Task<bool> SaveSubtitle(Interaction<string, Uri?> interaction, Workspace wsp)
    {
        Log.Info($"Preparing to save subtitle file {wsp.Title}");
        var uriChanged = wsp.SavePath is null;
        var uri = wsp.SavePath ?? await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        writer.Write(fileSystem, uri);
        Log.Info($"Saved subtitle file {wsp.Title}");

        if (uriChanged)
        {
            var projItem = projectProvider.Current.FindItemById(wsp.Id);
            if (projItem is not null)
                projectProvider.Current.SetNameAndUri(projItem, wsp.Title, uri);
        }
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SaveSubtitleAs(Interaction<string, Uri?> interaction, Workspace wsp)
    {
        Log.Info($"Preparing to save subtitle file {wsp.Title}");
        var isForking = wsp.SavePath is not null;
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        writer.Write(fileSystem, uri);
        Log.Info($"Saved subtitle file {wsp.Title}");

        var projItem = projectProvider.Current.FindItemById(wsp.Id);
        // The user is forking the file into a second file
        if (isForking)
        {
            if (projItem is DocumentItem current)
            {
                var original = projectProvider.Current.Copy(current);
                original.Workspace = null;
            }
        }

        if (projItem is not null)
            projectProvider.Current.SetNameAndUri(projItem, wsp.Title, uri);

        return true;
    }

    /// <inheritdoc />
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
        writer.Write(fileSystem, uri, true);
        Log.Info($"Exported {wsp.Title}");
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SafeCloseWorkspace(
        Workspace wsp,
        Interaction<string, Uri?> saveAs,
        bool replaceIfLast = true
    )
    {
        Log.Trace($"Closing workspace {wsp.Title}");
        var prj = projectProvider.Current;

        if (wsp.IsSaved)
        {
            prj.CloseDocument(wsp.Id, replaceIfLast);
            tabFactory.Release(wsp);
            return true;
        }

        Log.Trace($"Displaying message box because workspace {wsp.Title} is not saved");

        var box = MessageBoxManager.GetMessageBoxStandard(
            title: I18N.Other.MsgBox_Save_Title,
            text: string.Format(I18N.Other.MsgBox_Save_Body, wsp.Title),
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
                prj.CloseDocument(wsp.Id, replaceIfLast);
                tabFactory.Release(wsp);
                return true;
            default:
                return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveProject(Interaction<string, Uri?> interaction, Project prj)
    {
        Log.Info($"Preparing to save project file {prj.Title}");
        var uri = prj.SavePath ?? await interaction.Handle(prj.Title);

        if (uri is null)
        {
            Log.Info("Save operation cancelled");
            return false;
        }

        prj.SavePath = uri;
        prj.Save();

        Log.Info($"Saved project file {prj.Title}");
        return true;
    }
}
