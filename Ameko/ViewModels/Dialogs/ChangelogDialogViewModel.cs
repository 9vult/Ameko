// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using Avalonia.Platform;
using Markdig;
using Markdown.ColorCode;

namespace Ameko.ViewModels.Dialogs;

public partial class ChangelogDialogViewModel(string changelog) : ViewModelBase
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseColorCode()
        .Build();

    private static readonly string DarkCss = new StreamReader(
        AssetLoader.Open(new Uri("avares://Ameko/Assets/Css/md.dark.css"))
    ).ReadToEnd();

    public string ChangelogContent { get; } =
        $"<style>\n{DarkCss}\n</style>\n<body>\n"
        + Markdig.Markdown.ToHtml(changelog, Pipeline)
        + "\n</body>";
}
