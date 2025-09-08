// SPDX-License-Identifier: GPL-3.0-only

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Holo.Models;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Ameko.Views.Sdk;

/// <summary>
/// A simple input box API
/// </summary>
public partial class InputBox : Window
{
    public string InputText { get; private set; }

    public InputBox(
        string title,
        string text,
        string initialText,
        MessageBoxButtons buttonSet,
        MaterialIconKind iconKind = MaterialIconKind.Info
    )
    {
        Title = title;
        Icon = new WindowIcon(
            AssetLoader.Open(new Uri("avares://Ameko/Assets/Ameko-Simplified-BG-64.ico"))
        );
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        CanResize = false;
        Topmost = true;
        MaxWidth = 500;

        InputText = initialText;

        var grid = new Grid
        {
            Margin = new Thickness(20),
            ColumnSpacing = 10,
            RowSpacing = 10,
            ColumnDefinitions =
            [
                new ColumnDefinition { Width = GridLength.Auto }, // Icon
                new ColumnDefinition { Width = GridLength.Star }, // Content
            ],
            RowDefinitions =
            [
                new RowDefinition { Height = GridLength.Auto }, // Text
                new RowDefinition { Height = GridLength.Auto }, // Input
                new RowDefinition { Height = GridLength.Auto }, // Buttons
            ],
        };
        Content = grid;

        var icon = new MaterialIcon
        {
            Kind = iconKind,
            Width = 32,
            Height = 32,
        };
        icon.SetValue(Grid.ColumnProperty, 0);
        icon.SetValue(Grid.RowProperty, 0);
        grid.Children.Add(icon);

        var headerLabel = new TextBlock
        {
            Text = text,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };
        headerLabel.SetValue(Grid.ColumnProperty, 1);
        headerLabel.SetValue(Grid.RowProperty, 0);
        grid.Children.Add(headerLabel);

        var inputBox = new TextBox
        {
            Text = InputText,
            AcceptsReturn = false,
            AcceptsTab = false,
            TextWrapping = TextWrapping.Wrap,
        };
        inputBox.SetValue(Grid.ColumnProperty, 1);
        inputBox.SetValue(Grid.RowProperty, 1);
        grid.Children.Add(inputBox);

        var okButton = new Button
        {
            Content = new TextBlock { Text = I18N.Other.MsgBox_Btn_OK },
            Width = 75,
        };
        var yesButton = new Button
        {
            Content = new TextBlock { Text = I18N.Other.MsgBox_Btn_Yes },
            Width = 75,
        };
        var noButton = new Button
        {
            Content = new TextBlock { Text = I18N.Other.MsgBox_Btn_No },
            Width = 75,
        };
        var cancelButton = new Button
        {
            Content = new TextBlock { Text = I18N.Other.MsgBox_Btn_Cancel },
            Width = 75,
        };

        okButton.Click += (_, _) =>
        {
            InputText = inputBox.Text;
            Close(MessageBoxResult.Ok);
        };
        yesButton.Click += (_, _) =>
        {
            InputText = inputBox.Text;
            Close(MessageBoxResult.Yes);
        };
        noButton.Click += (_, _) =>
        {
            InputText = inputBox.Text;
            Close(MessageBoxResult.No);
        };
        cancelButton.Click += (_, _) =>
        {
            InputText = inputBox.Text;
            Close(MessageBoxResult.Cancel);
        };

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        buttons.SetValue(Grid.ColumnProperty, 1);
        buttons.SetValue(Grid.RowProperty, 2);

        switch (buttonSet)
        {
            case MessageBoxButtons.Ok:
                buttons.Children.Add(okButton);
                break;
            case MessageBoxButtons.YesNo:
                buttons.Children.AddRange([noButton, yesButton]);
                break;
            case MessageBoxButtons.OkCancel:
                buttons.Children.AddRange([cancelButton, okButton]);
                break;
            case MessageBoxButtons.YesNoCancel:
                buttons.Children.AddRange([cancelButton, noButton, yesButton]);
                break;
        }
        grid.Children.Add(buttons);
    }
}
