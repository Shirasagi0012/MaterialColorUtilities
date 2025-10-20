using MaterialColorUtilities.Contrast;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Scheme;
using MaterialColorUtilities.Utils;
using Xunit;
using DynColor = MaterialColorUtilities.DynamicColors.DynamicColor;
using DynScheme = MaterialColorUtilities.DynamicColors.DynamicScheme;

namespace MaterialColorUtilities.Tests.Scheme;

using DynamicColors;

/// <summary>
/// Tests that validate the correctness of color schemes across various conditions.
/// This is a simplified version of the Dart scheme_correctness_test.dart
/// </summary>
public class SchemeCorrectnessTests
{
    private const double ContrastTolerance = 0.05;
    private const double DeltaTolerance = 0.5;

    private static DynScheme SchemeFromVariant(
        Variant variant,
        Hct sourceColorHct,
        bool isDark,
        double contrastLevel)
    {
        return variant switch
        {
            Variant.Content => new SchemeContent(sourceColorHct, isDark, contrastLevel),
            Variant.Expressive => new SchemeExpressive(sourceColorHct, isDark, contrastLevel),
            Variant.Fidelity => new SchemeFidelity(sourceColorHct, isDark, contrastLevel),
            Variant.FruitSalad => new SchemeFruitSalad(sourceColorHct, isDark, contrastLevel),
            Variant.Monochrome => new SchemeMonochrome(sourceColorHct, isDark, contrastLevel),
            Variant.Neutral => new SchemeNeutral(sourceColorHct, isDark, contrastLevel),
            Variant.Rainbow => new SchemeRainbow(sourceColorHct, isDark, contrastLevel),
            Variant.TonalSpot => new SchemeTonalSpot(sourceColorHct, isDark, contrastLevel),
            Variant.Vibrant => new SchemeVibrant(sourceColorHct, isDark, contrastLevel),
            _ => throw new ArgumentException($"Unknown variant: {variant}")
        };
    }

    private static void AssertContrast(
        DynColor foreground,
        DynColor background,
        ContrastCurve contrastCurve,
        DynScheme scheme)
    {
        var foregroundColor = foreground.GetHct(scheme);
        var backgroundColor = background.GetHct(scheme);
        var actualContrast = MaterialColorUtilities.Contrast.Contrast.RatioOfTones(
            foregroundColor.Tone,
            backgroundColor.Tone);
        var desiredContrast = contrastCurve.Get(scheme.ContrastLevel);

        if (desiredContrast <= 4.5)
        {
            // A requirement of <= 4.5 must be met (with tolerance)
            Assert.True(
                actualContrast >= desiredContrast - ContrastTolerance,
                $"{foreground.Name} should have contrast at least {desiredContrast} " +
                $"against {background.Name}, but has {actualContrast}");
        }
        else
        {
            // Higher contrast requirements
            Assert.True(
                actualContrast >= 4.5 - ContrastTolerance,
                $"{foreground.Name} should have contrast at least 4.5 " +
                $"against {background.Name}, but has {actualContrast}");

            if (foregroundColor.Tone != 100.0 && foregroundColor.Tone != 0.0)
            {
                Assert.True(
                    actualContrast >= desiredContrast - ContrastTolerance,
                    $"{foreground.Name} should have contrast at least {desiredContrast} " +
                    $"against {background.Name}, but has {actualContrast}");
            }
        }
    }

    [Theory]
    [InlineData(0, 0.0, 0xFF0000FF, false)] // Content
    [InlineData(4, 0.5, 0xFF00FF00, true)]  // Monochrome
    [InlineData(2, -1.0, 0xFFFF0000, false)] // TonalSpot
    [InlineData(5, 1.0, 0xFFFFFF00, true)]   // Fidelity
    public void TextOnPrimaryHasAdequateContrast(
        int variantIndex,
        double contrastLevel,
        uint sourceColorValue,
        bool isDark)
    {
        var variant = (Variant)variantIndex;
        var sourceColor = Hct.From(new ArgbColor(sourceColorValue));
        var scheme = SchemeFromVariant(variant, sourceColor, isDark, contrastLevel);

        // Test basic contrast requirement for text on primary
        AssertContrast(
            MaterialDynamicColors.OnPrimary,
            MaterialDynamicColors.Primary,
            new ContrastCurve(4.5, 7, 11, 21),
            scheme);
    }

