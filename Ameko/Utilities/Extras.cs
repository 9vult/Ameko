// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;

namespace Ameko.Utilities;

public static class Extras
{
    /// <summary>
    /// Enumerate over the set flags in a [Flags] enum
    /// </summary>
    /// <param name="value">Enum to evaluate</param>
    /// <returns>Enumerable</returns>
    public static IEnumerable<T> GetFlags<T>(this T value)
        where T : Enum
    {
        var bits = Convert.ToUInt64(value);
        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagValue = Convert.ToUInt64(flag);
            if (flagValue != 0 && (bits & flagValue) == flagValue)
                yield return flag;
        }
    }

    /// <summary>
    /// Pre-KeyDown event handler for Time <seealso cref="TextBox"/>es
    /// </summary>
    /// <param name="sender">The textbox object</param>
    /// <param name="e">Key event args</param>
    public static void PreKeyDownEventHandler(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox box)
            return;

        // TODO: cut/copy/paste handling

        // Movement
        switch (e.Key)
        {
            // Left
            case Key.Back:
            case Key.Left:
            case Key.Down:
                box.CaretIndex -= 1;
                e.Handled = true;
                return;
            // Start
            case Key.Home:
                box.CaretIndex = 0;
                e.Handled = true;
                return;
            // End
            case Key.End:
                box.CaretIndex = box.Text!.Length;
                e.Handled = true;
                return;
            // Right
            case Key.Right:
            case Key.Up:
                box.CaretIndex += 1;
                e.Handled = true;
                return;
        }

        // Discard input if at the end of the box
        if (box.CaretIndex >= box.Text!.Length)
        {
            e.Handled = true;
            return;
        }

        var key = Convert.ToChar(e.KeySymbol ?? "a");

        // Discard input if invalid character
        if (key is < '0' or > '9' && key != ';' && key != '.' && key != ',' && key != ':')
        {
            e.Handled = true;
            return;
        }

        // Move forward if at punctuation
        var next = box.Text?[box.CaretIndex] ?? '-';
        if (next is ':' or '.' or ',')
        {
            box.CaretIndex += 1;
        }

        // Ignore punctuation keys
        if (key is ';' or '.' or ',' or ':')
        {
            e.Handled = true;
            return;
        }

        // Good input
        box.Text = box.Text!.Remove(box.CaretIndex, 1)
            .Insert(box.CaretIndex, Convert.ToString(key));

        box.CaretIndex += 1;
        e.Handled = true;
    }
}
