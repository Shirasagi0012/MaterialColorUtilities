using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Collections;

namespace MaterialColorUtilities.Avalonia;

/// <summary>
/// Strongly typed collection of <see cref="ExtraPalette"/> items keyed by identifier.
/// </summary>
public sealed class ExtendedPaletteCollection : AvaloniaDictionary<object, ExtendedPalette>
{
    readonly private HashSet<ExtendedPalette> _tracked = new();

    public event EventHandler? PalettesChanged;

    public ExtendedPaletteCollection()
    {
        CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var palette in _tracked)
                palette.ColorChanged -= OnPaletteChanged;
            _tracked.Clear();

            foreach ((var _, var palette) in this)
                TrackPalette(palette);

            PalettesChanged?.Invoke(sender: this, e: EventArgs.Empty);

            return;
        }

        if (e.OldItems is {})
            foreach (var item in e.OldItems)
                if (item is KeyValuePair<object, ExtendedPalette> kvp)
                    UntrackPalette(kvp.Value);

        if (e.NewItems is {})
            foreach (var item in e.NewItems)
                if (item is KeyValuePair<object, ExtendedPalette> kvp)
                    TrackPalette(kvp.Value);

        PalettesChanged?.Invoke(sender: this, e: EventArgs.Empty);
    }

    private void TrackPalette(ExtendedPalette palette)
    {
        if (_tracked.Add(palette))
            palette.ColorChanged += OnPaletteChanged;
    }

    private void UntrackPalette(ExtendedPalette palette)
    {
        if (_tracked.Remove(palette))
            palette.ColorChanged -= OnPaletteChanged;
    }

    private void OnPaletteChanged(object? sender, EventArgs e)
    {
        PalettesChanged?.Invoke(sender: this, e: EventArgs.Empty);
    }
}
