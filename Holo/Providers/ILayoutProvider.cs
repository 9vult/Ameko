// SPDX-License-Identifier: MPL-2.0

using Holo.Models;

namespace Holo.Providers;

public interface ILayoutProvider
{
    /// <summary>
    /// The current layout
    /// </summary>
    TabLayout Current { get; set; }

    /// <summary>
    /// List of available layouts
    /// </summary>
    AssCS.Utilities.ReadOnlyObservableCollection<TabLayout> Layouts { get; }

    /// <summary>
    /// Reload layouts from disk
    /// </summary>
    void Reload();

    /// <summary>
    /// Event fired when reloading completes
    /// </summary>
    event ReloadEventHandler? OnReload;

    /// <summary>
    /// Event Handler for <see cref="Reload"/>
    /// </summary>
    public delegate void ReloadEventHandler(object sender, EventArgs e);
}
