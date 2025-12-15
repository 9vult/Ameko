// SPDX-License-Identifier: GPL-3.0-only

using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Ameko.DataModels;
using Ameko.Messages;
using Holo.Media.Providers;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class SelectTrackDialogViewModel : ViewModelBase
{
    public TrackInformation[] Tracks { get; }

    public TrackInformation SelectedTrack
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, SelectTrackMessage> SelectTrackCommand { get; }

    public SelectTrackDialogViewModel(TrackInfo[] tracks)
    {
        Tracks = tracks
            .Select(t => new TrackInformation { Index = t.Index, Codec = t.Codec })
            .ToArray();
        SelectedTrack = Tracks[0];

        SelectTrackCommand = ReactiveCommand.Create(() =>
            new SelectTrackMessage(SelectedTrack.Index)
        );
    }
}
