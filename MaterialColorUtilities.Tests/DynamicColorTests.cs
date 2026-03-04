using System.Globalization;
using System.Collections.Concurrent;
using System.Collections;
using System.Reflection;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Scheme;
using MaterialColorUtilities.Utils;
using Xunit;
using DynamicColorType = MaterialColorUtilities.DynamicColors.DynamicColor;
using DynamicSchemeType = MaterialColorUtilities.DynamicColors.DynamicScheme;

namespace MaterialColorUtilities.Tests.DynamicColor;

using DynamicColors;

public class DynamicColorTests
{
    private static readonly MaterialDynamicColors Roles = new();
    private static readonly Hct[] SeedColors =
    {
        Hct.From(new ArgbColor(0xFFFF0000)),
        Hct.From(new ArgbColor(0xFFFFFF00)),
        Hct.From(new ArgbColor(0xFF00FF00)),
        Hct.From(new ArgbColor(0xFF0000FF))
    };

    private static readonly double[] ContrastLevels = { -1.0, -0.5, 0.0, 0.5, 1.0 };

    private record ColorPair(string ForegroundName, string BackgroundName);

    private static readonly IReadOnlyDictionary<string, DynamicColorType> Colors
        = new Dictionary<string, DynamicColorType>
        {
            ["background"] = Roles.Background,
            ["on_background"] = Roles.OnBackground,
            ["surface"] = Roles.Surface,
            ["surface_dim"] = Roles.SurfaceDim,
            ["surface_bright"] = Roles.SurfaceBright,
            ["surface_container_lowest"] = Roles.SurfaceContainerLowest,
            ["surface_container_low"] = Roles.SurfaceContainerLow,
            ["surface_container"] = Roles.SurfaceContainer,
            ["surface_container_high"] = Roles.SurfaceContainerHigh,
            ["surface_container_highest"] = Roles.SurfaceContainerHighest,
            ["on_surface"] = Roles.OnSurface,
            ["surface_variant"] = Roles.SurfaceVariant,
            ["on_surface_variant"] = Roles.OnSurfaceVariant,
            ["inverse_surface"] = Roles.InverseSurface,
            ["inverse_on_surface"] = Roles.InverseOnSurface,
            ["outline"] = Roles.Outline,
            ["outline_variant"] = Roles.OutlineVariant,
            ["shadow"] = Roles.Shadow,
            ["scrim"] = Roles.Scrim,
            ["surface_tint"] = Roles.SurfaceTint,
            ["primary"] = Roles.Primary,
            ["on_primary"] = Roles.OnPrimary,
            ["primary_container"] = Roles.PrimaryContainer,
            ["on_primary_container"] = Roles.OnPrimaryContainer,
            ["inverse_primary"] = Roles.InversePrimary,
            ["secondary"] = Roles.Secondary,
            ["on_secondary"] = Roles.OnSecondary,
            ["secondary_container"] = Roles.SecondaryContainer,
            ["on_secondary_container"] = Roles.OnSecondaryContainer,
            ["tertiary"] = Roles.Tertiary,
            ["on_tertiary"] = Roles.OnTertiary,
            ["tertiary_container"] = Roles.TertiaryContainer,
            ["on_tertiary_container"] = Roles.OnTertiaryContainer,
            ["error"] = Roles.Error,
            ["on_error"] = Roles.OnError,
            ["error_container"] = Roles.ErrorContainer,
            ["on_error_container"] = Roles.OnErrorContainer
        };

    private static readonly IReadOnlyList<ColorPair> TextSurfacePairs = new List<ColorPair>
    {
        new("on_primary", "primary"),
        new("on_primary_container", "primary_container"),
        new("on_secondary", "secondary"),
        new("on_secondary_container", "secondary_container"),
        new("on_tertiary", "tertiary"),
        new("on_tertiary_container", "tertiary_container"),
        new("on_error", "error"),
        new("on_error_container", "error_container"),
        new("on_background", "background"),
        new("on_surface_variant", "surface_bright"),
        new("on_surface_variant", "surface_dim"),
        new("inverse_on_surface", "inverse_surface")
    };

