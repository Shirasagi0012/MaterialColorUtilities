using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;

public abstract class SchemeProviderBase : AvaloniaObject, ISchemeProvider
{
    public readonly static StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<SchemeProviderBase, Color?>(nameof(Color));

    public readonly static StyledProperty<double?> ContrastLevelProperty =
        AvaloniaProperty.Register<SchemeProviderBase, double?>(nameof(ContrastLevel));

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
        this[!ColorProperty] = new CustomColor(color).ColorBinding;
    }

    protected SchemeProviderBase(string colorString) : this()
    {
        if (global::Avalonia.Media.Color.TryParse(s: colorString, color: out var color))
            this[!ColorProperty] = new CustomColor(color).ColorBinding;
        else
            throw new FormatException($"'{colorString}' is not a valid color string.");
    }

    // TODO: Support for StaticResource, which calls ctor with Object? parameter

    public event EventHandler? SchemeChanged;

    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(property: ColorProperty, value: value);
    }
    
    public double? ContrastLevel
    {
        get => GetValue(ContrastLevelProperty);
        set => SetValue(property: ContrastLevelProperty, value: value);
    }

    public abstract DynamicScheme CreateScheme(ThemeVariant theme);

    protected Hct ResolveSeedHct()
    {
        var color = Color ?? throw new InvalidOperationException("SchemeProvider requires Color to be set.");

        return Hct.From(ArgbExtensions.FromAvaloniaColor(color));
    }

    protected double ResolveContrast() => ContrastLevel ?? 0;

    private void OnPropertyChangedInternal(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ColorProperty || e.Property == ContrastLevelProperty)
            OnSchemeChanged();
    }

    private void OnColorProviderChanged(object? sender, EventArgs e) => OnSchemeChanged();

    private void OnSchemeChanged()
    {
        SchemeChanged?.Invoke(sender: this, e: EventArgs.Empty);
    }
}
