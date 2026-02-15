using System.Globalization;
using Avalonia.Controls;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia;

using DynamicColors;
using global::Avalonia;
using global::Avalonia.Media;
using global::Avalonia.Metadata;
using global::Avalonia.Threading;

public class DynamicMaterialColorScheme : ResourceProvider
{
    public readonly static DirectProperty<DynamicMaterialColorScheme, ISchemeProvider?> SchemeProperty =
        AvaloniaProperty.RegisterDirect(
            name: nameof(Scheme),
            getter: o => o.Scheme,
            setter: (DynamicMaterialColorScheme o, ISchemeProvider? v) => o.Scheme = v
        );

    public readonly static DirectProperty<DynamicMaterialColorScheme, ExtendedPaletteCollection> ExtraPalettesProperty =
        AvaloniaProperty.RegisterDirect<DynamicMaterialColorScheme, ExtendedPaletteCollection>(
            name: nameof(ExtendedPalettes),
            getter: o => o.ExtendedPalettes
        );

    private ISchemeProvider? _attachedSchemeProvider;
    private DynamicScheme? _lightColorScheme;
    private DynamicScheme? _darkColorScheme;

    private Dictionary<string, TonalPaletteScheme> _extendedPaletteSchemesCache = new(StringComparer.OrdinalIgnoreCase);

    private int _schemesDirtyFlag;
    private int _updateScheduled;

    public DynamicMaterialColorScheme()
    {
        ExtendedPalettes = new ExtendedPaletteCollection();
        ExtendedPalettes.PalettesChanged += OnExtraPalettesChanged;
    }

    public ISchemeProvider? Scheme
    {
        get;
        set
        {
            if (SetAndRaise(property: SchemeProperty, field: ref field, value: value))
            {
                AttachSchemeProvider(value);
                InvalidateSchemes();
            }
        }
    }

    [Content]
    public ExtendedPaletteCollection ExtendedPalettes { get; }

    override public bool HasResources => _lightColorScheme is {} && _darkColorScheme is {};

    override public bool TryGetResource(
        object key,
        ThemeVariant? theme,
        out object? value
    )
    {
        value = null;

        if (key is not string resourceKey)
            return false;

        var variant = theme ?? ThemeVariant.Light;

        value = ResolveSystemColor(resourceKey: resourceKey, variant: variant) ?? ResolveExtraPalette(resourceKey: resourceKey, variant: variant);

        return value is {};
    }

