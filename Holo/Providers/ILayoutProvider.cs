// SPDX-License-Identifier: MPL-2.0

using Holo.Models;

namespace Holo.Providers;

public interface ILayoutProvider
{
    /// <summary>
    /// The current layout
    /// </summary>
    TabLayout? Current { get; set; }

    /// <summary>
    /// List of available layouts
    /// </summary>
    AssCS.Utilities.ReadOnlyObservableCollection<TabLayout> Layouts { get; }

    /// <summary>
    /// Reload layouts from disk
    /// </summary>
    void Reload();

    /// <summary>
    /// Event fired when changing layouts
    /// </summary>
    /// <remarks>Also fired when reloading completes</remarks>
    event EventHandler<LayoutChangedEventArgs>? OnLayoutChanged;

    public class LayoutChangedEventArgs(TabLayout? layout) : EventArgs
    {
        public TabLayout? Layout { get; } = layout;
    }
}
