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

    public ObservableCollection<EditableKeybind> Keybinds { get; }

    public ReadOnlyCollection<EditableKeybindContext> Contexts { get; } =
        new(
            [
                GetEditableContext(KeybindContext.None)!,
                GetEditableContext(KeybindContext.Global)!,
                GetEditableContext(KeybindContext.Grid)!,
                GetEditableContext(KeybindContext.Editor)!,
                GetEditableContext(KeybindContext.Video)!,
                GetEditableContext(KeybindContext.Audio)!,
            ]
        );

    public ReactiveCommand<Unit, EmptyMessage> SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    private List<string> _keybindsToRemove = [];

    private static EditableKeybindContext GetEditableContext(KeybindContext context)
    {
        return new EditableKeybindContext
        {
            Context = context,
            Display = context switch
            {
                KeybindContext.None => I18N.Keybinds.KeybindContext_None,
                KeybindContext.Global => I18N.Keybinds.KeybindContext_Global,
                KeybindContext.Grid => I18N.Keybinds.KeybindContext_Grid,
                KeybindContext.Editor => I18N.Keybinds.KeybindContext_Editor,
                KeybindContext.Video => I18N.Keybinds.KeybindContext_Video,
                KeybindContext.Audio => I18N.Keybinds.KeybindContext_Audio,
                _ => throw new ArgumentOutOfRangeException(nameof(context), context, null),
            },
        };
    }

    private EmptyMessage Save()
    {
        foreach (var keybind in Keybinds)
        {
            _registrar.ApplyOverride(
                keybind.QualifiedName,
                keybind.OverrideKey,
                keybind.Context.Context
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
                    IsBuiltin = k.IsBuiltin,
                    QualifiedName = k.QualifiedName,
                    DefaultKey = k.DefaultKey ?? string.Empty,
                    OverrideKey = k.OverrideKey,
                    Context = GetEditableContext(k.Context),
                })
                .OrderBy(k => k.QualifiedName)
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
