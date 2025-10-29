// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ameko.DataModels;
using Ameko.Utilities;
using AssCS.IO;
using AssCS.Utilities;
using Holo;
using Holo.Configuration;
using Holo.IO;
using Holo.Media.Providers;
using Holo.Models;
using Holo.Providers;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using SkiaSharp;

namespace Ameko.Services;

public class IoService(
    IProjectProvider projectProvider,
    ITabFactory tabFactory,
    IFileSystem fileSystem,
    IMessageBoxService messageBoxService,
    IMessageService messageService,
    IConfiguration configuration,
    ILogger<IoService> logger
) : IIoService
{
    /// <inheritdoc />
    public async Task<bool> SaveSubtitle(Interaction<string, Uri?> interaction, Workspace wsp)
    {
        logger.LogDebug("Preparing to save subtitle file {WspTitle}", wsp.Title);
        var uriChanged = wsp.SavePath is null;
        var uri = wsp.SavePath ?? await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            logger.LogInformation("Saving {WspTitle} cancelled", wsp.Title);
            return false;
        }

        // Set the audio/video path, if applicable
        if (wsp.MediaController.IsVideoLoaded)
        {
            var dir = Path.GetDirectoryName(uri.LocalPath) ?? "/";
            var relPath = PathExtensions.GetRelativePath(dir, wsp.MediaController.VideoInfo.Path);
            wsp.Document.GarbageManager.Set("Video File", relPath);
            wsp.Document.GarbageManager.Set("Audio File", relPath);
            wsp.Document.GarbageManager.Set("Video Position", wsp.MediaController.CurrentFrame);
        }
        wsp.Document.GarbageManager.Set("Active Line", wsp.SelectionManager.ActiveEvent.Index - 1);

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        writer.Write(fileSystem, uri);
        logger.LogInformation("Saved subtitle file {WspTitle}", wsp.Title);

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
        logger.LogDebug("Preparing to save subtitle file {WspTitle}", wsp.Title);
        var isForking = wsp.SavePath is not null;
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            logger.LogInformation("Saving {WspTitle} cancelled", wsp.Title);
            return false;
        }

        // Set the audio/video path, if applicable
        if (wsp.MediaController.IsVideoLoaded)
        {
            var dir = Path.GetDirectoryName(uri.LocalPath) ?? "/";
            var relPath = PathExtensions.GetRelativePath(dir, wsp.MediaController.VideoInfo.Path);
            wsp.Document.GarbageManager.Set("Video File", relPath);
            wsp.Document.GarbageManager.Set("Audio File", relPath);
            wsp.Document.GarbageManager.Set("Video Position", wsp.MediaController.CurrentFrame);
        }
        wsp.Document.GarbageManager.Set("Active Line", wsp.SelectionManager.ActiveEvent.Index - 1);

        var writer = new AssWriter(wsp.Document, ConsumerService.AmekoInfo);
        wsp.SavePath = uri;
        wsp.IsSaved = true;
        writer.Write(fileSystem, uri);
        logger.LogInformation("Saved subtitle file {WspTitle}", wsp.Title);

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
        logger.LogDebug("Preparing to export subtitle file {WspTitle}", wsp.Title);
        var uri = await interaction.Handle(wsp.Title);

        if (uri is null)
        {
            logger.LogInformation("Exporting {WspTitle} cancelled", wsp.Title);
            return false;
        }

        var writer = new TxtWriter(wsp.Document, ConsumerService.AmekoInfo);
        writer.Write(fileSystem, uri, true);
        logger.LogInformation("Exported {WspTitle}", wsp.Title);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SafeCloseWorkspace(
        Workspace wsp,
        Interaction<string, Uri?> saveAs,
        bool replaceIfLast = true
    )
    {
        logger.LogDebug("Closing workspace {WspTitle}", wsp.Title);
        var prj = projectProvider.Current;

        if (wsp.IsSaved)
        {
            prj.CloseDocument(wsp.Id, replaceIfLast);
            tabFactory.Release(wsp);
            return true;
        }

        logger.LogTrace(
            "Displaying message box because workspace {WspTitle} is not saved",
            wsp.Title
        );

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
                    logger.LogInformation("Tab close operation aborted");
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
        logger.LogDebug("Preparing to save project file {PrjTitle}", prj.Title);
        var uri = prj.SavePath ?? await interaction.Handle(prj.Title);

        if (uri is null)
        {
            logger.LogInformation("Saving {PrjTitle} cancelled", prj.Title);
            return false;
        }

        prj.SavePath = uri;
        prj.Save();

        logger.LogInformation("Saved project file {PrjTitle}", prj.Title);
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
        logger.LogDebug("Opening subtitle file {Uri}", uri);

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

            logger.LogInformation("Opened subtitle file {WspTitle}", wsp.Title);
            prj.WorkingSpace = wsp;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse file {UriLocalPath}", uri.LocalPath);
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
        logger.LogDebug("Preparing to open project file");
        foreach (var wsp in projectProvider.Current.LoadedWorkspaces.ToArray())
        {
            await SafeCloseWorkspace(wsp, saveAs, false);
        }

        if (projectProvider.Current.LoadedWorkspaces.Count > 0)
        {
            logger.LogInformation(
                "Opening project file aborted - {LoadedWorkspacesCount} workspaces remain open",
                projectProvider.Current.LoadedWorkspaces.Count
            );
            return false;
        }

        projectProvider.Current = projectProvider.CreateFromFile(uri);
        logger.LogInformation("Loaded project file");
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> OpenProjectDirectory(Uri uri, Interaction<string, Uri?> saveAs)
    {
        logger.LogDebug("Preparing to open project directory");
        foreach (var wsp in projectProvider.Current.LoadedWorkspaces.ToArray())
        {
            await SafeCloseWorkspace(wsp, saveAs, false);
        }

        if (projectProvider.Current.LoadedWorkspaces.Count > 0)
        {
            logger.LogInformation(
                "Opening project directory aborted - {LoadedWorkspacesCount} workspaces remain open",
                projectProvider.Current.LoadedWorkspaces.Count
            );
            return false;
        }

        projectProvider.Current = projectProvider.CreateFromDirectory(uri);
        logger.LogInformation("Loaded project directory");
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
        logger.LogDebug("Preparing to attach a reference file");
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
            logger.LogError(ex, "Failed to parse file {UriLocalPath}", uri.LocalPath);
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
            logger.LogError(ex, "Failed to open video file");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveFrameToFile(
        Interaction<Unit, Uri?> interaction,
        Workspace workspace,
        SaveFrameMode mode
    )
    {
        if (!workspace.MediaController.IsVideoLoaded)
            return false;

        string path;
        var videoInfo = workspace.MediaController.VideoInfo;
        var frame = workspace.MediaController.CurrentFrame;

        switch (configuration.SaveFrames)
        {
            case SaveFrames.WithVideo:
                var sourceName = Path.GetFileNameWithoutExtension(videoInfo.Path);
                var outputName =
                    $"{sourceName}-{frame}{mode switch {
                        SaveFrameMode.VideoOnly => "-video",
                        SaveFrameMode.SubtitlesOnly => "-subs",
                        _ => string.Empty
                    }}.png";
                path = Path.Combine(Path.GetDirectoryName(videoInfo.Path) ?? "/", outputName);
                break;
            case SaveFrames.WithSubtitles when workspace.SavePath is not null:
                sourceName = Path.GetFileNameWithoutExtension(workspace.SavePath!.LocalPath);
                outputName =
                    $"{sourceName}-{frame}{mode switch {
                        SaveFrameMode.VideoOnly => "-video",
                        SaveFrameMode.SubtitlesOnly => "-subs",
                        _ => string.Empty
                    }}.png";
                path = Path.Combine(
                    Path.GetDirectoryName(workspace.SavePath!.LocalPath) ?? "/",
                    outputName
                );
                break;
            default: // Subs aren't saved to disk, so we need to request a filename from the user
                var uri = await interaction.Handle(Unit.Default);
                if (uri is null)
                    return false;
                path = uri.LocalPath;
                break;
        }

        SKData? result;
        unsafe
        {
            result = GetImageData(workspace, workspace.MediaController.GetCurrentFrame(), mode);
        }

        if (result is null)
            return false;

        await using var fs = fileSystem.FileStream.New(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );
        result.SaveTo(fs);

        messageService.Enqueue(
            string.Format(I18N.Other.Message_SavedFrame, path),
            TimeSpan.FromSeconds(5)
        );
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CopyFrameToClipboard(
        Interaction<string, Unit> interaction,
        Workspace workspace,
        SaveFrameMode mode
    )
    {
        if (!workspace.MediaController.IsVideoLoaded)
            return false;

        SKData? result;
        unsafe
        {
            result = GetImageData(workspace, workspace.MediaController.GetCurrentFrame(), mode);
        }

        if (result is null)
            return false;

        // Save to temp file because apparently copying the byte[] to the clipboard doesn't work
        var path = Path.Combine(Directories.CacheHome, $"{DateTime.Now:yyyyMMddHHmmssfff}.png");
        await using var fs = fileSystem.FileStream.New(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );
        result.SaveTo(fs);

        await interaction.Handle(path);

        messageService.Enqueue(I18N.Other.Message_CopiedFrame, TimeSpan.FromSeconds(5));
        return true;
    }

    /// <summary>
    /// Get the SKData needed for saving or copying a frame
    /// </summary>
    /// <param name="workspace">Workspace</param>
    /// <param name="group">Frame group to copy or save</param>
    /// <param name="mode">Mode to use</param>
    /// <returns>SKData if successful</returns>
    private static unsafe SKData? GetImageData(
        Workspace workspace,
        FrameGroup* group,
        SaveFrameMode mode
    )
    {
        var videoInfo = workspace.MediaController.VideoInfo!;
        var imageInfo = new SKImageInfo
        {
            Width = videoInfo.Width,
            Height = videoInfo.Height,
            ColorType = SKColorType.Bgra8888,
            AlphaType = SKAlphaType.Premul,
        };
        var bmp = new SKBitmap(imageInfo);

        switch (mode)
        {
            case SaveFrameMode.VideoOnly:
                bmp.InstallPixels(imageInfo, (nint)group->VideoFrame->Data);
                break;
            case SaveFrameMode.SubtitlesOnly:
                bmp.InstallPixels(imageInfo, (nint)group->SubtitleFrame->Data);
                break;
            case SaveFrameMode.Full:
            {
                using var vid = new SKBitmap(imageInfo);
                using var sub = new SKBitmap(imageInfo);
                vid.InstallPixels(imageInfo, (nint)group->VideoFrame->Data);
                sub.InstallPixels(imageInfo, (nint)group->SubtitleFrame->Data);

                var canvas = new SKCanvas(bmp);
                canvas.DrawBitmap(vid, 0, 0);
                canvas.DrawBitmap(sub, 0, 0);
                canvas.Flush();
                break;
            }
        }

        return SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100);
    }
}
