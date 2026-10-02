using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Avalonia;

/// <summary>Provides generated Material colors through Avalonia's native resource system.</summary>
/// <remarks>
/// Add one instance to a host's Resources.MergedDictionaries. Values are always Color;
/// native DynamicResource performs conversion for IBrush properties. A provider has one owner,
/// but its ColorScheme may be shared. Input changes and resource access belong on the UI thread.
/// </remarks>
public sealed class MaterialColorResources : ResourceProvider
{
    public static readonly StyledProperty<ColorScheme?> SchemeProperty =
        AvaloniaProperty.Register<MaterialColorResources, ColorScheme?>(nameof(Scheme));

    private EventHandler? _schemeChangedHandler;
    private SchemeSnapshot? _snapshot;
    private int _generation;
    private bool _isBuilding;

    [Content]
    public ColorScheme? Scheme
    {
        get => GetValue(SchemeProperty);
        set => SetValue(SchemeProperty, value);
    }

    public override bool HasResources => Scheme is not null;

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        value = null;
        // Check the key BEFORE reading inputs or generating a scheme. DynamicResource seed
        // bindings may search this provider too, and must be able to continue outward.
        if (!IsSupportedKey(key) || Scheme is not { Color: not null } scheme)
            return false;

        // Publish only a fully built generation. A failed build never exposes partial or stale data.
        var snapshot = _snapshot ?? CreateSnapshot(scheme);
        return snapshot.TryGetColor(key, ColorScheme.IsDark(theme), out value);
    }

    private SchemeSnapshot CreateSnapshot(ColorScheme scheme)
    {
        if (_isBuilding)
            throw new InvalidOperationException("A ColorScheme must not recursively resolve its own generated resources.");

        var generation = _generation;
        _isBuilding = true;
        try
        {
            var snapshot = new SchemeSnapshot(scheme);
            // ColorScheme is extensible. An override that mutates its inputs during generation
            // must not publish a mixed light/dark snapshot or overwrite a newer invalidation.
            if (generation != _generation)
                throw new InvalidOperationException("ColorScheme inputs changed while its resources were being generated.");
            return _snapshot = snapshot;
        }
        finally
        {
            _isBuilding = false;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != SchemeProperty)
            return;

        if (change.OldValue is ColorScheme previous && _schemeChangedHandler is { } oldHandler)
            previous.SchemeChanged -= oldHandler;
        _schemeChangedHandler = change.NewValue is ColorScheme next ? SubscribeWeakly(next, this) : null;
        Invalidate();
    }

    private static EventHandler SubscribeWeakly(ColorScheme scheme, MaterialColorResources provider)
    {
        var weakProvider = new WeakReference<MaterialColorResources>(provider);
        EventHandler? handler = null;
        handler = (sender, _) =>
        {
            if (weakProvider.TryGetTarget(out var target))
                target.Invalidate();
            else if (sender is ColorScheme source)
                source.SchemeChanged -= handler;
        };
        scheme.SchemeChanged += handler;
        return handler;
    }

    private void Invalidate()
    {
        _generation++;
        _snapshot = null;
        // Notify even when Scheme/Color has just become null so native consumers lose old values.
        RaiseResourcesChanged();
    }

    private static bool IsSupportedKey(object key) => key switch
    {
        SysColorToken token => Enum.IsDefined(token),
        RefPaletteKey palette => palette.IsValid,
        CustomColorKey custom => custom.IsValid,
        _ => false
    };

    private sealed class SchemeSnapshot
    {
        private readonly DynamicScheme _light;
        private readonly DynamicScheme _dark;
        private readonly IReadOnlyDictionary<string, TonalPalette> _customPalettes;
        // Only successful, validated keys are retained. The key space is bounded by standard
        // roles, 101 tones per palette, and the custom names declared in this generation.
        private readonly Dictionary<object, Color> _lightColors = new();
        private readonly Dictionary<object, Color> _darkColors = new();

        public SchemeSnapshot(ColorScheme scheme)
        {
            _light = scheme.CreateScheme(ThemeVariant.Light);
            _dark = scheme.CreateScheme(ThemeVariant.Dark);
            _customPalettes = scheme.CreateCustomPalettes();
        }

        public bool TryGetColor(object key, bool isDark, out object? value)
        {
            var cache = isDark ? _darkColors : _lightColors;
            if (cache.TryGetValue(key, out var color))
            {
                value = color;
                return true;
            }

            var scheme = isDark ? _dark : _light;
            switch (key)
            {
                case SysColorToken token:
                    color = Resolve(token, scheme).ToAvaloniaColor();
                    break;
                case RefPaletteKey paletteKey:
                    TonalPalette palette;
                    if (paletteKey.Palette == RefPaletteToken.Custom)
                    {
                        if (!_customPalettes.TryGetValue(paletteKey.CustomName!, out palette!))
                        {
                            value = null;
                            return false;
                        }
                    }
                    else
                    {
                        palette = paletteKey.Palette switch
                        {
                            RefPaletteToken.Primary => scheme.PrimaryPalette,
                            RefPaletteToken.Secondary => scheme.SecondaryPalette,
                            RefPaletteToken.Tertiary => scheme.TertiaryPalette,
                            RefPaletteToken.Neutral => scheme.NeutralPalette,
                            RefPaletteToken.NeutralVariant => scheme.NeutralVariantPalette,
                            RefPaletteToken.Error => scheme.ErrorPalette,
                            _ => throw new InvalidOperationException("The palette key was not validated.")
                        };
                    }
                    color = palette.Get(paletteKey.Tone).ToAvaloniaColor();
                    break;
                case CustomColorKey custom:
                    if (!_customPalettes.TryGetValue(custom.Name, out var customPalette))
                    {
                        value = null;
                        return false;
                    }
                    // Retain the existing fixed-tone contract, independent of contrast.
                    var tone = (custom.Role, isDark) switch
                    {
                        (CustomColorRole.Color, false) => 40,
                        (CustomColorRole.OnColor, false) => 100,
                        (CustomColorRole.Container, false) => 90,
                        (CustomColorRole.OnContainer, false) => 10,
                        (CustomColorRole.Color, true) => 80,
                        (CustomColorRole.OnColor, true) => 20,
                        (CustomColorRole.Container, true) => 30,
                        (CustomColorRole.OnContainer, true) => 90,
                        _ => throw new InvalidOperationException("The custom color key was not validated.")
                    };
                    color = customPalette.Get(tone).ToAvaloniaColor();
                    break;
                default:
                    value = null;
                    return false;
            }

            cache.Add(key, color);
            value = color;
            return true;
        }

        private static ArgbColor Resolve(SysColorToken token, DynamicScheme scheme)
        {
            return token switch
            {
                SysColorToken.Background => scheme.Background,
                SysColorToken.OnBackground => scheme.OnBackground,
                SysColorToken.Surface => scheme.Surface,
                SysColorToken.SurfaceDim => scheme.SurfaceDim,
                SysColorToken.SurfaceBright => scheme.SurfaceBright,
                SysColorToken.SurfaceContainerLowest => scheme.SurfaceContainerLowest,
                SysColorToken.SurfaceContainerLow => scheme.SurfaceContainerLow,
                SysColorToken.SurfaceContainer => scheme.SurfaceContainer,
                SysColorToken.SurfaceContainerHigh => scheme.SurfaceContainerHigh,
                SysColorToken.SurfaceContainerHighest => scheme.SurfaceContainerHighest,
                SysColorToken.OnSurface => scheme.OnSurface,
                SysColorToken.SurfaceVariant => scheme.SurfaceVariant,
                SysColorToken.OnSurfaceVariant => scheme.OnSurfaceVariant,
                SysColorToken.InverseSurface => scheme.InverseSurface,
                SysColorToken.InverseOnSurface => scheme.InverseOnSurface,
                SysColorToken.Outline => scheme.Outline,
                SysColorToken.OutlineVariant => scheme.OutlineVariant,
                SysColorToken.Shadow => scheme.Shadow,
                SysColorToken.Scrim => scheme.Scrim,
                SysColorToken.SurfaceTint => scheme.SurfaceTint,
                SysColorToken.Primary => scheme.Primary,
                SysColorToken.OnPrimary => scheme.OnPrimary,
                SysColorToken.PrimaryContainer => scheme.PrimaryContainer,
                SysColorToken.OnPrimaryContainer => scheme.OnPrimaryContainer,
                SysColorToken.InversePrimary => scheme.InversePrimary,
                SysColorToken.Secondary => scheme.Secondary,
                SysColorToken.OnSecondary => scheme.OnSecondary,
                SysColorToken.SecondaryContainer => scheme.SecondaryContainer,
                SysColorToken.OnSecondaryContainer => scheme.OnSecondaryContainer,
                SysColorToken.Tertiary => scheme.Tertiary,
                SysColorToken.OnTertiary => scheme.OnTertiary,
                SysColorToken.TertiaryContainer => scheme.TertiaryContainer,
                SysColorToken.OnTertiaryContainer => scheme.OnTertiaryContainer,
                SysColorToken.Error => scheme.Error,
                SysColorToken.OnError => scheme.OnError,
                SysColorToken.ErrorContainer => scheme.ErrorContainer,
                SysColorToken.OnErrorContainer => scheme.OnErrorContainer,
                SysColorToken.PrimaryFixed => scheme.PrimaryFixed,
                SysColorToken.PrimaryFixedDim => scheme.PrimaryFixedDim,
                SysColorToken.OnPrimaryFixed => scheme.OnPrimaryFixed,
                SysColorToken.OnPrimaryFixedVariant => scheme.OnPrimaryFixedVariant,
                SysColorToken.SecondaryFixed => scheme.SecondaryFixed,
                SysColorToken.SecondaryFixedDim => scheme.SecondaryFixedDim,
                SysColorToken.OnSecondaryFixed => scheme.OnSecondaryFixed,
                SysColorToken.OnSecondaryFixedVariant => scheme.OnSecondaryFixedVariant,
                SysColorToken.TertiaryFixed => scheme.TertiaryFixed,
                SysColorToken.TertiaryFixedDim => scheme.TertiaryFixedDim,
                SysColorToken.OnTertiaryFixed => scheme.OnTertiaryFixed,
                SysColorToken.OnTertiaryFixedVariant => scheme.OnTertiaryFixedVariant,
                _ => throw new ArgumentOutOfRangeException(nameof(token), token, null)
            };
        }
    }
}
