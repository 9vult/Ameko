// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using TheArtOfDev.HtmlRenderer.Core.Entities;

namespace Ameko.Views.Windows;

public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();

        HtmlPanel.ImageLoad += (_, e) =>
        {
            ImageLoad(e.Event);
        };
    }

    private static void ImageLoad(HtmlImageLoadEventArgs e)
    {
        var source = new Uri(Path.Combine("avares://Ameko/Assets/Help/en-US/", e.Src));
        if (!AssetLoader.Exists(source))
            return;

        var image = new Bitmap(AssetLoader.Open(source));
        e.Callback(image);
    }
}
