using System.Globalization;
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
            ["background"] = MaterialDynamicColors.Background,
            ["on_background"] = MaterialDynamicColors.OnBackground,
            ["surface"] = MaterialDynamicColors.Surface,
            ["surface_dim"] = MaterialDynamicColors.SurfaceDim,
            ["surface_bright"] = MaterialDynamicColors.SurfaceBright,
            ["surface_container_lowest"] = MaterialDynamicColors.SurfaceContainerLowest,
            ["surface_container_low"] = MaterialDynamicColors.SurfaceContainerLow,
            ["surface_container"] = MaterialDynamicColors.SurfaceContainer,
            ["surface_container_high"] = MaterialDynamicColors.SurfaceContainerHigh,
            ["surface_container_highest"] = MaterialDynamicColors.SurfaceContainerHighest,
            ["on_surface"] = MaterialDynamicColors.OnSurface,
            ["surface_variant"] = MaterialDynamicColors.SurfaceVariant,
            ["on_surface_variant"] = MaterialDynamicColors.OnSurfaceVariant,
            ["inverse_surface"] = MaterialDynamicColors.InverseSurface,
            ["inverse_on_surface"] = MaterialDynamicColors.InverseOnSurface,
            ["outline"] = MaterialDynamicColors.Outline,
            ["outline_variant"] = MaterialDynamicColors.OutlineVariant,
            ["shadow"] = MaterialDynamicColors.Shadow,
            ["scrim"] = MaterialDynamicColors.Scrim,
            ["surface_tint"] = MaterialDynamicColors.SurfaceTint,
            ["primary"] = MaterialDynamicColors.Primary,
            ["on_primary"] = MaterialDynamicColors.OnPrimary,
            ["primary_container"] = MaterialDynamicColors.PrimaryContainer,
            ["on_primary_container"] = MaterialDynamicColors.OnPrimaryContainer,
            ["inverse_primary"] = MaterialDynamicColors.InversePrimary,
            ["secondary"] = MaterialDynamicColors.Secondary,
            ["on_secondary"] = MaterialDynamicColors.OnSecondary,
            ["secondary_container"] = MaterialDynamicColors.SecondaryContainer,
            ["on_secondary_container"] = MaterialDynamicColors.OnSecondaryContainer,
            ["tertiary"] = MaterialDynamicColors.Tertiary,
            ["on_tertiary"] = MaterialDynamicColors.OnTertiary,
            ["tertiary_container"] = MaterialDynamicColors.TertiaryContainer,
            ["on_tertiary_container"] = MaterialDynamicColors.OnTertiaryContainer,
            ["error"] = MaterialDynamicColors.Error,
            ["on_error"] = MaterialDynamicColors.OnError,
            ["error_container"] = MaterialDynamicColors.ErrorContainer,
            ["on_error_container"] = MaterialDynamicColors.OnErrorContainer
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

    [Fact]
    public void ValuesAreCorrect()
    {
        Assert.Equal(
            unchecked((int)0xFFFFFFFF),
            MaterialDynamicColors
                .OnPrimaryContainer.GetArgb(
                    new SchemeFidelity(Hct.From(new ArgbColor(0xFFFF0000)), false, 0.5)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xFFFFFFFF),
            MaterialDynamicColors
                .OnSecondaryContainer.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFF0000FF)), false, 0.5)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xff959b1a),
            MaterialDynamicColors
                .OnTertiaryContainer.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFFFFFF00)), true, -0.5)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xFF2F2F3B),
            MaterialDynamicColors
                .InverseSurface.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFF0000FF)), false, 0.0)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xffff422f),
            MaterialDynamicColors
                .InversePrimary.GetArgb(
                    new SchemeContent(Hct.From(new ArgbColor(0xFFFF0000)), false, -0.5)
                )
                .Value
        );

        Assert.Equal(
            unchecked((int)0xFF484831),
            MaterialDynamicColors
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
    public void FixedColorsInNonMonochromeSchemes()
    {
        var scheme = new SchemeTonalSpot(Hct.From(new ArgbColor(0xFFFF0000)), true, 0.0);

        AssertToneClose(
            90.0,
            MaterialDynamicColors.PrimaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.PrimaryFixed)
        );
        AssertToneClose(
            80.0,
            MaterialDynamicColors.PrimaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.PrimaryFixedDim)
        );
        AssertToneClose(
            10.0,
            MaterialDynamicColors.OnPrimaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnPrimaryFixed)
        );
        AssertToneClose(
            30.0,
            MaterialDynamicColors.OnPrimaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnPrimaryFixedVariant)
        );

        AssertToneClose(
            90.0,
            MaterialDynamicColors.SecondaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.SecondaryFixed)
        );
        AssertToneClose(
            80.0,
            MaterialDynamicColors.SecondaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.SecondaryFixedDim)
        );
        AssertToneClose(
            10.0,
            MaterialDynamicColors.OnSecondaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnSecondaryFixed)
        );
        AssertToneClose(
            30.0,
            MaterialDynamicColors.OnSecondaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnSecondaryFixedVariant)
        );

        AssertToneClose(
            90.0,
            MaterialDynamicColors.TertiaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.TertiaryFixed)
        );
        AssertToneClose(
            80.0,
            MaterialDynamicColors.TertiaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.TertiaryFixedDim)
        );
        AssertToneClose(
            10.0,
            MaterialDynamicColors.OnTertiaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnTertiaryFixed)
        );
        AssertToneClose(
            30.0,
            MaterialDynamicColors.OnTertiaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnTertiaryFixedVariant)
        );
    }

    [Fact]
    public void FixedColorsInLightMonochromeSchemes()
    {
        var scheme = new SchemeMonochrome(Hct.From(new ArgbColor(0xFFFF0000)), false, 0.0);

        AssertToneClose(
            40.0,
            MaterialDynamicColors.PrimaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.PrimaryFixed)
        );
        AssertToneClose(
            30.0,
            MaterialDynamicColors.PrimaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.PrimaryFixedDim)
        );
        AssertToneClose(
            100.0,
            MaterialDynamicColors.OnPrimaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnPrimaryFixed)
        );
        AssertToneClose(
            90.0,
            MaterialDynamicColors.OnPrimaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnPrimaryFixedVariant)
        );

        AssertToneClose(
            80.0,
            MaterialDynamicColors.SecondaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.SecondaryFixed)
        );
        AssertToneClose(
            70.0,
            MaterialDynamicColors.SecondaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.SecondaryFixedDim)
        );
        AssertToneClose(
            10.0,
            MaterialDynamicColors.OnSecondaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnSecondaryFixed)
        );
        AssertToneClose(
            25.0,
            MaterialDynamicColors.OnSecondaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnSecondaryFixedVariant)
        );

        AssertToneClose(
            40.0,
            MaterialDynamicColors.TertiaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.TertiaryFixed)
        );
        AssertToneClose(
            30.0,
            MaterialDynamicColors.TertiaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.TertiaryFixedDim)
        );
        AssertToneClose(
            100.0,
            MaterialDynamicColors.OnTertiaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnTertiaryFixed)
        );
        AssertToneClose(
            90.0,
            MaterialDynamicColors.OnTertiaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnTertiaryFixedVariant)
        );
    }

    [Fact]
    public void FixedColorsInDarkMonochromeSchemes()
    {
        var scheme = new SchemeMonochrome(Hct.From(new ArgbColor(0xFFFF0000)), true, 0.0);

        AssertToneClose(
            40.0,
            MaterialDynamicColors.PrimaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.PrimaryFixed)
        );
        AssertToneClose(
            30.0,
            MaterialDynamicColors.PrimaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.PrimaryFixedDim)
        );
        AssertToneClose(
            100.0,
            MaterialDynamicColors.OnPrimaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnPrimaryFixed)
        );
        AssertToneClose(
            90.0,
            MaterialDynamicColors.OnPrimaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnPrimaryFixedVariant)
        );

        AssertToneClose(
            80.0,
            MaterialDynamicColors.SecondaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.SecondaryFixed)
        );
        AssertToneClose(
            70.0,
            MaterialDynamicColors.SecondaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.SecondaryFixedDim)
        );
        AssertToneClose(
            10.0,
            MaterialDynamicColors.OnSecondaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnSecondaryFixed)
        );
        AssertToneClose(
            25.0,
            MaterialDynamicColors.OnSecondaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnSecondaryFixedVariant)
        );

        AssertToneClose(
            40.0,
            MaterialDynamicColors.TertiaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.TertiaryFixed)
        );
        AssertToneClose(
            30.0,
            MaterialDynamicColors.TertiaryFixedDim.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.TertiaryFixedDim)
        );
        AssertToneClose(
            100.0,
            MaterialDynamicColors.OnTertiaryFixed.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnTertiaryFixed)
        );
        AssertToneClose(
            90.0,
            MaterialDynamicColors.OnTertiaryFixedVariant.GetHct(scheme).Tone,
            nameof(MaterialDynamicColors.OnTertiaryFixedVariant)
        );
    }
}