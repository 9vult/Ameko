// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using Holo;

namespace Ameko.ViewModels.Windows;

public partial class LogWindowViewModel : ViewModelBase
{
    public static ReadOnlyObservableCollection<string> LogEntries =>
        HoloContext.Instance.LogEntries;
}