    private Color? ResolveSystemColor(string resourceKey, ThemeVariant variant) => resourceKey switch
    {
        "md.sys.color.background"     => GetColor(color: MaterialDynamicColors.Background, variant: variant),
        "md.sys.color.on-background"  => GetColor(color: MaterialDynamicColors.OnBackground, variant: variant),
        "md.sys.color.surface"        => GetColor(color: MaterialDynamicColors.Surface, variant: variant),
        "md.sys.color.surface-dim"    => GetColor(color: MaterialDynamicColors.SurfaceDim, variant: variant),
        "md.sys.color.surface-bright" => GetColor(color: MaterialDynamicColors.SurfaceBright, variant: variant),
        "md.sys.color.surface-container-lowest" => GetColor(
            color: MaterialDynamicColors.SurfaceContainerLowest,
            variant: variant
        ),
        "md.sys.color.surface-container-low" => GetColor(
            color: MaterialDynamicColors.SurfaceContainerLow,
            variant: variant
        ),
        "md.sys.color.surface-container" => GetColor(
            color: MaterialDynamicColors.SurfaceContainer,
            variant: variant
        ),
        "md.sys.color.surface-container-high" => GetColor(
            color: MaterialDynamicColors.SurfaceContainerHigh,
            variant: variant
        ),
        "md.sys.color.surface-container-highest" => GetColor(
            color: MaterialDynamicColors.SurfaceContainerHighest,
            variant: variant
        ),
        "md.sys.color.on-surface" => GetColor(color: MaterialDynamicColors.OnSurface, variant: variant),
        "md.sys.color.surface-variant" => GetColor(
            color: MaterialDynamicColors.SurfaceVariant,
            variant: variant
        ),
        "md.sys.color.on-surface-variant" => GetColor(
            color: MaterialDynamicColors.OnSurfaceVariant,
            variant: variant
        ),
        "md.sys.color.inverse-surface" => GetColor(
            color: MaterialDynamicColors.InverseSurface,
            variant: variant
        ),
        "md.sys.color.inverse-on-surface" => GetColor(
            color: MaterialDynamicColors.InverseOnSurface,
            variant: variant
        ),
        "md.sys.color.outline" => GetColor(color: MaterialDynamicColors.Outline, variant: variant),
        "md.sys.color.outline-variant" => GetColor(
            color: MaterialDynamicColors.OutlineVariant,
            variant: variant
        ),
        "md.sys.color.shadow"       => GetColor(color: MaterialDynamicColors.Shadow, variant: variant),
        "md.sys.color.scrim"        => GetColor(color: MaterialDynamicColors.Scrim, variant: variant),
        "md.sys.color.surface-tint" => GetColor(color: MaterialDynamicColors.SurfaceTint, variant: variant),
        "md.sys.color.primary"      => GetColor(color: MaterialDynamicColors.Primary, variant: variant),
        "md.sys.color.on-primary"   => GetColor(color: MaterialDynamicColors.OnPrimary, variant: variant),
        "md.sys.color.primary-container" => GetColor(
            color: MaterialDynamicColors.PrimaryContainer,
            variant: variant
        ),
        "md.sys.color.on-primary-container" => GetColor(
            color: MaterialDynamicColors.OnPrimaryContainer,
            variant: variant
        ),
        "md.sys.color.inverse-primary" => GetColor(
            color: MaterialDynamicColors.InversePrimary,
            variant: variant
        ),
        "md.sys.color.secondary"    => GetColor(color: MaterialDynamicColors.Secondary, variant: variant),
        "md.sys.color.on-secondary" => GetColor(color: MaterialDynamicColors.OnSecondary, variant: variant),
        "md.sys.color.secondary-container" => GetColor(
            color: MaterialDynamicColors.SecondaryContainer,
            variant: variant
        ),
        "md.sys.color.on-secondary-container" => GetColor(
            color: MaterialDynamicColors.OnSecondaryContainer,
            variant: variant
        ),
        "md.sys.color.tertiary"    => GetColor(color: MaterialDynamicColors.Tertiary, variant: variant),
        "md.sys.color.on-tertiary" => GetColor(color: MaterialDynamicColors.OnTertiary, variant: variant),
        "md.sys.color.tertiary-container" => GetColor(
            color: MaterialDynamicColors.TertiaryContainer,
            variant: variant
        ),
        "md.sys.color.on-tertiary-container" => GetColor(
            color: MaterialDynamicColors.OnTertiaryContainer,
            variant: variant
        ),
        "md.sys.color.error"    => GetColor(color: MaterialDynamicColors.Error, variant: variant),
        "md.sys.color.on-error" => GetColor(color: MaterialDynamicColors.OnError, variant: variant),
        "md.sys.color.error-container" => GetColor(
            color: MaterialDynamicColors.ErrorContainer,
            variant: variant
        ),
        "md.sys.color.on-error-container" => GetColor(
            color: MaterialDynamicColors.OnErrorContainer,
            variant: variant
        ),
        "md.sys.color.primary-fixed" => GetColor(color: MaterialDynamicColors.PrimaryFixed, variant: variant),
        "md.sys.color.primary-fixed-dim" => GetColor(
            color: MaterialDynamicColors.PrimaryFixedDim,
            variant: variant
        ),
        "md.sys.color.on-primary-fixed" => GetColor(
            color: MaterialDynamicColors.OnPrimaryFixed,
            variant: variant
        ),
        "md.sys.color.on-primary-fixed-variant" => GetColor(
            color: MaterialDynamicColors.OnPrimaryFixedVariant,
            variant: variant
        ),
        "md.sys.color.secondary-fixed" => GetColor(
            color: MaterialDynamicColors.SecondaryFixed,
            variant: variant
        ),
        "md.sys.color.secondary-fixed-dim" => GetColor(
            color: MaterialDynamicColors.SecondaryFixedDim,
            variant: variant
        ),
        "md.sys.color.on-secondary-fixed" => GetColor(
            color: MaterialDynamicColors.OnSecondaryFixed,
            variant: variant
        ),
        "md.sys.color.on-secondary-fixed-variant" => GetColor(
            color: MaterialDynamicColors.OnSecondaryFixedVariant,
            variant: variant
        ),
        "md.sys.color.tertiary-fixed" => GetColor(color: MaterialDynamicColors.TertiaryFixed, variant: variant),
        "md.sys.color.tertiary-fixed-dim" => GetColor(
            color: MaterialDynamicColors.TertiaryFixedDim,
            variant: variant
        ),
        "md.sys.color.on-tertiary-fixed" => GetColor(
            color: MaterialDynamicColors.OnTertiaryFixed,
            variant: variant
        ),
        "md.sys.color.on-tertiary-fixed-variant" => GetColor(
            color: MaterialDynamicColors.OnTertiaryFixedVariant,
            variant: variant
        ),
        var _ => null,
    };

