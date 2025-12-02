// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.ViewModels.Dialogs;
using AssCS;
using AssCS.History;
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
                    "project" => (SelectedProjectStyle, Project.StyleManager),
                    "document" => (SelectedDocumentStyle, Document.StyleManager),
                    _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
                };

                if (style is null)
                    return;

                var newStyle = Style.FromStyle(manager.NextId, style);
                while (manager.TryGet(newStyle.Name, out _))
                    newStyle.Name += $" ({I18N.StylesManager.StylesManager_CopyAppendage})";

                manager.Add(newStyle);

                if (input == "document")
                    Document.HistoryManager.Commit(ChangeType.AddStyle);
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
                    "project" => (SelectedProjectStyle, Project.StyleManager),
                    "document" => (SelectedDocumentStyle, Document.StyleManager),
                    _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
                };

                if (style is null)
                    return;

                manager.Remove(style.Name);

                // Ensure documents always have at least one style
                if (input == "document" && manager.Styles.Count == 0)
                    manager.Add(new Style(manager.NextId));

                if (input == "document")
                    Document.HistoryManager.Commit(ChangeType.RemoveStyle);
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
                    "project" => SelectedProjectStyle,
                    "document" => SelectedDocumentStyle,
                    _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null),
                };

                if (style is null)
                    return;

                var manager = dest switch
                {
                    "global" => Globals.StyleManager,
                    "project" => Project.StyleManager,
                    "document" => Document.StyleManager,
                    _ => throw new ArgumentOutOfRangeException(nameof(dest), dest, null),
                };

                manager.AddOrReplace(Style.FromStyle(manager.NextId, style));

                if (dest == "document")
                    Document.HistoryManager.Commit(ChangeType.AddStyle);
            }
        );
    }

    /// <summary>
    /// Edit a style
    /// </summary>
    private ReactiveCommand<string, Unit> CreateEditStyleCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (string input) =>
            {
                var (style, manager, document) = input switch
                {
                    "global" => (SelectedGlobalStyle, Globals.StyleManager, null),
                    "project" => (SelectedProjectStyle, Project.StyleManager, null),
                    "document" => (SelectedDocumentStyle, Document.StyleManager, Document),
                    _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
                };

                if (style is null)
                    return;

                var clone = style.Clone();

                var vm = new StyleEditorDialogViewModel(_persistence, style, manager, document);
                var result = await ShowStyleEditorWindow.Handle(vm);

                // Revert if aborted
                if (result is null)
                {
                    style.SetFields(StyleField.All, clone);
                    return;
                }

                if (input == "document")
                    Document.HistoryManager.Commit(ChangeType.ModifyStyle);
            }
        );
    }
}
