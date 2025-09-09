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

    /// <summary>
    /// A box for getting user input
    /// </summary>
    /// <param name="title">Window title</param>
    /// <param name="text">Body</param>
    /// <param name="initialText">Initial text in the input</param>
    /// <param name="buttonSet">Which buttons to use</param>
    /// <param name="primary">Which button is default</param>
    /// <param name="iconKind">Which icon to use</param>
    public InputBox(
        string title,
        string text,
        string initialText,
        MsgBoxButtonSet buttonSet = MsgBoxButtonSet.Ok,
        MsgBoxButton primary = MsgBoxButton.Ok,
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
            IsDefault = primary == MsgBoxButton.Ok,
        };
        var yesButton = new Button
        {
            Content = new TextBlock { Text = I18N.Other.MsgBox_Btn_Yes },
            Width = 75,
            IsDefault = primary == MsgBoxButton.Yes,
        };
        var noButton = new Button
        {
            Content = new TextBlock { Text = I18N.Other.MsgBox_Btn_No },
            Width = 75,
            IsDefault = primary == MsgBoxButton.No,
        };
        var cancelButton = new Button
        {
            Content = new TextBlock { Text = I18N.Other.MsgBox_Btn_Cancel },
            Width = 75,
            IsDefault = primary == MsgBoxButton.Cancel,
        };

        okButton.Classes.Add(primary == MsgBoxButton.Ok ? "Primary" : "Tertiary");
        yesButton.Classes.Add(primary == MsgBoxButton.Yes ? "Primary" : "Tertiary");
        noButton.Classes.Add(primary == MsgBoxButton.No ? "Primary" : "Tertiary");
        cancelButton.Classes.Add(primary == MsgBoxButton.Cancel ? "Primary" : "Tertiary");

        okButton.Click += (_, _) =>
        {
            InputText = inputBox.Text;
            Close(MsgBoxButton.Ok);
        };
        yesButton.Click += (_, _) =>
        {
            InputText = inputBox.Text;
            Close(MsgBoxButton.Yes);
        };
        noButton.Click += (_, _) =>
        {
            InputText = inputBox.Text;
            Close(MsgBoxButton.No);
        };
        cancelButton.Click += (_, _) =>
        {
            InputText = inputBox.Text;
            Close(MsgBoxButton.Cancel);
        };

        var isMacOs = OperatingSystem.IsMacOS();

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = isMacOs ? HorizontalAlignment.Right : HorizontalAlignment.Left,
        };
        buttons.SetValue(Grid.ColumnProperty, 1);
        buttons.SetValue(Grid.RowProperty, 2);

        switch (buttonSet)
        {
            case MsgBoxButtonSet.Ok:
                buttons.Children.Add(okButton);
                break;
            case MsgBoxButtonSet.YesNo:
                buttons.Children.AddRange(isMacOs ? [noButton, yesButton] : [yesButton, noButton]);
                break;
            case MsgBoxButtonSet.OkCancel:
                buttons.Children.AddRange(
                    isMacOs ? [cancelButton, okButton] : [okButton, cancelButton]
                );
                break;
            case MsgBoxButtonSet.YesNoCancel:
                buttons.Children.AddRange(
                    isMacOs
                        ? [cancelButton, noButton, yesButton]
                        : [yesButton, noButton, cancelButton]
                );
                break;
        }
        grid.Children.Add(buttons);
    }
}
