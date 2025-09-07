// SPDX-License-Identifier: GPL-3.0-only

using Ameko.DataModels.Sdk;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Ameko.Views.Sdk;

/// <summary>
/// A simple message box API
/// </summary>
public partial class MessageBox : Window
{
    public MessageBox(
        string title,
        string header,
        string message,
        MessageBoxButtons buttonSet,
        MaterialIconKind iconKind = MaterialIconKind.Info
    )
    {
        Title = title;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;
        Topmost = true;
        MaxWidth = 500;

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
                new RowDefinition { Height = GridLength.Auto }, // Header
                new RowDefinition { Height = GridLength.Auto }, // Body
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
            Text = header,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };
        headerLabel.SetValue(Grid.ColumnProperty, 1);
        headerLabel.SetValue(Grid.RowProperty, 0);
        grid.Children.Add(headerLabel);

        var body = new TextBlock
        {
            Text = message,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };
        body.SetValue(Grid.ColumnProperty, 1);
        body.SetValue(Grid.RowProperty, 2);
        grid.Children.Add(body);

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

        okButton.Click += (_, _) => Close(MessageBoxResult.Ok);
        yesButton.Click += (_, _) => Close(MessageBoxResult.Yes);
        noButton.Click += (_, _) => Close(MessageBoxResult.No);
        cancelButton.Click += (_, _) => Close(MessageBoxResult.Cancel);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        buttons.SetValue(Grid.ColumnProperty, 1);
        buttons.SetValue(Grid.RowProperty, 3);

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
