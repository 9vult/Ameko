// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using Ameko.Messages;
using Holo.Configuration;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class ColorDialogViewModel : ViewModelBase
{
    private readonly IPersistence _persistence;
    private readonly AssCS.Color _assColor;
    private Avalonia.Media.HsvColor _hsvColor;
    private bool _useRing;

    public ReactiveCommand<Unit, ColorDialogClosedMessage> SelectColorCommand { get; }

    public AssCS.Color Color => _assColor;

    public string AssFormattedColor => _assColor.AsOverrideColor();

    public Avalonia.Media.HsvColor HsvColor
    {
        get => _hsvColor;
        set
        {
            var rgb = value.ToRgb();
            _assColor.Alpha = (byte)(255 - rgb.A);
            _assColor.Red = rgb.R;
            _assColor.Green = rgb.G;
            _assColor.Blue = rgb.B;
            this.RaiseAndSetIfChanged(ref _hsvColor, value);
            this.RaisePropertyChanged(nameof(Color));
            this.RaisePropertyChanged(nameof(AssFormattedColor));
        }
    }

    /// <summary>
    /// Sets the configuration value as well
    /// </summary>
    public bool UseRing
    {
        get => _useRing;
        set
        {
            this.RaiseAndSetIfChanged(ref this._useRing, value);
            _persistence.UseColorRing = value;
        }
    }

    /// <summary>
    /// Initialize the Color dialog
    /// </summary>
    /// <param name="persistence">Application persistence</param>
    /// <param name="color">Color being edited</param>
    public ColorDialogViewModel(IPersistence persistence, AssCS.Color color)
    {
        _persistence = persistence;

        _assColor = color;
        _hsvColor = new Avalonia.Media.HsvColor(
            new Avalonia.Media.Color((byte)(255 - color.Alpha), color.Red, color.Green, color.Blue)
        );

        SelectColorCommand = ReactiveCommand.Create(() => new ColorDialogClosedMessage(_assColor));

        _useRing = persistence.UseColorRing;
    }
}
