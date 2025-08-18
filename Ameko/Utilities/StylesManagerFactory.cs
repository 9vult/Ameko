// SPDX-License-Identifier: GPL-3.0-only

using Ameko.ViewModels.Windows;
using AssCS;
using Holo;
using Holo.Configuration;

namespace Ameko.Utilities;

public interface IStylesManagerFactory
{
    /// <summary>
    /// Create a new Styles Manager ViewModel for the given solution and document
    /// </summary>
    /// <param name="solution">Solution for the manager</param>
    /// <param name="document">Document for the manager</param>
    /// <returns>Styles Manager ViewModel</returns>
    StylesManagerWindowViewModel Create(Solution solution, Document document);
}

public class StylesManagerFactory(IPersistence persistence, IGlobals globals)
    : IStylesManagerFactory
{
    /// <inheritdoc cref="IStylesManagerFactory.Create"/>
    public StylesManagerWindowViewModel Create(Solution solution, Document document)
    {
        return new StylesManagerWindowViewModel(persistence, globals, solution, document);
    }
}
