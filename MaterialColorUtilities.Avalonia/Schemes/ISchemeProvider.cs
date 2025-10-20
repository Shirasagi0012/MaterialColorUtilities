using System;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;

/// <summary>
/// Defines a factory that can create <see cref="DynamicScheme"/> instances
/// for a given evaluation context.
/// </summary>
public interface ISchemeProvider
{
    event EventHandler? SchemeChanged;

    DynamicScheme CreateScheme(ThemeVariant theme);
}
