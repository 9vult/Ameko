// SPDX-License-Identifier: GPL-3.0-only

using Ameko.ViewModels.Windows;
using AssCS;
using Holo;

namespace Ameko.Providers;

public class StylesManagerViewModelProvider(Globals globals)
{
    /// <summary>
    /// Create a new Styles Manager ViewModel for the given solution and document
    /// </summary>
    /// <param name="solution">Solution for the manager</param>
    /// <param name="document">Document for the manager</param>
    /// <returns>Styles Manager ViewModel</returns>
    public StylesManagerWindowViewModel Create(Solution solution, Document document)
    {
        return new StylesManagerWindowViewModel(globals, solution, document);
    }
}
