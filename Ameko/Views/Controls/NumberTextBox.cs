// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace Ameko.Views.Controls;

public class NumberTextBox : TextBox
{
    protected override Type StyleKeyOverride => typeof(TextBox);

    /// <summary>
    /// Minimum allowed value
    /// </summary>
    public static readonly StyledProperty<decimal> MinimumProperty = AvaloniaProperty.Register<
        NumberTextBox,
        decimal
    >(nameof(Minimum), defaultValue: int.MinValue);

    /// <summary>
    /// Maximum allowed value
    /// </summary>
    public static readonly StyledProperty<decimal> MaximumProperty = AvaloniaProperty.Register<
        NumberTextBox,
        decimal
    >(nameof(Maximum), defaultValue: int.MaxValue);

    /// <summary>
    /// Amount to increment or decrement by
    /// </summary>
    public static readonly StyledProperty<decimal> IncrementProperty = AvaloniaProperty.Register<
        NumberTextBox,
        decimal
    >(nameof(Increment), defaultValue: 1m);

    /// <summary>
    /// Display format
    /// </summary>
    public static readonly StyledProperty<string> FormatStringProperty = AvaloniaProperty.Register<
        NumberTextBox,
        string
    >(nameof(FormatString), defaultValue: "0");

    /// <summary>
    /// Whether to allow decimal numbers or not
    /// </summary>
    public static readonly StyledProperty<bool> AllowDecimalProperty = AvaloniaProperty.Register<
        NumberTextBox,
        bool
    >(nameof(AllowDecimal), defaultValue: false);

    public static readonly StyledProperty<decimal> ValueProperty = AvaloniaProperty.Register<
        NumberTextBox,
        decimal
    >(nameof(AllowDecimal), defaultValue: 0m);

    public static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent = RoutedEvent.Register<
        NumberTextBox,
        RoutedEventArgs
    >(nameof(ValueChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// Minimum allowed value
    /// </summary>
    public decimal Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Maximum allowed value
    /// </summary>
    public decimal Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Amount to increment or decrement by
    /// </summary>
    public decimal Increment
    {
        get => GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    /// <summary>
    /// Display format
    /// </summary>
    public string FormatString
    {
        get => GetValue(FormatStringProperty);
        set
        {
            SetValue(FormatStringProperty, value);
            Text = Value.ToString(FormatString, NumberFormatInfo.InvariantInfo);
        }
    }

    /// <summary>
    /// Whether to allow decimal numbers or not
    /// </summary>
    public bool AllowDecimal
    {
        get => GetValue(AllowDecimalProperty);
        set => SetValue(AllowDecimalProperty, value);
    }

    /// <summary>
    /// Current value
    /// </summary>
    public decimal Value
    {
        get => GetValue(ValueProperty);
        set
        {
            SetValue(ValueProperty, value);
            RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }
    }

    public event EventHandler<RoutedEventArgs> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public NumberTextBox()
    {
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        AddHandler(PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel);
        Text = Value.ToString(FormatString, NumberFormatInfo.InvariantInfo);
    }

    /// <summary>
    /// Text input handler
    /// </summary>
    /// <param name="sender">The textbox object</param>
    /// <param name="e">Input event args</param>
    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (sender is not TextBox)
            return;

        if (!string.IsNullOrWhiteSpace(e.Text))
            InsertText(e.Text);

        e.Handled = true;
    }

    /// <summary>
    /// Navigation handler
    /// </summary>
    /// <param name="sender">The textbox object</param>
    /// <param name="e">Key event args</param>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox)
            return;

        // Movement
        switch (e.Key)
        {
            // Backspace
            case Key.Back:
                base.OnKeyDown(e);
                if (Text?.Length == 0)
                {
                    Value = 0;
                    Text = Value.ToString(FormatString, NumberFormatInfo.InvariantInfo);
                    CaretIndex = Text.Length;
                }
                return;
            // Left
            case Key.Left:
                CaretIndex -= 1;
                e.Handled = true;
                return;
            // Start
            case Key.Home:
                CaretIndex = 0;
                e.Handled = true;
                return;
            // End
            case Key.End:
                CaretIndex = Text!.Length;
                e.Handled = true;
                return;
            // Right
            case Key.Right:
                CaretIndex += 1;
                e.Handled = true;
                return;
            // Increment
            case Key.Up:
                if (decimal.TryParse(Text, out var dec))
                {
                    var val = Math.Min(dec + Increment, Maximum);
                    Text = val.ToString(FormatString, NumberFormatInfo.InvariantInfo);
                    Value = val;
                }
                e.Handled = true;
                return;
            // Decrement
            case Key.Down:
                if (decimal.TryParse(Text, out dec))
                {
                    var val = Math.Max(dec - Increment, Minimum);
                    Text = val.ToString(FormatString, NumberFormatInfo.InvariantInfo);
                    Value = val;
                }
                e.Handled = true;
                return;
            // Paste
            case Key.V when (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0:
                ClipboardPaste();
                e.Handled = true;
                return;
            // Copy
            case Key.C when (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0:
                ClipboardCopy();
                e.Handled = true;
                return;
        }
    }

    /// <summary>
    /// Scroll handler
    /// </summary>
    /// <param name="sender">The textbox object</param>
    /// <param name="e">Scroll event args</param>
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // Increment
        if (e.Delta.Y > 0)
        {
            if (decimal.TryParse(Text, out var dec))
            {
                var val = Math.Min(dec + Increment, Maximum);
                Text = val.ToString(FormatString, NumberFormatInfo.InvariantInfo);
                Value = val;
            }
        }
        // Decrement
        else
        {
            if (decimal.TryParse(Text, out var dec))
            {
                var val = Math.Max(dec - Increment, Minimum);
                Text = val.ToString(FormatString, NumberFormatInfo.InvariantInfo);
                Value = val;
            }
        }
        e.Handled = true;
    }

    /// <summary>
    /// Try to get the selected text and copy it
    /// </summary>
    private void ClipboardCopy()
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

            // There is an actual selection
            if (SelectionStart != SelectionEnd)
            {
                clipboard?.SetTextAsync(SelectedText);
                return;
            }

            // Copy the whole box
            clipboard?.SetTextAsync(Text);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    /// <summary>
    /// Try to get the text in the clipboard and paste it
    /// </summary>
    private void ClipboardPaste()
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

            var text = clipboard?.TryGetTextAsync().Result;
            if (text is null)
                return;

            InsertText(text);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    /// <summary>
    /// Insert the text
    /// </summary>
    /// <param name="input">Text to insert</param>
    private void InsertText(string input)
    {
        foreach (var c in input)
        {
            // Discard input if invalid character
            if (c is (< '0' or > '9') and (not '.' or ','))
                continue;

            if (!AllowDecimal && c is '.' or ',')
                continue;

            // Move forward if at punctuation
            if (CaretIndex < Text?.Length)
            {
                var next = Text?[CaretIndex] ?? '-';
                if (c is '.' or ',' && next is '.' or ',')
                    CaretIndex += 1;
            }

            // Ignore punctuation keys
            if (c is '.' or ',' && (Text?.Contains('.') ?? true))
                continue;

            // Good input
            Text = Text?.Insert(CaretIndex, c.ToString());

            CaretIndex += 1;
        }

        if (!decimal.TryParse(Text, out var dec))
            dec = 0;

        var val = Math.Clamp(dec, Minimum, Maximum);
        Text = val.ToString(FormatString, NumberFormatInfo.InvariantInfo);
        Value = val;
    }
}
