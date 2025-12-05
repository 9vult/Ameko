// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Ameko.DataModels;
using Avalonia.Platform;
using Holo.Scripting;
using Markdig;
using Markdown.ColorCode;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class HelpWindowViewModel(IPackageManager packageManager, IFileSystem fileSystem)
    : ViewModelBase
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseColorCode()
        .Build();

    private static readonly string DarkCss = new StreamReader(
        AssetLoader.Open(new Uri("avares://Ameko/Assets/Css/md.dark.css"))
    ).ReadToEnd();

    public List<ScriptHelp> ScriptHelps =>
        packageManager
            .InstalledPackages.Where(m => !string.IsNullOrEmpty(m.HelpUrl))
            .Select(m => new ScriptHelp
            {
                DisplayName = m.DisplayName,
                Content = BuildScriptHelp(PackageManager.HelpPath(m)),
            })
            .ToList();

    public ScriptHelp? SelectedScriptHelp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private string BuildScriptHelp(string path)
    {
        using var fs = fileSystem.FileStream.New(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );
        using var reader = new StreamReader(fs);
        var raw = reader.ReadToEnd();

        return $"<style>\n{DarkCss}\n</style>\n<body>\n"
            + Markdig.Markdown.ToHtml(raw, Pipeline)
            + "\n</body>";
    }
}
