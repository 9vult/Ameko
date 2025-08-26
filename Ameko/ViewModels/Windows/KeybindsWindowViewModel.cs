// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Ameko.DataModels;
using Ameko.Messages;
using Holo.Configuration.Keybinds;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class KeybindsWindowViewModel : ViewModelBase
{
    private readonly IKeybindRegistrar _registrar;
    private readonly List<string> _keybindsToRemove = [];

    public ObservableCollection<EditableKeybind> Keybinds { get; }

    public ReactiveCommand<Unit, EmptyMessage> SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    private EmptyMessage Save()
    {
        foreach (var keybind in Keybinds)
        {
            _registrar.ApplyOverride(
                keybind.QualifiedName,
                keybind.OverrideKey,
                keybind.Context.Context,
                keybind.IsEnabled
            );
        }

        foreach (var keybind in _keybindsToRemove)
        {
            _registrar.DeregisterKeybind(keybind);
        }

        _registrar.Save();
        return new EmptyMessage(); // Funky workaround to get the super clean commands
    }

    public KeybindsWindowViewModel(IKeybindRegistrar registrar)
    {
        _registrar = registrar;

        Keybinds = new ObservableCollection<EditableKeybind>(
            registrar
                .GetKeybinds()
                .Select(k => new EditableKeybind
                {
                    IsEnabled = k.IsEnabled,
                    IsBuiltin = k.IsBuiltin,
                    QualifiedName = k.QualifiedName,
                    DefaultKey = k.DefaultKey ?? string.Empty,
                    OverrideKey = k.OverrideKey,
                    Context = new EditableKeybindContext(k.Context),
                })
                .OrderByDescending(k => k.Context.Context) // Places Global first and None last
                .ThenBy(k => k.QualifiedName)
                .ToList()
        );

        SaveCommand = ReactiveCommand.Create(Save);
        DeleteCommand = ReactiveCommand.Create(
            (EditableKeybind keybind) =>
            {
                Keybinds.Remove(Keybinds.First(k => k.QualifiedName == keybind.QualifiedName));
                _keybindsToRemove.Add(keybind.QualifiedName);
            }
        );
    }
}
