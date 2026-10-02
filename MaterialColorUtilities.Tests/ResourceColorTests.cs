using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.DynamicColors;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Scheme;
using MaterialColorUtilities.Utils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class ResourceColorTests
{
    private static readonly Color Seed = Color.Parse("#6750A4");
    private static readonly byte[] Tones = [0, 1, 40, 60, 99, 100];
    private static readonly string[] SchemeNames =
    [
        "TonalSpot", "Vibrant", "Expressive", "Content", "Fidelity", "FruitSalad", "Monochrome", "Neutral", "Rainbow"
    ];

    public static IEnumerable<object[]> SchemeConfigurations()
    {
        foreach (var name in SchemeNames)
        foreach (var spec in Enum.GetValues<ColorSpec.SpecVersion>())
        foreach (var platform in Enum.GetValues<DynamicScheme.Platform>())
        foreach (var contrast in new[] { -1d, 0d, 1d })
            yield return [name, spec, platform, contrast];
    }

    [AvaloniaTheory]
    [MemberData(nameof(SchemeConfigurations))]
    public void EveryStandardRoleMatchesCoreForBothThemes(
        string name, ColorSpec.SpecVersion spec, DynamicScheme.Platform platform, double contrast)
    {
        var scheme = CreateInput(name);
        scheme.SpecVersion = spec;
        scheme.Platform = platform;
        scheme.ContrastLevel = contrast;
        var resources = new MaterialColorResources { Scheme = scheme };

        foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            var core = CreateCore(name, Seed, theme == ThemeVariant.Dark, contrast, spec, platform);
            AssertRoles(resources, core, theme);
            AssertPalettes(resources, core, theme);
        }
    }

    [AvaloniaTheory]
    [InlineData(DynamicScheme.Platform.Phone, -1d)]
    [InlineData(DynamicScheme.Platform.Phone, 0d)]
    [InlineData(DynamicScheme.Platform.Phone, 1d)]
    [InlineData(DynamicScheme.Platform.Watch, -1d)]
    [InlineData(DynamicScheme.Platform.Watch, 0d)]
    [InlineData(DynamicScheme.Platform.Watch, 1d)]
    public void CmfSingleAndDualSeedsMatchCore(DynamicScheme.Platform platform, double contrast)
    {
        var scheme = new CmfScheme(Seed)
        {
            SpecVersion = ColorSpec.SpecVersion.Spec2026, Platform = platform, ContrastLevel = contrast
        };
        var resources = new MaterialColorResources { Scheme = scheme };
        foreach (var secondary in new Color?[] { null, Colors.Orange, Colors.Teal, null })
        {
            scheme.SecondaryColor = secondary;
            foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                var source = Hct.From(ArgbColor.FromAvaloniaColor(Seed));
                Hct[] seeds = secondary is { } color ? [source, Hct.From(ArgbColor.FromAvaloniaColor(color))] : [source];
                var core = new SchemeCmf(seeds, theme == ThemeVariant.Dark, contrast,
                    ColorSpec.SpecVersion.Spec2026, platform);
                AssertRoles(resources, core, theme);
                AssertPalettes(resources, core, theme);
            }
        }
    }

    [AvaloniaFact]
    public void Spec2025PhonePaletteUsesTheRequestedTheme()
    {
        var scheme = new TonalSpotScheme(Seed)
        {
            SpecVersion = ColorSpec.SpecVersion.Spec2025, Platform = DynamicScheme.Platform.Phone
        };
        var resources = new MaterialColorResources { Scheme = scheme };
        var key = new RefPaletteKey(RefPaletteToken.Primary, 60);
        var light = scheme.CreateScheme(ThemeVariant.Light).PrimaryPalette.Get(60).ToAvaloniaColor();
        var dark = scheme.CreateScheme(ThemeVariant.Dark).PrimaryPalette.Get(60).ToAvaloniaColor();

        Assert.NotEqual(light, dark); // Guards against a vacuous theme-regression fixture.
        Assert.Equal(light, Read(resources, key, ThemeVariant.Light));
        Assert.Equal(dark, Read(resources, key, ThemeVariant.Dark));
    }

    [AvaloniaFact]
    public void DefaultPaletteKeyIsPrimaryToneZero()
    {
        var resources = new MaterialColorResources { Scheme = new TonalSpotScheme(Seed) };
        Assert.Equal(Colors.Black, Read(resources, default(RefPaletteKey), ThemeVariant.Light));
        Assert.Equal(Colors.Black, Read(resources, default(RefPaletteKey), ThemeVariant.Dark));
    }

    [AvaloniaFact]
    public void NullSchemeOrSeedMissesAndCanRecoverWithoutReplacingTheProvider()
    {
        var resources = new MaterialColorResources();
        AssertMiss(resources, SysColorToken.Primary);
        var scheme = new TonalSpotScheme();
        scheme.CustomColors.Add(new CustomColor("Brand", Colors.Red));
        resources.Scheme = scheme;
        object[] keys =
        [
            SysColorToken.Primary, new RefPaletteKey(RefPaletteToken.Primary, 60),
            new CustomColorKey("Brand", CustomColorRole.Color), new RefPaletteKey("Brand", 60)
        ];
        foreach (var key in keys)
            AssertMiss(resources, key);

        scheme.Color = Seed;
        foreach (var key in keys)
            _ = Read(resources, key, ThemeVariant.Dark);
        scheme.Color = null;
        foreach (var key in keys)
            AssertMiss(resources, key);
        scheme.Color = Colors.Teal;
        Assert.Equal(scheme.CreateScheme(ThemeVariant.Dark).Primary.ToAvaloniaColor(),
            Read(resources, SysColorToken.Primary, ThemeVariant.Dark));
        resources.Scheme = null;
        foreach (var key in keys)
            AssertMiss(resources, key);
    }

    [AvaloniaFact]
    public void UnrecognizedKeysAreRejectedBeforeAnyGeneration()
    {
        var scheme = new CountingScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        object[] keys =
        [
            new object(), "Primary", "md.sys.color.primary", "SystemAccentColor", (int)SysColorToken.Primary,
            RefPaletteToken.Primary, (SysColorToken)(-1), (SysColorToken)49, (SysColorToken)int.MaxValue,
            default(CustomColorKey)
        ];
        foreach (var key in keys)
            AssertMiss(resources, key);
        Assert.Equal(0, scheme.CreationCount);

        // A broken coherent configuration must not throw while another provider's key is being looked up.
        resources.Scheme = new CmfScheme(Seed);
        foreach (var key in keys)
            AssertMiss(resources, key);
    }

    [AvaloniaFact]
    public void AllMutableSchemeInputsInvalidateAlreadyReadRolesAndPalettes()
    {
        var scheme = new TonalSpotScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        Action[] edits =
        [
            () => { }, () => scheme.Color = Colors.Teal, () => scheme.ContrastLevel = 1,
            () => scheme.ContrastLevel = -1, () => scheme.ContrastLevel = null,
            () => scheme.SpecVersion = ColorSpec.SpecVersion.Spec2025,
            () => scheme.Platform = DynamicScheme.Platform.Watch,
            () => scheme.SpecVersion = ColorSpec.SpecVersion.Spec2026,
            () => scheme.Platform = DynamicScheme.Platform.Phone
        ];
        foreach (var edit in edits)
        {
            edit();
            foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                var core = CreateCore("TonalSpot", scheme.Color!.Value, theme == ThemeVariant.Dark,
                    scheme.ContrastLevel ?? 0, scheme.SpecVersion, scheme.Platform);
                AssertRoles(resources, core, theme);
                AssertPalettes(resources, core, theme);
            }
        }
    }

    [AvaloniaFact]
    public void ReplacingTheSchemeCannotReadThePreviousGeneration()
    {
        var oldScheme = new TonalSpotScheme(Seed);
        var newScheme = new ExpressiveScheme(Colors.Teal);
        var resources = new MaterialColorResources { Scheme = oldScheme };
        _ = Read(resources, SysColorToken.Primary, ThemeVariant.Dark);
        resources.Scheme = newScheme;
        var expected = newScheme.CreateScheme(ThemeVariant.Dark).Primary.ToAvaloniaColor();
        Assert.Equal(expected, Read(resources, SysColorToken.Primary, ThemeVariant.Dark));
        oldScheme.Color = Colors.Red;
        Assert.Equal(expected, Read(resources, SysColorToken.Primary, ThemeVariant.Dark));
    }

    [AvaloniaTheory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-1.0001)]
    [InlineData(1.0001)]
    public void ContrastMustBeFiniteAndWithinTheSupportedRange(double invalid)
    {
        var scheme = new TonalSpotScheme(Seed) { ContrastLevel = 0.5 };
        Assert.ThrowsAny<ArgumentException>(() => scheme.ContrastLevel = invalid);
        Assert.Equal(0.5, scheme.ContrastLevel);
    }

    [AvaloniaFact]
    public void UndefinedSpecAndPlatformAreRejectedAtTheInputBoundary()
    {
        var scheme = new TonalSpotScheme(Seed);
        foreach (var invalid in new[] { -1, 3, int.MaxValue })
            Assert.ThrowsAny<ArgumentException>(() => scheme.SpecVersion = (ColorSpec.SpecVersion)invalid);
        foreach (var invalid in new[] { -1, 2, int.MaxValue })
            Assert.ThrowsAny<ArgumentException>(() => scheme.Platform = (DynamicScheme.Platform)invalid);
        Assert.Equal(DynamicScheme.DefaultSpecVersion, scheme.SpecVersion);
        Assert.Equal(DynamicScheme.DefaultPlatform, scheme.Platform);
    }

    [AvaloniaFact]
    public void NullContrastRetainsTheZeroContrastDefault()
    {
        var scheme = new TonalSpotScheme(Seed);
        Assert.Null(scheme.ContrastLevel);
        var resources = new MaterialColorResources { Scheme = scheme };
        var expected = new SchemeTonalSpot(Hct.From(ArgbColor.FromAvaloniaColor(Seed)), false, 0);
        AssertRoles(resources, expected, ThemeVariant.Light);
    }

    [AvaloniaFact]
    public void CmfValidationWaitsForACompleteSnapshotAndCanRecoverAfterAnError()
    {
        var scheme = new CmfScheme();
        var resources = new MaterialColorResources { Scheme = scheme };
        scheme.SecondaryColor = Colors.Teal; // Property assignment itself must allow XAML initialization order.
        AssertMiss(resources, SysColorToken.Primary);
        scheme.Color = Seed;
        Assert.Throws<InvalidOperationException>(() => resources.TryGetResource(SysColorToken.Primary, ThemeVariant.Light, out _));
        scheme.SpecVersion = ColorSpec.SpecVersion.Spec2026;
        Assert.Equal(scheme.CreateScheme(ThemeVariant.Light).Primary.ToAvaloniaColor(),
            Read(resources, SysColorToken.Primary, ThemeVariant.Light));
        scheme.SpecVersion = ColorSpec.SpecVersion.Spec2025;
        Assert.Throws<InvalidOperationException>(() => resources.TryGetResource(SysColorToken.Primary, ThemeVariant.Dark, out _));
        scheme.SpecVersion = ColorSpec.SpecVersion.Spec2026;
        Assert.Equal(scheme.CreateScheme(ThemeVariant.Dark).Primary.ToAvaloniaColor(),
            Read(resources, SysColorToken.Primary, ThemeVariant.Dark));
    }

    [AvaloniaFact]
    public void RepeatedLookupsReuseOnlyTheCurrentLightAndDarkGeneration()
    {
        var scheme = new CountingScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        for (var repeat = 0; repeat < 20; repeat++)
        {
            _ = Read(resources, SysColorToken.Primary, ThemeVariant.Light);
            _ = Read(resources, SysColorToken.Surface, ThemeVariant.Dark);
            _ = Read(resources, new RefPaletteKey(RefPaletteToken.Primary, 60), ThemeVariant.Dark);
            AssertMiss(resources, new CustomColorKey($"Missing {repeat}", CustomColorRole.Color));
        }
        Assert.Equal(2, scheme.CreationCount);
        scheme.Color = Colors.Teal;
        _ = Read(resources, SysColorToken.Primary, ThemeVariant.Light);
        _ = Read(resources, SysColorToken.Primary, ThemeVariant.Dark);
        Assert.Equal(4, scheme.CreationCount);
    }

    [AvaloniaFact]
    public void HistoricalGenerationsAndUnknownNamesAreNotRetainedInCaches()
    {
        var scheme = new CountingScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        var unknownNames = ExerciseGenerationsAndMissingNames(resources, scheme);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.InRange(scheme.Generated.Count(reference => reference.TryGetTarget(out _)), 0, 2);
        Assert.All(unknownNames, reference => Assert.False(reference.TryGetTarget(out _)));
        GC.KeepAlive(resources);
        GC.KeepAlive(scheme);
    }

    [AvaloniaFact]
    public void AGenerationFailureDoesNotPublishAPartialOrStaleSnapshot()
    {
        var scheme = new CountingScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        _ = Read(resources, SysColorToken.Primary, ThemeVariant.Light);
        scheme.Color = Colors.Teal;
        scheme.FailDarkGeneration = true;
        Assert.Throws<InvalidOperationException>(() => resources.TryGetResource(SysColorToken.Primary, ThemeVariant.Light, out _));
        Assert.Throws<InvalidOperationException>(() => resources.TryGetResource(SysColorToken.Primary, ThemeVariant.Dark, out _));
        scheme.FailDarkGeneration = false;
        Assert.Equal(new SchemeTonalSpot(Hct.From(ArgbColor.FromAvaloniaColor(Colors.Teal)), false, 0).Primary.ToAvaloniaColor(),
            Read(resources, SysColorToken.Primary, ThemeVariant.Light));
    }

    [AvaloniaFact]
    public void InputMutationDuringGenerationRejectsTheMixedSnapshotAndRetriesCleanly()
    {
        var scheme = new CountingScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        scheme.BeforeGeneration = theme =>
        {
            if (theme != ThemeVariant.Dark)
                return;
            scheme.BeforeGeneration = null;
            scheme.Color = Colors.Teal;
        };

        Assert.Throws<InvalidOperationException>(() => resources.TryGetResource(SysColorToken.Primary, ThemeVariant.Light, out _));
        foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            var core = new SchemeTonalSpot(Hct.From(ArgbColor.FromAvaloniaColor(Colors.Teal)), theme == ThemeVariant.Dark, 0);
            AssertRoles(resources, core, theme);
        }
    }

    [AvaloniaFact]
    public void RecursiveGeneratedResourceLookupFailsClearlyAndTheBuildGuardResets()
    {
        var scheme = new CountingScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        scheme.BeforeGeneration = theme => resources.TryGetResource(SysColorToken.Primary, theme, out _);
        Assert.Throws<InvalidOperationException>(() => resources.TryGetResource(SysColorToken.Primary, ThemeVariant.Light, out _));
        scheme.BeforeGeneration = null;
        Assert.Equal(new SchemeTonalSpot(Hct.From(ArgbColor.FromAvaloniaColor(Seed)), false, 0).Primary.ToAvaloniaColor(),
            Read(resources, SysColorToken.Primary, ThemeVariant.Light));
    }

    [AvaloniaFact]
    public void UnrelatedResourceLookupDuringGenerationCanContinueOutward()
    {
        var scheme = new CountingScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        scheme.BeforeGeneration = _ => AssertMiss(resources, "SystemAccentColor");
        _ = Read(resources, SysColorToken.Primary, ThemeVariant.Light);
        Assert.Equal(2, scheme.CreationCount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static List<WeakReference<string>> ExerciseGenerationsAndMissingNames(
        MaterialColorResources resources, CountingScheme scheme)
    {
        for (var generation = 1; generation <= 20; generation++)
        {
            scheme.Color = Color.FromRgb((byte)(generation * 11), (byte)(generation * 7), (byte)(generation * 3));
            _ = Read(resources, SysColorToken.Primary, ThemeVariant.Light);
            _ = Read(resources, SysColorToken.Primary, ThemeVariant.Dark);
        }
        var names = new List<WeakReference<string>>();
        for (var i = 0; i < 100; i++)
        {
            var name = $"Unknown custom color {i}";
            names.Add(new WeakReference<string>(name));
            AssertMiss(resources, new CustomColorKey(name, CustomColorRole.Color));
            AssertMiss(resources, new RefPaletteKey(name, 40));
        }
        return names;
    }

    private static ColorScheme CreateInput(string name) => name switch
    {
        "TonalSpot" => new TonalSpotScheme(Seed), "Vibrant" => new VibrantScheme(Seed),
        "Expressive" => new ExpressiveScheme(Seed), "Content" => new ContentScheme(Seed),
        "Fidelity" => new FidelityScheme(Seed), "FruitSalad" => new FruitSaladScheme(Seed),
        "Monochrome" => new MonochromeScheme(Seed), "Neutral" => new NeutralScheme(Seed),
        "Rainbow" => new RainbowScheme(Seed), _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    private static DynamicScheme CreateCore(string name, Color seed, bool dark, double contrast,
        ColorSpec.SpecVersion spec, DynamicScheme.Platform platform)
    {
        var hct = Hct.From(ArgbColor.FromAvaloniaColor(seed));
        return name switch
        {
            "TonalSpot" => new SchemeTonalSpot(hct, dark, contrast, spec, platform),
            "Vibrant" => new SchemeVibrant(hct, dark, contrast, spec, platform),
            "Expressive" => new SchemeExpressive(hct, dark, contrast, spec, platform),
            "Content" => new SchemeContent(hct, dark, contrast, spec, platform),
            "Fidelity" => new SchemeFidelity(hct, dark, contrast, spec, platform),
            "FruitSalad" => new SchemeFruitSalad(hct, dark, contrast, spec, platform),
            "Monochrome" => new SchemeMonochrome(hct, dark, contrast, spec, platform),
            "Neutral" => new SchemeNeutral(hct, dark, contrast, spec, platform),
            "Rainbow" => new SchemeRainbow(hct, dark, contrast, spec, platform),
            _ => throw new ArgumentOutOfRangeException(nameof(name))
        };
    }

    private static void AssertRoles(MaterialColorResources resources, DynamicScheme core, ThemeVariant theme)
    {
        foreach (var token in Enum.GetValues<SysColorToken>())
            Assert.Equal(CoreRole(core, token).ToAvaloniaColor(), Read(resources, token, theme));
    }

    private static void AssertPalettes(MaterialColorResources resources, DynamicScheme core, ThemeVariant theme)
    {
        foreach (var palette in Enum.GetValues<RefPaletteToken>().Where(token => token != RefPaletteToken.Custom))
        foreach (var tone in Tones)
            Assert.Equal(CorePalette(core, palette).Get(tone).ToAvaloniaColor(),
                Read(resources, new RefPaletteKey(palette, tone), theme));
    }

    private static TonalPalette CorePalette(DynamicScheme core, RefPaletteToken palette) => palette switch
    {
        RefPaletteToken.Primary => core.PrimaryPalette, RefPaletteToken.Secondary => core.SecondaryPalette,
        RefPaletteToken.Tertiary => core.TertiaryPalette, RefPaletteToken.Neutral => core.NeutralPalette,
        RefPaletteToken.NeutralVariant => core.NeutralVariantPalette, RefPaletteToken.Error => core.ErrorPalette,
        _ => throw new ArgumentOutOfRangeException(nameof(palette))
    };

    private static ArgbColor CoreRole(DynamicScheme core, SysColorToken token) => token switch
    {
        SysColorToken.Background => core.Background,
        SysColorToken.OnBackground => core.OnBackground,
        SysColorToken.Surface => core.Surface,
        SysColorToken.SurfaceDim => core.SurfaceDim,
        SysColorToken.SurfaceBright => core.SurfaceBright,
        SysColorToken.SurfaceContainerLowest => core.SurfaceContainerLowest,
        SysColorToken.SurfaceContainerLow => core.SurfaceContainerLow,
        SysColorToken.SurfaceContainer => core.SurfaceContainer,
        SysColorToken.SurfaceContainerHigh => core.SurfaceContainerHigh,
        SysColorToken.SurfaceContainerHighest => core.SurfaceContainerHighest,
        SysColorToken.OnSurface => core.OnSurface,
        SysColorToken.SurfaceVariant => core.SurfaceVariant,
        SysColorToken.OnSurfaceVariant => core.OnSurfaceVariant,
        SysColorToken.InverseSurface => core.InverseSurface,
        SysColorToken.InverseOnSurface => core.InverseOnSurface,
        SysColorToken.Outline => core.Outline,
        SysColorToken.OutlineVariant => core.OutlineVariant,
        SysColorToken.Shadow => core.Shadow,
        SysColorToken.Scrim => core.Scrim,
        SysColorToken.SurfaceTint => core.SurfaceTint,
        SysColorToken.Primary => core.Primary,
        SysColorToken.OnPrimary => core.OnPrimary,
        SysColorToken.PrimaryContainer => core.PrimaryContainer,
        SysColorToken.OnPrimaryContainer => core.OnPrimaryContainer,
        SysColorToken.InversePrimary => core.InversePrimary,
        SysColorToken.Secondary => core.Secondary,
        SysColorToken.OnSecondary => core.OnSecondary,
        SysColorToken.SecondaryContainer => core.SecondaryContainer,
        SysColorToken.OnSecondaryContainer => core.OnSecondaryContainer,
        SysColorToken.Tertiary => core.Tertiary,
        SysColorToken.OnTertiary => core.OnTertiary,
        SysColorToken.TertiaryContainer => core.TertiaryContainer,
        SysColorToken.OnTertiaryContainer => core.OnTertiaryContainer,
        SysColorToken.Error => core.Error,
        SysColorToken.OnError => core.OnError,
        SysColorToken.ErrorContainer => core.ErrorContainer,
        SysColorToken.OnErrorContainer => core.OnErrorContainer,
        SysColorToken.PrimaryFixed => core.PrimaryFixed,
        SysColorToken.PrimaryFixedDim => core.PrimaryFixedDim,
        SysColorToken.OnPrimaryFixed => core.OnPrimaryFixed,
        SysColorToken.OnPrimaryFixedVariant => core.OnPrimaryFixedVariant,
        SysColorToken.SecondaryFixed => core.SecondaryFixed,
        SysColorToken.SecondaryFixedDim => core.SecondaryFixedDim,
        SysColorToken.OnSecondaryFixed => core.OnSecondaryFixed,
        SysColorToken.OnSecondaryFixedVariant => core.OnSecondaryFixedVariant,
        SysColorToken.TertiaryFixed => core.TertiaryFixed,
        SysColorToken.TertiaryFixedDim => core.TertiaryFixedDim,
        SysColorToken.OnTertiaryFixed => core.OnTertiaryFixed,
        SysColorToken.OnTertiaryFixedVariant => core.OnTertiaryFixedVariant,
        _ => throw new ArgumentOutOfRangeException(nameof(token))
    };

    private static Color Read(MaterialColorResources resources, object key, ThemeVariant? theme)
    {
        Assert.True(resources.TryGetResource(key, theme, out var value), $"Missing {key} for {theme}");
        return Assert.IsType<Color>(value);
    }

    private static void AssertMiss(MaterialColorResources resources, object key)
    {
        Assert.False(resources.TryGetResource(key, ThemeVariant.Light, out var value));
        Assert.Null(value);
    }

    private sealed class CountingScheme(Color color) : ColorScheme(color)
    {
        public int CreationCount { get; private set; }
        public bool FailDarkGeneration { get; set; }
        public Action<ThemeVariant>? BeforeGeneration { get; set; }
        public List<WeakReference<DynamicScheme>> Generated { get; } = [];

        public override DynamicScheme CreateScheme(ThemeVariant theme)
        {
            CreationCount++;
            BeforeGeneration?.Invoke(theme);
            if (FailDarkGeneration && IsDark(theme))
                throw new InvalidOperationException("Deliberate generation failure.");
            var scheme = new SchemeTonalSpot(ResolveSeedHct(), IsDark(theme), ResolveContrast(),
                ResolveSpecVersion(), ResolvePlatform());
            Generated.Add(new WeakReference<DynamicScheme>(scheme));
            return scheme;
        }
    }
}
