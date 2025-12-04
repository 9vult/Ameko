// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Ameko.DataModels;
using Ameko.Messages;
using Holo.Configuration.Keybinds;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class KeybindsDialogViewModel : ViewModelBase
{
    private readonly IKeybindRegistrar _registrar;
    private readonly List<string> _keybindsToRemove = [];

    public RangeObservableCollection<EditableKeybind> Keybinds { get; }

    public ReactiveCommand<Unit, EmptyMessage> SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ResetCommand { get; }

    private EmptyMessage Save()
    {
        foreach (var keybind in Keybinds)
        {
            _registrar.ApplyOverride(
                keybind.QualifiedName,
                keybind.OverrideKey,
                keybind.OverrideContext.Context,
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

    private static IList<EditableKeybind> CreateEditableKeybinds(IEnumerable<Keybind> keybinds)
    {
        return keybinds
            .Select(k => new EditableKeybind
            {
                IsEnabled = k.IsEnabled,
                IsBuiltin = k.IsBuiltin,
                QualifiedName = k.QualifiedName,
                DefaultKey = k.DefaultKey ?? string.Empty,
                OverrideKey = k.OverrideKey,
                DefaultContext = k.DefaultContext,
                OverrideContext = new EditableKeybindContext(k.Context),
            })
            .OrderByDescending(k => k.OverrideContext.Context) // Places Global first and None last
            .ThenBy(k => k.QualifiedName)
            .ToList();
    }

    public KeybindsDialogViewModel(IKeybindRegistrar registrar)
    {
        _registrar = registrar;

        Keybinds = new RangeObservableCollection<EditableKeybind>(
            CreateEditableKeybinds(registrar.GetKeybinds())
        );

        SaveCommand = ReactiveCommand.Create(Save);
        DeleteCommand = ReactiveCommand.Create(
            (EditableKeybind keybind) =>
            {
                Keybinds.Remove(Keybinds.First(k => k.QualifiedName == keybind.QualifiedName));
                _keybindsToRemove.Add(keybind.QualifiedName);
            }
        );
        ResetCommand = ReactiveCommand.Create(() =>
        {
            Keybinds.Clear();
            if (registrar.ClearOverrides())
            {
                Keybinds.AddRange(CreateEditableKeybinds(registrar.GetKeybinds()));
            }
        });
    }
}
