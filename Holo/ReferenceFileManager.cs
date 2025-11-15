// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using AssCS;

namespace Holo;

public class ReferenceFileManager : BindableBase
{
    private readonly SelectionManager _selectionManager;

    /// <summary>
    /// Document being referenced
    /// </summary>
    public Document? Reference
    {
        get;
        set
        {
            SetProperty(ref field, value);
            IsReferenceLoaded = value is not null;
            RaisePropertyChanged(nameof(IsReferenceLoaded));
            GetCorrespondingLines();
        }
    }

    /// <summary>
    /// If the reference file is loaded
    /// </summary>
    [MemberNotNullWhen(true, nameof(Reference))]
    public bool IsReferenceLoaded { get; private set; }

    public string CurrentLines
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public void Shift(int seconds)
    {
        if (!IsReferenceLoaded)
            return;

        var offset = Time.FromSeconds(seconds);
        foreach (var @event in Reference.EventManager.Events)
        {
            @event.Start += offset;
            @event.End += offset;
        }
        GetCorrespondingLines();
    }

    private void GetCorrespondingLines()
    {
        if (!IsReferenceLoaded)
            return;
        var hits = Reference
            .EventManager.Events.Where(e => e.CollidesWith(_selectionManager.ActiveEvent))
            .Select(e => e.Text);
        CurrentLines = string.Join(Environment.NewLine, hits);
    }

    public ReferenceFileManager(SelectionManager selectionManager)
    {
        _selectionManager = selectionManager;
        _selectionManager.PropertyChanged += (_, _) => GetCorrespondingLines();
    }
}
