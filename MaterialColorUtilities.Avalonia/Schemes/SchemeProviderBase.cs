using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;

public abstract class SchemeProviderBase : AvaloniaObject, ISchemeProvider
{
    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<SchemeProviderBase, Color?>(nameof(Color));

    public static readonly StyledProperty<double?> ContrastLevelProperty =
        AvaloniaProperty.Register<SchemeProviderBase, double?>(nameof(ContrastLevel));

    public static readonly StyledProperty<ColorSpec.SpecVersion> SpecVersionProperty =
        AvaloniaProperty.Register<SchemeProviderBase, ColorSpec.SpecVersion>(
            nameof(SpecVersion),
            DynamicScheme.DefaultSpecVersion
        );

    protected SchemeProviderBase()
    {
        PropertyChanged += OnPropertyChangedInternal;
    }

    protected SchemeProviderBase(IBinding binding) : this()
    {
        this[!ColorProperty] = binding;
    }

    protected SchemeProviderBase(Color color) : this()
    {
        Color = color;
    }

    protected SchemeProviderBase(string colorString) : this()
    {
        if (global::Avalonia.Media.Color.TryParse(colorString, out var color))
            Color = color;
        else
            throw new FormatException($"'{colorString}' is not a valid color string.");
    }

    // TODO: Support for StaticResource, which calls ctor with Object? parameter

    public event EventHandler? SchemeChanged;

    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public double? ContrastLevel
    {
        get => GetValue(ContrastLevelProperty);
        set => SetValue(ContrastLevelProperty, value);
    }

    public ColorSpec.SpecVersion SpecVersion
    {
        get => GetValue(SpecVersionProperty);
        set => SetValue(SpecVersionProperty, value);
    }

    public abstract DynamicScheme CreateScheme(ThemeVariant theme);

    protected Hct ResolveSeedHct()
    {
        var color = Color ?? throw new InvalidOperationException("SchemeProvider requires Color to be set.");

        return Hct.From(ArgbColor.FromAvaloniaColor(color));
    }

    protected double ResolveContrast()
    {
        return ContrastLevel ?? 0;
    }

    protected ColorSpec.SpecVersion ResolveSpecVersion()
    {
        return SpecVersion;
    }

    protected DynamicScheme.Platform ResolvePlatform()
    {
        return DynamicScheme.DefaultPlatform;
    }

    private void OnPropertyChangedInternal(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ColorProperty || e.Property == ContrastLevelProperty || e.Property == SpecVersionProperty)
            OnSchemeChanged();
    }

    protected void OnSchemeChanged()
    {
        SchemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