    private static void AssertToneClose(double expected, double actual, string description)
    {
        var difference = Math.Abs(expected - actual);
        Assert.True(
            difference <= 1.0,
            string.Format(
                CultureInfo.InvariantCulture,
                "Expected tone {0:F1} ±1.0 for {1}, but got {2:F4} (diff {3:F4}).",
                expected,
                description,
                actual,
                difference
            )
        );
    }

    private static int GetDynamicColorCacheCount(DynamicColorType color)
    {
        var cacheField = typeof(DynamicColorType).GetField(
            "_hctCacheMap",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        Assert.NotNull(cacheField);
        var cache = Assert.IsAssignableFrom<IDictionary>(cacheField.GetValue(color));
        return cache.Count;
    }

    [Fact]
    public void ValuesAreCorrect()
    {
        Assert.Equal(
            unchecked((int)0xFFFFFFFF),
            Roles
                .OnPrimaryContainer.GetArgb(
                    new SchemeFidelity(Hct.From(new ArgbColor(0xFFFF0000)), false, 0.5)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xFFFFFFFF),
            Roles
                .OnSecondaryContainer.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFF0000FF)), false, 0.5)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xff959b1a),
            Roles
                .OnTertiaryContainer.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFFFFFF00)), true, -0.5)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xFF2F2F3B),
            Roles
                .InverseSurface.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFF0000FF)), false, 0.0)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xffff422f),
            Roles
                .InversePrimary.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFFFF0000)), false, -0.5)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xFF484831),
            Roles
                .OutlineVariant.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFFFFFF00)), true, 0.0)
                )
                .Value
        );
    }

    [Fact]
    public void DynamicSchemesRespectContrastRequirements()
    {
        foreach (var seedColor in SeedColors)
        foreach (var contrastLevel in ContrastLevels)
        foreach (var isDark in new[] { false, true })
        {
            var schemes = new DynamicSchemeType[]
            {
                new SchemeContent(seedColor, isDark, contrastLevel),
                new SchemeMonochrome(seedColor, isDark, contrastLevel),
                new SchemeTonalSpot(seedColor, isDark, contrastLevel),
                new SchemeFidelity(seedColor, isDark, contrastLevel)
            };

            foreach (var scheme in schemes)
            foreach (var pair in TextSurfacePairs)
            {
                var foregroundTone = Colors[pair.ForegroundName].GetHct(scheme).Tone;
                var backgroundTone = Colors[pair.BackgroundName].GetHct(scheme).Tone;
                var contrast = MaterialColorUtilities.Contrast.Contrast.RatioOfTones(
                    foregroundTone,
                    backgroundTone
                );

                var minimumRequirement = contrastLevel >= 0.0 ? 4.5 : 3.0;

                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Contrast {0:F2} is too low between foreground ({1}; {2:F1}) and background ({3}; {4:F1}) for {5} with seed {6:X8}, contrast level {7}, isDark={8}.",
                    contrast,
                    pair.ForegroundName,
                    foregroundTone,
                    pair.BackgroundName,
                    backgroundTone,
                    scheme.GetType().Name,
                    seedColor.Argb.Value,
                    contrastLevel,
                    isDark
                );

                Assert.True(contrast >= minimumRequirement, message);
            }
        }
    }

    [Fact]
    public void DynamicColorCacheHandlesConcurrentEviction()
    {
        var schemes = Enumerable
            .Range(0, 256)
            .Select(index =>
            {
                var seedArgb = unchecked(
                    (uint)(0xFF000000u | ((uint)index * 2654435761u & 0x00FFFFFFu))
                );
                var seed = Hct.From(new ArgbColor(seedArgb));
                var contrast = ContrastLevels[index % ContrastLevels.Length];
                var isDark = (index & 1) == 0;

                return (index % 4) switch
                {
                    0 => (DynamicSchemeType)new SchemeContent(seed, isDark, contrast),
                    1 => new SchemeMonochrome(seed, isDark, contrast),
                    2 => new SchemeTonalSpot(seed, isDark, contrast),
                    _ => new SchemeFidelity(seed, isDark, contrast)
                };
            })
            .ToArray();

        var exceptions = new ConcurrentQueue<Exception>();

        for (var round = 0; round < 24; round++)
        {
            Parallel.ForEach(schemes, scheme =>
            {
                try
                {
                    _ = Roles.Primary.GetHct(scheme);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });
        }

        Assert.True(
            exceptions.IsEmpty,
            $"Concurrent cache access failed with {exceptions.Count} exception(s)."
        );
    }

    [Fact]
    public void WithWithoutOverridesReturnsEquivalentButDistinctInstance()
    {
        var original = DynamicColorType.FromPalette(
            "copy_source",
            s => s.PrimaryPalette,
            _ => 35.0,
            isBackground: true,
            background: _ => Roles.Surface,
            contrastCurve: _ => new ContrastCurve(3.0, 4.5, 7.0, 11.0)
        );
        var copy = original.With();
        var scheme = new SchemeTonalSpot(Hct.From(new ArgbColor(0xFF6750A4)), false, 0.0);

        Assert.NotSame(original, copy);
        Assert.Equal(original.Name, copy.Name);
        Assert.Equal(original.IsBackground, copy.IsBackground);
        Assert.Same(original.Palette, copy.Palette);
        Assert.Same(original.Tone, copy.Tone);
        Assert.Equal(original.GetTone(scheme), copy.GetTone(scheme), 8);
        Assert.Equal(original.GetArgb(scheme).Value, copy.GetArgb(scheme).Value);
    }

    [Fact]
    public void WithOverridesOnlySpecifiedMembers()
    {
        var original = DynamicColorType.FromPalette(
            "copy_source",
            s => s.PrimaryPalette,
            _ => 30.0,
            isBackground: true,
            background: _ => Roles.Surface
        );
        var copy = original.With(name: "copy_override", tone: _ => 80.0);
        var scheme = new SchemeTonalSpot(Hct.From(new ArgbColor(0xFF6750A4)), false, 0.0);

        Assert.Equal("copy_source", original.Name);
        Assert.Equal("copy_override", copy.Name);
        Assert.Equal(30.0, original.GetTone(scheme), 8);
        Assert.Equal(80.0, copy.GetTone(scheme), 8);
        Assert.Same(original.Palette, copy.Palette);
        Assert.Equal(original.IsBackground, copy.IsBackground);
    }

    [Fact]
    public void WithCreatesInstanceWithIndependentCache()
    {
        var original = DynamicColorType.FromPalette(
            "cache_source",
            s => s.PrimaryPalette,
            _ => 40.0
        );
        var scheme = new SchemeTonalSpot(Hct.From(new ArgbColor(0xFF6750A4)), true, 0.5);

        _ = original.GetHct(scheme);
        Assert.Equal(1, GetDynamicColorCacheCount(original));

        var copy = original.With();
        Assert.Equal(0, GetDynamicColorCacheCount(copy));

        _ = copy.GetHct(scheme);
        Assert.Equal(1, GetDynamicColorCacheCount(copy));
        Assert.Equal(1, GetDynamicColorCacheCount(original));
    }

    [Fact]
    public void FixedColorsInNonMonochromeSchemes()
    {
        var scheme = new SchemeTonalSpot(Hct.From(new ArgbColor(0xFFFF0000)), true, 0.0);

        AssertToneClose(
            90.0,
            Roles.PrimaryFixed.GetHct(scheme).Tone,
            nameof(Roles.PrimaryFixed)
        );
        AssertToneClose(
            80.0,
            Roles.PrimaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.PrimaryFixedDim)
        );
        AssertToneClose(
            10.0,
            Roles.OnPrimaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnPrimaryFixed)
        );
        AssertToneClose(
            30.0,
            Roles.OnPrimaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnPrimaryFixedVariant)
        );

        AssertToneClose(
            90.0,
            Roles.SecondaryFixed.GetHct(scheme).Tone,
            nameof(Roles.SecondaryFixed)
        );
        AssertToneClose(
            80.0,
            Roles.SecondaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.SecondaryFixedDim)
        );
        AssertToneClose(
            10.0,
            Roles.OnSecondaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnSecondaryFixed)
        );
        AssertToneClose(
            30.0,
            Roles.OnSecondaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnSecondaryFixedVariant)
        );

        AssertToneClose(
            90.0,
            Roles.TertiaryFixed.GetHct(scheme).Tone,
            nameof(Roles.TertiaryFixed)
        );
        AssertToneClose(
            80.0,
            Roles.TertiaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.TertiaryFixedDim)
        );
        AssertToneClose(
            10.0,
            Roles.OnTertiaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnTertiaryFixed)
        );
        AssertToneClose(
            30.0,
            Roles.OnTertiaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnTertiaryFixedVariant)
        );
    }

    [Fact]
    public void FixedColorsInLightMonochromeSchemes()
    {
        var scheme = new SchemeMonochrome(Hct.From(new ArgbColor(0xFFFF0000)), false, 0.0);

        AssertToneClose(
            40.0,
            Roles.PrimaryFixed.GetHct(scheme).Tone,
            nameof(Roles.PrimaryFixed)
        );
        AssertToneClose(
            30.0,
            Roles.PrimaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.PrimaryFixedDim)
        );
        AssertToneClose(
            100.0,
            Roles.OnPrimaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnPrimaryFixed)
        );
        AssertToneClose(
            90.0,
            Roles.OnPrimaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnPrimaryFixedVariant)
        );

        AssertToneClose(
            80.0,
            Roles.SecondaryFixed.GetHct(scheme).Tone,
            nameof(Roles.SecondaryFixed)
        );
        AssertToneClose(
            70.0,
            Roles.SecondaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.SecondaryFixedDim)
        );
        AssertToneClose(
            10.0,
            Roles.OnSecondaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnSecondaryFixed)
        );
        AssertToneClose(
            25.0,
            Roles.OnSecondaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnSecondaryFixedVariant)
        );

        AssertToneClose(
            40.0,
            Roles.TertiaryFixed.GetHct(scheme).Tone,
            nameof(Roles.TertiaryFixed)
        );
        AssertToneClose(
            30.0,
            Roles.TertiaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.TertiaryFixedDim)
        );
        AssertToneClose(
            100.0,
            Roles.OnTertiaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnTertiaryFixed)
        );
        AssertToneClose(
            90.0,
            Roles.OnTertiaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnTertiaryFixedVariant)
        );
    }

    [Fact]
    public void FixedColorsInDarkMonochromeSchemes()
    {
        var scheme = new SchemeMonochrome(Hct.From(new ArgbColor(0xFFFF0000)), true, 0.0);

        AssertToneClose(
            40.0,
            Roles.PrimaryFixed.GetHct(scheme).Tone,
            nameof(Roles.PrimaryFixed)
        );
        AssertToneClose(
            30.0,
            Roles.PrimaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.PrimaryFixedDim)
        );
        AssertToneClose(
            100.0,
            Roles.OnPrimaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnPrimaryFixed)
        );
        AssertToneClose(
            90.0,
            Roles.OnPrimaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnPrimaryFixedVariant)
        );

        AssertToneClose(
            80.0,
            Roles.SecondaryFixed.GetHct(scheme).Tone,
            nameof(Roles.SecondaryFixed)
        );
        AssertToneClose(
            70.0,
            Roles.SecondaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.SecondaryFixedDim)
        );
        AssertToneClose(
            10.0,
            Roles.OnSecondaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnSecondaryFixed)
        );
        AssertToneClose(
            25.0,
            Roles.OnSecondaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnSecondaryFixedVariant)
        );

        AssertToneClose(
            40.0,
            Roles.TertiaryFixed.GetHct(scheme).Tone,
            nameof(Roles.TertiaryFixed)
        );
        AssertToneClose(
            30.0,
            Roles.TertiaryFixedDim.GetHct(scheme).Tone,
            nameof(Roles.TertiaryFixedDim)
        );
        AssertToneClose(
            100.0,
            Roles.OnTertiaryFixed.GetHct(scheme).Tone,
            nameof(Roles.OnTertiaryFixed)
        );
        AssertToneClose(
            90.0,
            Roles.OnTertiaryFixedVariant.GetHct(scheme).Tone,
            nameof(Roles.OnTertiaryFixedVariant)
        );
    }
}