    [Theory]
    [InlineData(0, 0.0)]  // Content
    [InlineData(4, 0.5)]  // Monochrome
    [InlineData(2, 1.0)]  // TonalSpot
    public void OnBackgroundHasAdequateContrast(int variantIndex, double contrastLevel)
    {
        var variant = (Variant)variantIndex;
        var sourceColor = Hct.From(new ArgbColor(0xFF0000FF));

        foreach (var isDark in new[] { false, true })
        {
            var scheme = SchemeFromVariant(variant, sourceColor, isDark, contrastLevel);

            AssertContrast(
                MaterialDynamicColors.OnBackground,
                MaterialDynamicColors.Background,
                new ContrastCurve(3, 3, 4.5, 7),
                scheme);
        }
    }

    [Theory]
    [InlineData(0)]  // Content
    [InlineData(4)]  // Monochrome
    [InlineData(2)]  // TonalSpot
    [InlineData(3)]  // Vibrant
    public void PrimaryFixedDimIsDarkerThanPrimaryFixed(int variantIndex)
    {
        var variant = (Variant)variantIndex;
        var sourceColor = Hct.From(new ArgbColor(0xFF0000FF));

        foreach (var isDark in new[] { false, true })
        {
            var scheme = SchemeFromVariant(variant, sourceColor, isDark, 0.0);

            var primaryFixed = MaterialDynamicColors.PrimaryFixed.GetHct(scheme).Tone;
            var primaryFixedDim = MaterialDynamicColors.PrimaryFixedDim.GetHct(scheme).Tone;

            // PrimaryFixedDim should be at least 10 tones darker
            var actualDelta = primaryFixed - primaryFixedDim;
            Assert.True(
                actualDelta >= 10 - DeltaTolerance,
                $"PrimaryFixedDim should be 10 darker than PrimaryFixed, " +
                $"but they have tones {primaryFixedDim} and {primaryFixed}, respectively");
        }
    }

    [Fact]
    public void AllVariantsProduceValidSchemes()
    {
        var sourceColors = new[]
        {
            new ArgbColor(0xFF0000FF),
            new ArgbColor(0xFF00FF00),
            new ArgbColor(0xFFFFFF00),
            new ArgbColor(0xFFFF0000)
        };

        var contrastLevels = new[] { -1.0, 0.0, 0.5, 1.0 };

        foreach (var variant in Enum.GetValues<Variant>())
        {
            foreach (var contrastLevel in contrastLevels)
            {
                foreach (var sourceColor in sourceColors)
                {
                    foreach (var isDark in new[] { false, true })
                    {
                        var sourceHct = Hct.From(sourceColor);
                        var scheme = SchemeFromVariant(variant, sourceHct, isDark, contrastLevel);

                        // Basic sanity check - scheme should be created successfully
                        Assert.NotNull(scheme);
                        Assert.Equal(variant, scheme.Variant);
                        Assert.Equal(isDark, scheme.IsDark);
                        Assert.Equal(contrastLevel, scheme.ContrastLevel);

                        // Ensure key colors can be retrieved
                        var primary = MaterialDynamicColors.Primary.GetHct(scheme);
                        Assert.NotNull(primary);
                        Assert.True(primary.Tone >= 0 && primary.Tone <= 100);
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(0, 0.0, false)]  // Content
    [InlineData(4, 0.5, true)]   // Monochrome
    [InlineData(2, 1.0, false)]  // TonalSpot
    public void SurfaceColorsFormOrderedProgression(
        int variantIndex,
        double contrastLevel,
        bool isDark)
    {
        var variant = (Variant)variantIndex;
        var sourceColor = Hct.From(new ArgbColor(0xFF0000FF));
        var scheme = SchemeFromVariant(variant, sourceColor, isDark, contrastLevel);

        // Get surface tones
        var surfaceDim = MaterialDynamicColors.SurfaceDim.GetHct(scheme).Tone;
        var surface = MaterialDynamicColors.Surface.GetHct(scheme).Tone;
        var surfaceBright = MaterialDynamicColors.SurfaceBright.GetHct(scheme).Tone;

        if (isDark)
        {
            // In dark mode: surfaceDim < surface < surfaceBright
            Assert.True(surfaceDim <= surface,
                $"In dark mode, surfaceDim ({surfaceDim}) should be <= surface ({surface})");
            Assert.True(surface <= surfaceBright,
                $"In dark mode, surface ({surface}) should be <= surfaceBright ({surfaceBright})");
        }
        else
        {
            // In light mode: surfaceBright > surface > surfaceDim
            Assert.True(surfaceBright >= surface,
                $"In light mode, surfaceBright ({surfaceBright}) should be >= surface ({surface})");
            Assert.True(surface >= surfaceDim,
                $"In light mode, surface ({surface}) should be >= surfaceDim ({surfaceDim})");
        }
    }
}
