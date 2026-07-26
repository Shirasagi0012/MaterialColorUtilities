using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class CustomColorTests
{
    private const string Key = "Brand";
    private static readonly Color Seed = Colors.Red;
    private static readonly Color Brand = Color.FromRgb(0xFF, 0x57, 0x22);

    private static TonalSpotScheme SchemeWithBrand(bool harmonize = true)
    {
        var scheme = new TonalSpotScheme(Seed);
        scheme.CustomColors.Add(new CustomColor(Key, Brand) { Harmonize = harmonize });
        return scheme;
    }

    /// <summary>The palette a custom color is expected to produce, computed independently of the resolver.</summary>
    private static TonalPalette ExpectedPalette(bool harmonize = true)
    {
        var argb = ArgbExtensions.FromAvaloniaColor(Brand);
        if (harmonize)
            argb = global::MaterialColorUtilities.Blend.Blend.Harmonize(
                argb, ArgbExtensions.FromAvaloniaColor(Seed));

        return new TonalPalette(Hct.From(argb));
    }

    [AvaloniaTheory]
    [InlineData(SysColorToken.Custom, 40)]
    [InlineData(SysColorToken.OnCustom, 100)]
    [InlineData(SysColorToken.CustomContainer, 90)]
    [InlineData(SysColorToken.OnCustomContainer, 10)]
    public void SysColor_Light_UsesSpecTones(SysColorToken token, int tone)
    {
        Assert.Equal(
            ExpectedPalette().Get(tone).ToAvaloniaColor(),
            MaterialColorTestHelper.ResolveSys(SchemeWithBrand(), token, ThemeVariant.Light, Key));
    }

    [AvaloniaTheory]
    [InlineData(SysColorToken.Custom, 80)]
    [InlineData(SysColorToken.OnCustom, 20)]
    [InlineData(SysColorToken.CustomContainer, 30)]
    [InlineData(SysColorToken.OnCustomContainer, 90)]
    public void SysColor_Dark_UsesSpecTones(SysColorToken token, int tone)
    {
        Assert.Equal(
            ExpectedPalette().Get(tone).ToAvaloniaColor(),
            MaterialColorTestHelper.ResolveSys(SchemeWithBrand(), token, ThemeVariant.Dark, Key));
    }

    [AvaloniaFact]
    public void RefPalette_Custom_ReadsRequestedTone()
    {
        Assert.Equal(
            ExpectedPalette().Get(60).ToAvaloniaColor(),
            MaterialColorTestHelper.ResolveRef(SchemeWithBrand(), RefPaletteToken.Custom, 60, Key));
    }

    [AvaloniaFact]
    public void Harmonize_False_KeepsSeedHueExactly()
    {
        var unharmonized = MaterialColorTestHelper.ResolveRef(
            SchemeWithBrand(harmonize: false), RefPaletteToken.Custom, 40, Key);

        Assert.Equal(ExpectedPalette(harmonize: false).Get(40).ToAvaloniaColor(), unharmonized);
    }

    [AvaloniaFact]
    public void Harmonize_True_RotatesHueTowardSourceColor()
    {
        var brandHue = Hct.From(ArgbExtensions.FromAvaloniaColor(Brand)).Hue;
        var harmonizedHue = ExpectedPalette().KeyColor.Hue;
        var sourceHue = Hct.From(ArgbExtensions.FromAvaloniaColor(Seed)).Hue;

        // Red sits at a lower hue than the orange brand color, so harmonizing must pull it down —
        // and never past the source itself.
        Assert.True(harmonizedHue < brandHue, $"expected {harmonizedHue} < {brandHue}");
        Assert.True(harmonizedHue > sourceHue, $"expected {harmonizedHue} > {sourceHue}");

        Assert.NotEqual(
            MaterialColorTestHelper.ResolveRef(SchemeWithBrand(harmonize: false), RefPaletteToken.Custom, 40, Key),
            MaterialColorTestHelper.ResolveRef(SchemeWithBrand(), RefPaletteToken.Custom, 40, Key));
    }

    [AvaloniaFact]
    public void CustomKey_LookupIsCaseInsensitive()
    {
        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(SchemeWithBrand(), SysColorToken.Custom, ThemeVariant.Light, "Brand"),
            MaterialColorTestHelper.ResolveSys(SchemeWithBrand(), SysColorToken.Custom, ThemeVariant.Light, "brAND"));
    }

    [AvaloniaFact]
    public void UnknownCustomKey_DoesNotResolve()
    {
        Assert.False(MaterialColorTestHelper.TryResolveSys(
            SchemeWithBrand(), SysColorToken.Custom, ThemeVariant.Light, "Missing", out _));

        Assert.False(MaterialColorTestHelper.TryResolveRef(
            SchemeWithBrand(), RefPaletteToken.Custom, 40, "Missing", out _));
    }

    [AvaloniaFact]
    public void MissingCustomKey_DoesNotResolve()
    {
        Assert.False(MaterialColorTestHelper.TryResolveSys(
            SchemeWithBrand(), SysColorToken.Custom, ThemeVariant.Light, null, out _));

        Assert.False(MaterialColorTestHelper.TryResolveRef(
            SchemeWithBrand(), RefPaletteToken.Custom, 40, null, out _));
    }

    [AvaloniaFact]
    public void CustomColorWithoutSeed_IsSkipped()
    {
        var scheme = new TonalSpotScheme(Seed);
        scheme.CustomColors.Add(new CustomColor { Name = Key });

        Assert.False(MaterialColorTestHelper.TryResolveSys(
            scheme, SysColorToken.Custom, ThemeVariant.Light, Key, out _));
    }

    [AvaloniaFact]
    public void DuplicateNames_LastDeclarationWins()
    {
        var scheme = new TonalSpotScheme(Seed);
        scheme.CustomColors.Add(new CustomColor(Key, Colors.Green) { Harmonize = false });
        scheme.CustomColors.Add(new CustomColor(Key, Brand) { Harmonize = false });

        Assert.Equal(
            ExpectedPalette(harmonize: false).Get(40).ToAvaloniaColor(),
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Custom, ThemeVariant.Light, Key));
    }

    [AvaloniaFact]
    public void AddingCustomColor_ReResolvesBoundTarget()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Seed);
        MaterialColor.SetScheme(target, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Custom) { CustomKey = Key },
            target,
            Border.BackgroundProperty,
            target);

        target.Bind(Border.BackgroundProperty, binding);

        // Nothing to resolve yet, so the extension's fallback stands.
        Assert.Equal(Colors.Transparent, Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);

        scheme.CustomColors.Add(new CustomColor(Key, Brand));

        Assert.Equal(
            ExpectedPalette().Get(40).ToAvaloniaColor(),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void MutatingCustomColor_ReResolvesBoundTarget()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Seed);
        var custom = new CustomColor(Key, Colors.Green) { Harmonize = false };
        scheme.CustomColors.Add(custom);
        MaterialColor.SetScheme(target, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Custom) { CustomKey = Key },
            target,
            Border.BackgroundProperty,
            target);

        target.Bind(Border.BackgroundProperty, binding);

        custom.Color = Brand;

        Assert.Equal(
            ExpectedPalette(harmonize: false).Get(40).ToAvaloniaColor(),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void RemovingCustomColor_StopsResolving()
    {
        var scheme = SchemeWithBrand();
        Assert.True(MaterialColorTestHelper.TryResolveSys(
            scheme, SysColorToken.Custom, ThemeVariant.Light, Key, out _));

        scheme.CustomColors.Clear();

        Assert.False(MaterialColorTestHelper.TryResolveSys(
            scheme, SysColorToken.Custom, ThemeVariant.Light, Key, out _));
    }

    [AvaloniaFact]
    public void StandardTokens_AreUnaffectedByCustomColors()
    {
        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(new TonalSpotScheme(Seed), SysColorToken.Primary, ThemeVariant.Light),
            MaterialColorTestHelper.ResolveSys(SchemeWithBrand(), SysColorToken.Primary, ThemeVariant.Light));
    }
}
