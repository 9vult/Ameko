// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel;

namespace Holo.Providers;

public interface IProjectProvider : INotifyPropertyChanged
{
    public Project Current { get; set; }
}
