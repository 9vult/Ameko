// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using AssCS;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class StylesManagerWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Duplicate a style
    /// </summary>
    private ReactiveCommand<string, Unit> CreateDuplicateCommand()
    {
        return ReactiveCommand.Create(
            (string input) =>
            {
                var (style, manager) = input switch
                {
                    "global" => (SelectedGlobalStyle, Globals.StyleManager),
                    "solution" => (SelectedSolutionStyle, Solution.StyleManager),
                    "document" => (SelectedDocumentStyle, Document.StyleManager),
                    _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
                };

                if (style is null)
                    return;

                var newStyle = Style.FromStyle(manager.NextId, style);
                while (manager.TryGet(newStyle.Name, out _))
                    newStyle.Name += $" ({I18N.StylesManager.StylesManager_CopyAppendage})";

                manager.Add(newStyle);
            }
        );
    }

    /// <summary>
    /// Delete a style
    /// </summary>
    private ReactiveCommand<string, Unit> CreateDeleteCommand()
    {
        return ReactiveCommand.Create(
            (string input) =>
            {
                var (style, manager) = input switch
                {
                    "global" => (SelectedGlobalStyle, Globals.StyleManager),
                    "solution" => (SelectedSolutionStyle, Solution.StyleManager),
                    "document" => (SelectedDocumentStyle, Document.StyleManager),
                    _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
                };

                if (style is null)
                    return;

                manager.Remove(style.Name);

                // Ensure documents always have at least one style
                if (input == "document" && manager.Styles.Count == 0)
                    manager.Add(new Style(manager.NextId));
            }
        );
    }

    /// <summary>
    /// Copy a style from one group to another
    /// </summary>
    private ReactiveCommand<string, Unit> CreateCopyToCommand()
    {
        return ReactiveCommand.Create(
            (string input) =>
            {
                var splits = input.Split(',');
                if (splits.Length != 2)
                    throw new ArgumentException();

                var origin = splits[0];
                var dest = splits[1];

                var style = origin switch
                {
                    "global" => SelectedGlobalStyle,
                    "solution" => SelectedSolutionStyle,
                    "document" => SelectedDocumentStyle,
                    _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null),
                };

                if (style is null)
                    return;

                var manager = dest switch
                {
                    "global" => Globals.StyleManager,
                    "solution" => Solution.StyleManager,
                    "document" => Document.StyleManager,
                    _ => throw new ArgumentOutOfRangeException(nameof(dest), dest, null),
                };

                manager.AddOrReplace(Style.FromStyle(manager.NextId, style));
            }
        );
    }
}
