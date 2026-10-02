using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.DynamicColors;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class ThemeVariantResolutionTests
{
    [Fact]
    public void TheBuiltInVariantsResolveToThemselves()
    {
        Assert.True(ColorScheme.IsDark(ThemeVariant.Dark));
        Assert.False(ColorScheme.IsDark(ThemeVariant.Light));
        Assert.False(ColorScheme.IsDark(ThemeVariant.Default));
    }

    [Fact]
    public void ACustomVariantResolvesThroughItsInheritanceChain()
    {
        var dim = new ThemeVariant("Dim", ThemeVariant.Dark);
        var dimmer = new ThemeVariant("Dimmer", dim);
        var bright = new ThemeVariant("Bright", ThemeVariant.Light);
        Assert.True(ColorScheme.IsDark(dim));
        Assert.True(ColorScheme.IsDark(dimmer));
        Assert.False(ColorScheme.IsDark(bright));
        Assert.False(ColorScheme.IsDark(new ThemeVariant("Brighter", bright)));
        Assert.False(ColorScheme.IsDark(new ThemeVariant("Sepia", null)));
    }

    [Fact]
    public void NoVariantIsNotAnError()
    {
        Assert.False(ColorScheme.IsDark(null));
    }

    [AvaloniaFact]
    public void ProviderNormalizesEveryKeyFamilyThroughTheSameThemeChain()
    {
        var scheme = new TonalSpotScheme(Colors.Red) { SpecVersion = ColorSpec.SpecVersion.Spec2025 };
        scheme.CustomColors.Add(new CustomColor("Brand", Colors.Blue));
        var resources = new MaterialColorResources { Scheme = scheme };
        object[] keys =
        [
            SysColorToken.Primary,
            new RefPaletteKey(RefPaletteToken.Primary, 60),
            new RefPaletteKey("Brand", 40),
            new CustomColorKey("Brand", CustomColorRole.Container)
        ];
        ThemeVariant?[] lightVariants =
        [
            null, ThemeVariant.Default, ThemeVariant.Light, new ThemeVariant("Unknown", null),
            new ThemeVariant("LightChild", new ThemeVariant("LightParent", ThemeVariant.Light)),
            new ThemeVariant("UnknownChild", new ThemeVariant("UnknownParent", null))
        ];
        ThemeVariant[] darkVariants =
        [
            ThemeVariant.Dark, new ThemeVariant("DarkChild", ThemeVariant.Dark),
            new ThemeVariant("DarkGrandchild", new ThemeVariant("DarkParent", ThemeVariant.Dark))
        ];

        foreach (var key in keys)
        {
            var light = Read(resources, key, ThemeVariant.Light);
            var dark = Read(resources, key, ThemeVariant.Dark);
            foreach (var theme in lightVariants)
                Assert.Equal(light, Read(resources, key, theme));
            foreach (var theme in darkVariants)
                Assert.Equal(dark, Read(resources, key, theme));
        }
    }

    [AvaloniaFact]
    public void AlternatingThemesDoesNotKeepAGlobalCurrentTheme()
    {
        var scheme = new TonalSpotScheme(Colors.Red) { SpecVersion = ColorSpec.SpecVersion.Spec2025 };
        var resources = new MaterialColorResources { Scheme = scheme };
        var light = scheme.CreateScheme(ThemeVariant.Light).Primary.ToAvaloniaColor();
        var dark = scheme.CreateScheme(ThemeVariant.Dark).Primary.ToAvaloniaColor();
        Assert.NotEqual(light, dark);

        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(dark, Read(resources, SysColorToken.Primary, ThemeVariant.Dark));
            Assert.Equal(light, Read(resources, SysColorToken.Primary, ThemeVariant.Light));
            Assert.Equal(light, Read(resources, SysColorToken.Primary, null));
        }
    }

    private static Color Read(MaterialColorResources resources, object key, ThemeVariant? theme)
    {
        Assert.True(resources.TryGetResource(key, theme, out var value));
        return Assert.IsType<Color>(value);
    }
}