    private Color? GetColor(DynamicColor color, ThemeVariant variant)
    {
        var scheme = variant == ThemeVariant.Dark ? _darkColorScheme : _lightColorScheme;

        return scheme?.GetHct(color)
           .ToAvaloniaColor();
    }

    private Color? ResolveExtraPalette(string resourceKey, ThemeVariant variant)
    {
        const string sysPrefix = "md.sys.color.ex.";

        if (resourceKey.StartsWith(value: sysPrefix, comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            var remainder = resourceKey[sysPrefix.Length..];
            var segments = remainder.Split(
                separator: '.',
                options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            if (segments.Length != 1)
                return null;

            var ans = segments[0] switch
            {
                ['o', 'n', '-', .. var a, '-', 'c', 'o', 'n', 't', 'a', 'i', 'n', 'e', 'r',] => Fetch(key: new string(a), type: 3),
                ['o', 'n', '-', .. var a,]                                                   => Fetch(key: new string(a), type: 1),
                [.. var a, '-', 'c', 'o', 'n', 't', 'a', 'i', 'n', 'e', 'r',]                => Fetch(key: new string(a), type: 2),
                [.. var a,]                                                                  => Fetch(key: new string(a), type: 0),
                var _                                                                        => null,
            };

            return ans;
        }

        return null;

        Color? Fetch(
            string key,
            int type
        )
        {
            if (!_extendedPaletteSchemesCache.TryGetValue(key: key, value: out var palette))
                return null;

            return type switch
            {
                0     => GetColor(color: palette.Extended, variant: variant),
                1     => GetColor(color: palette.OnExtended, variant: variant),
                2     => GetColor(color: palette.ExtendedContainer, variant: variant),
                3     => GetColor(color: palette.OnExtendedContainer, variant: variant),
                var _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }


    private void AttachSchemeProvider(ISchemeProvider? provider)
    {
        if (_attachedSchemeProvider is {})
            _attachedSchemeProvider.SchemeChanged -= OnSchemeProviderChanged;

        _attachedSchemeProvider = provider;

        if (_attachedSchemeProvider is {})
            _attachedSchemeProvider.SchemeChanged += OnSchemeProviderChanged;
    }

    private void OnSchemeProviderChanged(object? sender, EventArgs e) => InvalidateSchemes();

    private void OnExtraPalettesChanged(object? sender, EventArgs e) => InvalidateSchemes();

    private void InvalidateSchemes()
    {
        Interlocked.Exchange(location1: ref _schemesDirtyFlag, value: 1);

        if (Interlocked.Exchange(location1: ref _updateScheduled, value: 1) == 1)
            return;

        Dispatcher.UIThread.Post(
            action: () =>
            {
                Interlocked.Exchange(location1: ref _updateScheduled, value: 0);

                if (Interlocked.Exchange(location1: ref _schemesDirtyFlag, value: 0) == 0)
                    return;
                try
                {
                    UpdateSchemes();
                }
                catch (Exception) {}
            },
            priority: DispatcherPriority.Background
        );
    }

    private void UpdateSchemes()
    {
        var provider =
            Scheme
         ?? throw new InvalidOperationException("DynamicMaterialColorScheme requires a Scheme to be set.");

        _lightColorScheme = provider.CreateScheme(ThemeVariant.Light);
        _darkColorScheme = provider.CreateScheme(ThemeVariant.Dark);
        _extendedPaletteSchemesCache = BuildExtraPaletteCache();

        RaiseResourcesChanged();
    }

    private Dictionary<string, TonalPaletteScheme> BuildExtraPaletteCache()
    {
        var result = new Dictionary<string, TonalPaletteScheme>(StringComparer.OrdinalIgnoreCase);
        foreach ((var key, var palette) in ExtendedPalettes)
        {
            var stringKey = Convert.ToString(value: key, provider: CultureInfo.InvariantCulture);

            if (stringKey is null)
                continue;

            try
            {
                var tonalPalette = palette.GeneratePalette(_lightColorScheme?.SourceColorArgb.ToAvaloniaColor());
                result[stringKey] = tonalPalette;
            }
            catch (Exception)
            {
                // Ignore invalid palettes but keep processing others.
            }
        }

        return result;
    }


}
