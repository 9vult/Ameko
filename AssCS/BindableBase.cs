// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AssCS;

/// <summary>
/// Implementation of INotifyPropertyChanged to make notifying easier
/// </summary>
public abstract class BindableBase : INotifyPropertyChanged
{
    /// <summary>
    /// Set a property, raising the PropertyChanged event
    /// </summary>
    /// <typeparam name="T">Property type</typeparam>
    /// <param name="storage">Reference to the backing member</param>
    /// <param name="value">Potential new value</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns><see langword="true"/> if the property changed</returns>
    /// <example>Example:
    /// <code>
    /// private string _data;
    /// public string Data { get => _data; set => SetProperty(ref _data, value); }
    /// </code>
    /// </example>
    protected virtual bool SetProperty<T>(
        ref T storage,
        T value,
        [CallerMemberName] string? propertyName = null
    )
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
