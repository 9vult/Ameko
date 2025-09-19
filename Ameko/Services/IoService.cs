// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ameko.Utilities;
using AssCS.IO;
using Holo;
using Holo.Models;
using Holo.Providers;
using Material.Icons;
using NLog;
using ReactiveUI;

namespace Ameko.Services;

public class IoService(
    IProjectProvider projectProvider,
    ITabFactory tabFactory,
    IFileSystem fileSystem,
    IMessageBoxService messageBoxService,
    IMessageService messageService
) : IIoService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <inheritdoc />
    public async Task<bool> SaveSubtitle(Interaction<string, Uri?> interaction, Workspace wsp)
    {
        Logger.Debug($"Preparing to save subtitle file {wsp.Title}");
        var uriChanged = wsp.SavePath is null;
        var uri = wsp.SavePath ?? await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Logger.Info($"Saving {wsp.Title} cancelled");
            return false;
        }

        // Set the audio/video path, if applicable
        if (wsp.MediaController.IsVideoLoaded)
        {
            var dir = Path.GetDirectoryName(uri.LocalPath) ?? "/";
            var relPath = Path.GetRelativePath(dir, wsp.MediaController.VideoInfo.Path);
            wsp.Document.GarbageManager.Set("Video File", relPath);
            wsp.Document.GarbageManager.Set("Audio File", relPath);
            wsp.Document.GarbageManager.Set("Video Position", wsp.MediaController.CurrentFrame);
        }
        wsp.Document.GarbageManager.Set("Active Line", wsp.SelectionManager.ActiveEvent.Index - 1);

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        writer.Write(fileSystem, uri);
        Logger.Info($"Saved subtitle file {wsp.Title}");

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
        Logger.Debug($"Preparing to save subtitle file {wsp.Title}");
        var isForking = wsp.SavePath is not null;
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Logger.Info($"Saving {wsp.Title} cancelled");
            return false;
        }

        // Set the audio/video path, if applicable
        if (wsp.MediaController.IsVideoLoaded)
        {
            var dir = Path.GetDirectoryName(uri.LocalPath) ?? "/";
            var relPath = Path.GetRelativePath(dir, wsp.MediaController.VideoInfo.Path);
            wsp.Document.GarbageManager.Set("Video File", relPath);
            wsp.Document.GarbageManager.Set("Audio File", relPath);
            wsp.Document.GarbageManager.Set("Video Position", wsp.MediaController.CurrentFrame);
        }
        wsp.Document.GarbageManager.Set("Active Line", wsp.SelectionManager.ActiveEvent.Index - 1);

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        writer.Write(fileSystem, uri);
        Logger.Info($"Saved subtitle file {wsp.Title}");

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
        Logger.Debug($"Preparing to export subtitle file {wsp.Title}");
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            Logger.Info($"Exporting {wsp.Title} cancelled");
            return false;
        }

        var writer = new TxtWriter(wsp.Document, ConsumerService.AmekoInfo);
        writer.Write(fileSystem, uri, true);
        Logger.Info($"Exported {wsp.Title}");
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SafeCloseWorkspace(
        Workspace wsp,
        Interaction<string, Uri?> saveAs,
        bool replaceIfLast = true
    )
    {
        Logger.Debug($"Closing workspace {wsp.Title}");
        var prj = projectProvider.Current;

        if (wsp.IsSaved)
        {
            prj.CloseDocument(wsp.Id, replaceIfLast);
            tabFactory.Release(wsp);
            return true;
        }

        Logger.Trace($"Displaying message box because workspace {wsp.Title} is not saved");

        var boxResult = await messageBoxService.ShowAsync(
            I18N.Other.MsgBox_Save_Title,
            string.Format(I18N.Other.MsgBox_Save_Body, wsp.Title),
            MsgBoxButtonSet.YesNoCancel,
            MsgBoxButton.Yes,
            MaterialIconKind.QuestionMark
        );

        switch (boxResult)
        {
            case MsgBoxButton.Yes:
                var saved = await SaveSubtitle(saveAs, wsp);
                if (!saved)
                {
                    Logger.Info("Tab close operation aborted");
                    return false;
                }
                goto case MsgBoxButton.No; // lol
            case MsgBoxButton.No:
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
        Logger.Debug($"Preparing to save project file {prj.Title}");
        var uri = prj.SavePath ?? await interaction.Handle(prj.Title);

        if (uri is null)
        {
            Logger.Info($"Saving {prj.Title} cancelled");
            return false;
        }

        prj.SavePath = uri;
        prj.Save();

        Logger.Info($"Saved project file {prj.Title}");
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> OpenSubtitleFiles(Interaction<Unit, Uri[]?> interaction, Project prj)
    {
        var uris = await interaction.Handle(Unit.Default);
        if (uris is null || uris.Length == 0)
            return false;

        foreach (var uri in uris)
        {
            await OpenSubtitleFile(uri, prj);
        }

        return true;
    }

    /// <summary>
    /// Load a subtitle file into a Workspace
    /// </summary>
    /// <param name="uri">URI to the subtitle file</param>
    /// <param name="prj">Project to add the workspace to</param>
    /// <returns>The created workspace</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the format is invalid</exception>
    public async Task<bool> OpenSubtitleFile(Uri uri, Project prj)
    {
        Logger.Debug($"Opening subtitle file {uri}");

        try
        {
            var ext = Path.GetExtension(uri.LocalPath);
            var doc = ext switch
            {
                ".ass" => new AssParser().Parse(fileSystem, uri),
                ".srt" => new SrtParser().Parse(fileSystem, uri),
                ".txt" => new TxtParser().Parse(fileSystem, uri),
                _ => throw new ArgumentOutOfRangeException(nameof(uri)),
            };

            Workspace wsp;

            if (ext == ".ass")
            {
                wsp = prj.AddWorkspace(doc, uri);
                wsp.IsSaved = true;
                if (doc.GarbageManager.TryGetInt("Active Line", out var lineIdx))
                {
                    var line = doc.EventManager.Events.FirstOrDefault(e => e.Index == lineIdx + 1);
                    if (line is not null)
                        wsp.SelectionManager.Select(line);
                }

                await OpenVideoFile(wsp);
            }
            else
            {
                // Non-ass sourced documents need to be re-saved as an ass file
                wsp = prj.AddWorkspace(doc);
                wsp.IsSaved = false;
            }

            Logger.Info($"Opened subtitle file {wsp.Title}");
            prj.WorkingSpace = wsp;
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to parse file {uri.LocalPath}");
            Logger.Error(ex);
            await messageBoxService.ShowAsync(
                I18N.Resources.Error,
                $"{I18N.Resources.Error_FailedToParse}\n\n{ex.Message}",
                MsgBoxButtonSet.Ok,
                MsgBoxButton.Ok,
                MaterialIconKind.Error
            );
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> OpenProjectFile(Uri uri, Interaction<string, Uri?> saveAs)
    {
        Logger.Debug("Preparing to open project file");
        foreach (var wsp in projectProvider.Current.LoadedWorkspaces.ToArray())
        {
            await SafeCloseWorkspace(wsp, saveAs, false);
        }

        if (projectProvider.Current.LoadedWorkspaces.Count > 0)
        {
            Logger.Info(
                $"Opening project file aborted - {projectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
            );
            return false;
        }

        projectProvider.Current = Project.Parse(fileSystem, uri);
        Logger.Info("Loaded project file");
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> OpenProjectDirectory(Uri uri, Interaction<string, Uri?> saveAs)
    {
        Logger.Debug("Preparing to open project directory");
        foreach (var wsp in projectProvider.Current.LoadedWorkspaces.ToArray())
        {
            await SafeCloseWorkspace(wsp, saveAs, false);
        }

        if (projectProvider.Current.LoadedWorkspaces.Count > 0)
        {
            Logger.Info(
                $"Opening project directory aborted - {projectProvider.Current.LoadedWorkspaces.Count} workspaces remain open"
            );
            return false;
        }

        projectProvider.Current = Project.LoadDirectory(fileSystem, uri);
        Logger.Info("Loaded project directory");
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> AttachReferenceFile(Interaction<Unit, Uri?> interaction, Workspace wsp)
    {
        var uri = await interaction.Handle(Unit.Default);
        if (uri is null)
            return false;

        return await AttachReferenceFile(uri, wsp);
    }

    /// <inheritdoc />
    public async Task<bool> AttachReferenceFile(Uri uri, Workspace wsp)
    {
        Logger.Debug("Preparing to attach a reference file");
        var ext = Path.GetExtension(uri.LocalPath);
        try
        {
            wsp.ReferenceFileManager.Reference = ext switch
            {
                ".ass" => new AssParser().Parse(fileSystem, uri),
                ".srt" => new SrtParser().Parse(fileSystem, uri),
                _ => throw new ArgumentOutOfRangeException(nameof(uri)),
            };
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to parse file {uri.LocalPath}");
            Logger.Error(ex);
            await messageBoxService.ShowAsync(
                I18N.Resources.Error,
                $"{I18N.Resources.Error_FailedToParse}\n\n{ex.Message}",
                MsgBoxButtonSet.Ok,
                MsgBoxButton.Ok,
                MaterialIconKind.Error
            );
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> OpenVideoFile(Interaction<Unit, Uri?> interaction, Workspace workspace)
    {
        var uri = await interaction.Handle(Unit.Default);
        if (uri is null)
            return false;

        return OpenVideoFile(uri, workspace);
    }

    /// <inheritdoc />
    public async Task<bool> OpenVideoFile(Workspace workspace)
    {
        var doc = workspace.Document;
        if (!doc.GarbageManager.TryGetString("Video File", out var relVideoPath))
            return true;
        var videoPath = Path.GetFullPath(
            Path.Combine(Path.GetDirectoryName(workspace.SavePath?.LocalPath) ?? "/", relVideoPath)
        );

        if (fileSystem.File.Exists(videoPath))
        {
            var result = await messageBoxService.ShowAsync(
                I18N.Other.MsgBox_LoadVideo_Title,
                $"{I18N.Other.MsgBox_LoadVideo_Body}\n\n{relVideoPath}",
                MsgBoxButtonSet.YesNo,
                MsgBoxButton.Yes
            );
            if (result != MsgBoxButton.Yes)
                return true;
            workspace.MediaController.OpenVideo(videoPath);
            workspace.MediaController.SetSubtitles(workspace.Document);
            if (doc.GarbageManager.TryGetInt("Video Position", out var frame))
                workspace.MediaController.SeekTo(frame.Value); // Seek for clamp safety
            return true;
        }

        // Video not found
        messageService.Enqueue(
            string.Format(I18N.Other.Message_VideoNotFound, Path.GetFileName(videoPath)),
            TimeSpan.FromSeconds(7)
        );
        return false;
    }

    /// <inheritdoc />
    public bool OpenVideoFile(Uri uri, Workspace workspace)
    {
        try
        {
            workspace.MediaController.OpenVideo(uri.LocalPath);
            workspace.MediaController.SetSubtitles(workspace.Document);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return false;
        }
    }
}
