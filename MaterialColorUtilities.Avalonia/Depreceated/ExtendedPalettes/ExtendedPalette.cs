namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia;
using global::Avalonia.Media;
using HCT;
using Palettes;
using Utils;

public class ExtendedPalette : AvaloniaObject
{
    public event EventHandler? ColorChanged;

    public readonly static StyledProperty<Color> ColorProperty = AvaloniaProperty.Register<ExtendedPalette, Color>(nameof(Color));

    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(property: ColorProperty, value: value);
    }


    public readonly static StyledProperty<bool> HarmonizedProperty = AvaloniaProperty.Register<ExtendedPalette, bool>(nameof(Harmonized));

    public bool Harmonized
    {
        get => GetValue(HarmonizedProperty);
        set => SetValue(property: HarmonizedProperty, value: value);
    }

    private void OnColorChanged()
    {
        ColorChanged?.Invoke(sender: this, e: EventArgs.Empty);
    }

    public ExtendedPalette()
    {
        PropertyChanged += (sender, args) =>
        {
            if (args.Property == ColorProperty || args.Property == HarmonizedProperty)
                OnColorChanged();
        };
    }

    internal TonalPaletteScheme GeneratePalette(Color? sourceColor = null)
    {
        var hct = Hct.FromAvaloniaColor(Color);
        if (Harmonized == true && sourceColor is {} source)
            Blend.Blend.Harmonize(designColor: ArgbColor.FromAvaloniaColor(Color), sourceColor: ArgbColor.FromAvaloniaColor(source));

        return new TonalPaletteScheme(new TonalPalette(hct));
    }

}

public class TonalPaletteScheme
{
    public DynamicColor Extended { get; }

    public DynamicColor OnExtended { get; }

    public DynamicColor ExtendedContainer { get; }

    public DynamicColor OnExtendedContainer { get; }

    public TonalPaletteScheme(TonalPalette palette)
    {
        Extended = DynamicColor.FromPalette(
            name: "extended",
            palette: s => palette,
            tone: s =>
            {
                if (MaterialDynamicColors.IsMonochrome(s))
                    return s.IsDark ? 100 : 0;

                return s.IsDark ? 80 : 40;
            },
            isBackground: true,
            background: MaterialDynamicColors.HighestSurface,
            contrastCurve: new ContrastCurve(low: 3, normal: 4.5, medium: 7, high: 7),
            toneDeltaPair: s => new ToneDeltaPair(
                roleA: ExtendedContainer,
                roleB: Extended,
                delta: 10,
                polarity: TonePolarity.Nearer,
                stayTogether: false
            )
        );
        OnExtended = DynamicColor.FromPalette(
            name: "on_extended",
            palette: s => palette,
            tone: s =>
            {
                if (MaterialDynamicColors.IsMonochrome(s))
                    return s.IsDark ? 10 : 90;

                return s.IsDark ? 20 : 100;
            },
            background: s => Extended,
            contrastCurve: new ContrastCurve(low: 4.5, normal: 7, medium: 11, high: 21)
        );
        ExtendedContainer = DynamicColor.FromPalette(
            name: "extended_container",
            palette: s => palette,
            tone: s =>
            {
                if (MaterialDynamicColors.IsFidelity(s))
                    return s.SourceColorHct.Tone;
                if (MaterialDynamicColors.IsMonochrome(s))
                    return s.IsDark ? 85 : 25;

                return s.IsDark ? 30 : 90;
            },
            isBackground: true,
            background: MaterialDynamicColors.HighestSurface,
            contrastCurve: new ContrastCurve(low: 1, normal: 1, medium: 3, high: 4.5),
            toneDeltaPair: s => new ToneDeltaPair(
                roleA: ExtendedContainer,
                roleB: Extended,
                delta: 10,
                polarity: TonePolarity.Nearer,
                stayTogether: false
            )
        );
        OnExtendedContainer = DynamicColor.FromPalette(
            name: "on_extended_container",
            palette: s => palette,
            tone: s =>
            {
                if (MaterialDynamicColors.IsFidelity(s))
                    return DynamicColor.ForegroundTone(bgTone: ExtendedContainer.Tone(s), ratio: 4.5);
                if (MaterialDynamicColors.IsMonochrome(s))
                    return s.IsDark ? 0 : 100;

                return s.IsDark ? 90 : 30;
            },
            background: s => ExtendedContainer,
            contrastCurve: new ContrastCurve(low: 3, normal: 4.5, medium: 7, high: 11)
        );
    }
}
