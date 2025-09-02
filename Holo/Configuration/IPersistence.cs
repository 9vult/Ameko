// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel;
using Holo.Models;

namespace Holo.Configuration;

public interface IPersistence
{
    /// <summary>
    /// Name of the current <see cref="Layout"/>
    /// </summary>
    string LayoutName { get; set; }

    /// <summary>
    /// Whether the color picker should use a ring instead of a square
    /// </summary>
    bool UseColorRing { get; set; }

    /// <summary>
    /// C# Playground
    /// </summary>
    string PlaygroundCs { get; set; }

    /// <summary>
    /// JavaScript Playground
    /// </summary>
    string PlaygroundJs { get; set; }

    /// <summary>
    /// Write the persistence data to file
    /// </summary>
    /// <returns></returns>
    bool Save();

    event PropertyChangedEventHandler? PropertyChanged;
}
