// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel;
using AssCS;

namespace Holo.Configuration;

public interface IGlobals
{
    /// <summary>
    /// The globals style manager
    /// </summary>
    StyleManager StyleManager { get; }

    /// <summary>
    /// Global colors
    /// </summary>
    AssCS.Utilities.ReadOnlyObservableCollection<Color> Colors { get; }

    /// <summary>
    /// Add a global color
    /// </summary>
    /// <param name="color">Color to add</param>
    /// <returns><see langword="true"/> if the color was added</returns>
    bool AddColor(Color color);

    /// <summary>
    /// Remove a global color
    /// </summary>
    /// <param name="color">Color to remove</param>
    /// <returns><see langword="true"/> if the color was removed</returns>
    bool RemoveColor(Color color);

    /// <summary>
    /// Write the globals to file
    /// </summary>
    /// <returns><see langword="true"/> if saving was successful</returns>
    bool Save();

    event PropertyChangedEventHandler? PropertyChanged;
}
