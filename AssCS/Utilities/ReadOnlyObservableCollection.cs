// SPDX-License-Identifier: MPL-2.0

// Adapted from anvil-csharp-core under the MIT license

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AssCS.Utilities;

/// <summary>
/// An alias of <see cref="System.Collections.ObjectModel.ReadOnlyObservableCollection{T}"/>
/// that exposes <see cref="CollectionChanged"/>
/// </summary>
/// <typeparam name="T">The collection's element type</typeparam>
/// <remarks>
/// This alias is required because .NET currently does not want
/// to introduce a binary-breaking change by exposing the <see cref="CollectionChanged"/>
/// event publicly.
/// </remarks>
/// <seealso href="https://github.com/dotnet/runtime/issues/14267"/>
public class ReadOnlyObservableCollection<T>
    : System.Collections.ObjectModel.ReadOnlyObservableCollection<T>
{
    /// <inheritdoc cref="INotifyCollectionChanged.CollectionChanged"/>
    public new event NotifyCollectionChangedEventHandler CollectionChanged
    {
        add => base.CollectionChanged += value;
        remove => base.CollectionChanged -= value;
    }

    /// <inheritdoc cref="System.Collections.ObjectModel.ReadOnlyObservableCollection{T}"/>
    public ReadOnlyObservableCollection(ObservableCollection<T> list)
        : base(list) { }
}
