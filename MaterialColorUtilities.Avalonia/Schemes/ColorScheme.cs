using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;

public abstract class ColorScheme : AvaloniaObject
{
    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<ColorScheme, Color?>(nameof(Color));

    public static readonly StyledProperty<double?> ContrastLevelProperty =
        AvaloniaProperty.Register<ColorScheme, double?>(nameof(ContrastLevel));

    public static readonly StyledProperty<ColorSpec.SpecVersion> SpecVersionProperty =
        AvaloniaProperty.Register<ColorScheme, ColorSpec.SpecVersion>(
            nameof(SpecVersion),
            DynamicScheme.DefaultSpecVersion
        );

    public static readonly StyledProperty<DynamicScheme.Platform> PlatformProperty =
        AvaloniaProperty.Register<ColorScheme, DynamicScheme.Platform>(nameof(Platform), DynamicScheme.DefaultPlatform);

    protected ColorScheme()
    {
        PropertyChanged += OnPropertyChangedInternal;
    }

    protected ColorScheme(IBinding binding) : this()
    {
        this[!ColorProperty] = binding;
    }

    protected ColorScheme(Color color) : this()
    {
        Color = color;
    }

    protected ColorScheme(string colorString) : this()
    {
        if (global::Avalonia.Media.Color.TryParse(colorString, out var color))
            Color = color;
        else
            throw new FormatException($"'{colorString}' is not a valid color string.");
    }

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

    public DynamicScheme.Platform Platform
    {
        get => GetValue(PlatformProperty);
        set => SetValue(PlatformProperty, value);
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
        return Platform;
    }

    private void OnPropertyChangedInternal(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ColorProperty || e.Property == ContrastLevelProperty || e.Property == SpecVersionProperty ||
            e.Property == PlatformProperty)
            OnSchemeChanged();
    }

    protected void OnSchemeChanged()
    {
        SchemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public static bool IsDark(ThemeVariant variant)
    {
        if (variant == ThemeVariant.Dark)
            return true;

        if (variant == ThemeVariant.Light)
            return false;

        var inherited = variant.InheritVariant;
        while (inherited is { })
        {
            if (inherited == ThemeVariant.Dark)
                return true;

            if (inherited == ThemeVariant.Light)
                return false;

            inherited = inherited.InheritVariant;
        }

        return false;
    }
}
