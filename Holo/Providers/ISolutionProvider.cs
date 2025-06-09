// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel;

namespace Holo.Providers;

public interface ISolutionProvider : INotifyPropertyChanged
{
    public Solution Current { get; set; }
}
