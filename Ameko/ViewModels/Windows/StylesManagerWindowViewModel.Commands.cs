// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using AssCS;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class StylesManagerWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Create a new file
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

                switch (dest)
                {
                    case "global":
                        throw new NotImplementedException();
                    case "solution":
                        Solution.StyleManager.AddOrReplace(
                            Style.FromStyle(Solution.StyleManager.NextId, style)
                        );
                        break;
                    case "document":
                        Document.StyleManager.AddOrReplace(
                            Style.FromStyle(Document.StyleManager.NextId, style)
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dest), dest, null);
                }
            }
        );
    }
}
