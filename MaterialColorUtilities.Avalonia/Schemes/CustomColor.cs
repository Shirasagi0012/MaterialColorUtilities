using Avalonia;
using Avalonia.Media;

namespace MaterialColorUtilities.Avalonia;

/// <summary>
/// A named key color contributed to a <see cref="ColorScheme"/> in addition to the standard
/// primary / secondary / tertiary / error key colors.
/// <para>
/// Each custom color produces a full tonal palette, addressable as a reference palette via
/// <c>RefPaletteToken.Custom</c>, and four system roles via <c>SysColorToken.Custom</c>,
/// <c>OnCustom</c>, <c>CustomContainer</c> and <c>OnCustomContainer</c>. All of them are keyed by
/// <see cref="Name"/>.
/// </para>
/// </summary>
public class CustomColor : AvaloniaObject
{
    public static readonly StyledProperty<string?> NameProperty =
        AvaloniaProperty.Register<CustomColor, string?>(nameof(Name));

    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<CustomColor, Color?>(nameof(Color));

    public static readonly StyledProperty<bool> HarmonizeProperty =
        AvaloniaProperty.Register<CustomColor, bool>(nameof(Harmonize), true);

    public CustomColor()
    {
    }

    public CustomColor(string name, Color color)
    {
        Name = name;
        Color = color;
    }

    /// <summary>
    /// The key this color is addressed by from token markup, e.g. <c>{MdSysColor Custom, CustomKey=Brand}</c>.
    /// Lookup is case-insensitive.
    /// </summary>
    public string? Name
    {
        get => GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    /// <summary>
    /// The seed color this palette is generated from.
    /// </summary>
    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// When <c>true</c> (the default), the seed color's hue is rotated up to 15 degrees toward the
    /// scheme's source color so it sits comfortably alongside the generated roles. Set to
    /// <c>false</c> to keep a brand color exact.
    /// </summary>
    public bool Harmonize
    {
        get => GetValue(HarmonizeProperty);
        set => SetValue(HarmonizeProperty, value);
    }
}
