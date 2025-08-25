// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using Ameko.DataModels;
using Ameko.Messages;
using Holo.Configuration.Keybinds;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class KeybindsWindowViewModel : ViewModelBase
{
    private IKeybindRegistrar _registrar;

    public ReadOnlyCollection<EditableKeybind> Keybinds { get; }

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

    private EmptyMessage Save()
    {
        foreach (var keybind in Keybinds)
        {
            _registrar.ApplyOverride(
                keybind.QualifiedName,
                keybind.OverrideKey,
                keybind.OverrideContext?.Context
            );
        }
        _registrar.Save();
        return new EmptyMessage(); // Funky workaround to get the super clean commands
    }

    private static EditableKeybindContext? GetEditableContext(KeybindContext? context)
    {
        if (context is null)
            return null;

        return new EditableKeybindContext
        {
            Context = context.Value,
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

    public KeybindsWindowViewModel(IKeybindRegistrar registrar)
    {
        _registrar = registrar;

        Keybinds = new ReadOnlyCollection<EditableKeybind>(
            registrar
                .GetKeybinds()
                .Select(k => new EditableKeybind
                {
                    QualifiedName = k.QualifiedName,
                    DefaultKey = k.DefaultKey ?? string.Empty,
                    OverrideKey = k.OverrideKey,
                    DefaultContext = GetEditableContext(k.DefaultContext)!,
                    OverrideContext = GetEditableContext(k.OverrideContext),
                })
                .OrderBy(k => k.QualifiedName)
                .ToList()
        );

        SaveCommand = ReactiveCommand.Create(Save);
    }
}
