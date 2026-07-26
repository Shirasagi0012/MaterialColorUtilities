using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia.Data;
using Blend = MaterialColorUtilities.Blend.Blend;

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

    private readonly AvaloniaList<CustomColor> _customColors = [];

    protected ColorScheme()
    {
        PropertyChanged += OnPropertyChangedInternal;
        _customColors.CollectionChanged += OnCustomColorsChanged;
    }

    protected ColorScheme(BindingBase binding) : this()
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

    /// <summary>
    /// Named key colors contributed on top of the generated scheme. Declared inline in XAML:
    /// <code>
    /// &lt;mcu:TonalSpotScheme Color="#6750A4"&gt;
    ///     &lt;mcu:CustomColor Name="Brand" Color="#FF5722" /&gt;
    /// &lt;/mcu:TonalSpotScheme&gt;
    /// </code>
    /// </summary>
    [Content]
    public AvaloniaList<CustomColor> CustomColors => _customColors;

    public abstract DynamicScheme CreateScheme(ThemeVariant theme);

    /// <summary>
    /// Builds one tonal palette per named custom color, harmonizing toward the scheme's source
    /// color where requested. Entries without a name or seed color are skipped; a duplicate name
    /// keeps the last declaration.
    /// </summary>
    internal IReadOnlyDictionary<string, TonalPalette> CreateCustomPalettes()
    {
        if (_customColors.Count == 0)
            return EmptyCustomPalettes;

        var seed = Color is { } sourceColor ? ArgbColor.FromAvaloniaColor(sourceColor) : (ArgbColor?)null;
        var palettes = new Dictionary<string, TonalPalette>(StringComparer.OrdinalIgnoreCase);

        foreach (var custom in _customColors)
        {
            if (string.IsNullOrEmpty(custom.Name) || custom.Color is not { } customColor)
                continue;

            var argb = ArgbColor.FromAvaloniaColor(customColor);
            if (custom.Harmonize && seed is { } source)
                argb = Blend.Harmonize(argb, source);

            palettes[custom.Name] = new TonalPalette(Hct.From(argb));
        }

        return palettes;
    }

    private static readonly IReadOnlyDictionary<string, TonalPalette> EmptyCustomPalettes =
        new Dictionary<string, TonalPalette>(StringComparer.OrdinalIgnoreCase);

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

    private void OnCustomColorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is { } removed)
            foreach (CustomColor custom in removed)
                custom.PropertyChanged -= OnCustomColorPropertyChanged;

        if (e.NewItems is { } added)
            foreach (CustomColor custom in added)
                custom.PropertyChanged += OnCustomColorPropertyChanged;

        OnSchemeChanged();
    }

    private void OnCustomColorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == CustomColor.NameProperty || e.Property == CustomColor.ColorProperty ||
            e.Property == CustomColor.HarmonizeProperty)
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
