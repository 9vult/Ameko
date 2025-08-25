// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using System.Linq;
using Ameko.DataModels;
using Holo.Configuration.Keybinds;

namespace Ameko.ViewModels.Windows;

public partial class KeybindsWindowViewModel : ViewModelBase
{
    private IKeybindRegistrar _registrar;

    public ReadOnlyCollection<EditableKeybind> Keybinds { get; }

    private void Save()
    {
        foreach (var keybind in Keybinds)
        {
            _registrar.ApplyOverride(
                keybind.QualifiedName,
                keybind.OverrideKey,
                keybind.OverrideContext
            );
        }
        _registrar.Save();
    }

    public KeybindsWindowViewModel(IKeybindRegistrar registrar)
    {
        _registrar = registrar;

        Keybinds = new ReadOnlyCollection<EditableKeybind>(
            registrar
                .GetKeybinds()
                .Select(k => new EditableKeybind()
                {
                    QualifiedName = k.QualifiedName,
                    DefaultKey = k.DefaultKey ?? string.Empty,
                    OverrideKey = k.OverrideKey,
                    DefaultContext = k.DefaultContext,
                    OverrideContext = k.OverrideContext,
                })
                .ToList()
        );
    }
}
