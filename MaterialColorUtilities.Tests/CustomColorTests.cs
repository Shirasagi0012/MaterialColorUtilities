using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;
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

    private static TonalPalette ExpectedPalette(Color brand, Color seed, bool harmonize)
    {
        var argb = ArgbColor.FromAvaloniaColor(brand);
        if (harmonize)
            argb = global::MaterialColorUtilities.Blend.Blend.Harmonize(argb, ArgbColor.FromAvaloniaColor(seed));
        return new TonalPalette(Hct.From(argb));
    }

    [AvaloniaTheory]
    [InlineData(CustomColorRole.Color, 40, 80)]
    [InlineData(CustomColorRole.OnColor, 100, 20)]
    [InlineData(CustomColorRole.Container, 90, 30)]
    [InlineData(CustomColorRole.OnContainer, 10, 90)]
    public void CustomRolesKeepTheirFixedTonesRegardlessOfContrast(CustomColorRole role, int lightTone, int darkTone)
    {
        foreach (var harmonize in new[] { false, true })
        {
            var scheme = SchemeWithBrand(harmonize);
            var resources = new MaterialColorResources { Scheme = scheme };
            var palette = ExpectedPalette(Brand, Seed, harmonize);
            foreach (var contrast in new[] { -1d, 0d, 1d })
            {
                scheme.ContrastLevel = contrast;
                Assert.Equal(palette.Get(lightTone).ToAvaloniaColor(),
                    Read(resources, new CustomColorKey(Key, role), ThemeVariant.Light));
                Assert.Equal(palette.Get(darkTone).ToAvaloniaColor(),
                    Read(resources, new CustomColorKey(Key, role), ThemeVariant.Dark));
            }
        }
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(40)]
    [InlineData(60)]
    [InlineData(99)]
    [InlineData(100)]
    public void CustomPaletteReadsRequestedToneForEitherTheme(byte tone)
    {
        var resources = new MaterialColorResources { Scheme = SchemeWithBrand() };
        var expected = ExpectedPalette(Brand, Seed, true).Get(tone).ToAvaloniaColor();
        Assert.Equal(expected, Read(resources, new RefPaletteKey(Key, tone), ThemeVariant.Light));
        Assert.Equal(expected, Read(resources, new RefPaletteKey(Key, tone), ThemeVariant.Dark));
    }

    [AvaloniaFact]
    public void HarmonizeRotatesHueTowardTheSeedAndFalsePreservesTheBrandPalette()
    {
        var brandHue = Hct.From(ArgbColor.FromAvaloniaColor(Brand)).Hue;
        var harmonizedHue = ExpectedPalette(Brand, Seed, true).KeyColor.Hue;
        var seedHue = Hct.From(ArgbColor.FromAvaloniaColor(Seed)).Hue;
        Assert.InRange(harmonizedHue, seedHue, brandHue);
        var scheme = SchemeWithBrand();
        var resources = new MaterialColorResources { Scheme = scheme };
        var key = new RefPaletteKey(Key, 40);
        var harmonized = Read(resources, key);
        scheme.CustomColors[0].Harmonize = false;
        var unharmonized = Read(resources, key);
        Assert.Equal(ExpectedPalette(Brand, Seed, false).Get(40).ToAvaloniaColor(), unharmonized);
        Assert.NotEqual(harmonized, unharmonized);
        scheme.CustomColors[0].Harmonize = true;
        Assert.Equal(harmonized, Read(resources, key));
    }

    [AvaloniaFact]
    public void MainSeedChangeInvalidatesHarmonizedCustomColors()
    {
        var scheme = SchemeWithBrand();
        var resources = new MaterialColorResources { Scheme = scheme };
        var key = new CustomColorKey(Key, CustomColorRole.Color);
        var original = Read(resources, key);
        scheme.Color = Colors.Blue;
        Assert.Equal(ExpectedPalette(Brand, Colors.Blue, true).Get(40).ToAvaloniaColor(), Read(resources, key));
        Assert.NotEqual(original, Read(resources, key));
    }

    [AvaloniaFact]
    public void CustomNameLookupIsCaseInsensitiveForBothKeyFamilies()
    {
        var resources = new MaterialColorResources { Scheme = SchemeWithBrand() };
        Assert.Equal(Read(resources, new CustomColorKey("Brand", CustomColorRole.Color)),
            Read(resources, new CustomColorKey("brAND", CustomColorRole.Color)));
        Assert.Equal(Read(resources, new RefPaletteKey("Brand", 40)),
            Read(resources, new RefPaletteKey("brAND", 40)));
    }

    [AvaloniaFact]
    public void UnknownCustomNamesMissWithoutReturningATransparentFallback()
    {
        var resources = new MaterialColorResources { Scheme = SchemeWithBrand() };
        AssertMissing(resources, "Missing");
        Assert.False(resources.TryGetResource(default(CustomColorKey), ThemeVariant.Light, out var value));
        Assert.Null(value);
    }

    [AvaloniaFact]
    public void IncompleteCustomItemsAreSkippedAndResolveWhenReady()
    {
        var scheme = new TonalSpotScheme(Seed);
        var custom = new CustomColor();
        scheme.CustomColors.Add(custom);
        var resources = new MaterialColorResources { Scheme = scheme };
        AssertMissing(resources, Key);
        custom.Name = Key;
        AssertMissing(resources, Key);
        custom.Color = Brand;
        Assert.Equal(ExpectedPalette(Brand, Seed, true).Get(40).ToAvaloniaColor(),
            Read(resources, new CustomColorKey(Key, CustomColorRole.Color)));
        custom.Name = null;
        AssertMissing(resources, Key);
        custom.Name = Key;
        custom.Color = null;
        AssertMissing(resources, Key);
        custom.Color = Brand;
        _ = Read(resources, new RefPaletteKey(Key, 40));
    }

    [AvaloniaTheory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\n")]
    [InlineData(" Brand")]
    [InlineData("Brand ")]
    [InlineData("Brand\u00a0")]
    public void CustomInputRejectsInvalidAuthoredNames(string invalid)
    {
        var custom = new CustomColor(Key, Brand);
        Assert.ThrowsAny<ArgumentException>(() => custom.Name = invalid);
        Assert.Equal(Key, custom.Name);
        Assert.ThrowsAny<ArgumentException>(() => new CustomColor(invalid, Brand));
    }

    [AvaloniaFact]
    public void DuplicateNamesUseTheLastCompleteDeclarationAndRespondToReordering()
    {
        var scheme = new TonalSpotScheme(Seed);
        var green = new CustomColor("Brand", Colors.Green) { Harmonize = false };
        var orange = new CustomColor("brAND", Brand) { Harmonize = false };
        scheme.CustomColors.Add(green);
        scheme.CustomColors.Add(orange);
        var resources = new MaterialColorResources { Scheme = scheme };
        var key = new CustomColorKey(Key, CustomColorRole.Color);
        Assert.Equal(ExpectedPalette(Brand, Seed, false).Get(40).ToAvaloniaColor(), Read(resources, key));
        scheme.CustomColors.Move(0, 1);
        Assert.Equal(ExpectedPalette(Colors.Green, Seed, false).Get(40).ToAvaloniaColor(), Read(resources, key));
        scheme.CustomColors.Remove(green);
        Assert.Equal(ExpectedPalette(Brand, Seed, false).Get(40).ToAvaloniaColor(), Read(resources, key));
        scheme.CustomColors.Add(new CustomColor { Name = "BRAND" });
        Assert.Equal(ExpectedPalette(Brand, Seed, false).Get(40).ToAvaloniaColor(), Read(resources, key));
    }

    [AvaloniaFact]
    public void AddRemoveRenameAndReaddDoNotLeaveCachedCustomResults()
    {
        var scheme = new TonalSpotScheme(Seed);
        var resources = new MaterialColorResources { Scheme = scheme };
        var custom = new CustomColor(Key, Brand) { Harmonize = false };
        AssertMissing(resources, Key);
        scheme.CustomColors.Add(custom);
        var first = Read(resources, new CustomColorKey(Key, CustomColorRole.Color));
        _ = Read(resources, new RefPaletteKey(Key, 40));
        custom.Name = "Accent";
        AssertMissing(resources, Key);
        Assert.Equal(first, Read(resources, new CustomColorKey("Accent", CustomColorRole.Color)));
        custom.Name = "ACCENT";
        Assert.Equal(first, Read(resources, new CustomColorKey("accent", CustomColorRole.Color)));
        scheme.CustomColors.Remove(custom);
        AssertMissing(resources, "Accent");
        custom.Color = Colors.Blue;
        scheme.CustomColors.Add(custom);
        Assert.NotEqual(first, Read(resources, new CustomColorKey("Accent", CustomColorRole.Color)));
        scheme.CustomColors.Clear();
        AssertMissing(resources, "Accent");
    }

    [AvaloniaFact]
    public void ReplacingACollectionItemDetachesTheOldItemAndSubscribesTheNewOne()
    {
        var scheme = SchemeWithBrand(false);
        var oldItem = scheme.CustomColors[0];
        var replacement = new CustomColor(Key, Colors.Blue) { Harmonize = false };
        var resources = new MaterialColorResources { Scheme = scheme };
        _ = Read(resources, new CustomColorKey(Key, CustomColorRole.Color));
        var changes = 0;
        scheme.SchemeChanged += (_, _) => changes++;
        scheme.CustomColors[0] = replacement;
        var replaced = Read(resources, new CustomColorKey(Key, CustomColorRole.Color));
        Assert.Equal(ExpectedPalette(Colors.Blue, Seed, false).Get(40).ToAvaloniaColor(), replaced);
        changes = 0;
        oldItem.Color = Colors.Teal;
        Assert.Equal(0, changes);
        Assert.Equal(replaced, Read(resources, new CustomColorKey(Key, CustomColorRole.Color)));
        replacement.Color = Brand;
        Assert.Equal(1, changes);
        Assert.Equal(ExpectedPalette(Brand, Seed, false).Get(40).ToAvaloniaColor(),
            Read(resources, new CustomColorKey(Key, CustomColorRole.Color)));
    }

    [AvaloniaTheory]
    [InlineData(ResetBehavior.Reset)]
    [InlineData(ResetBehavior.Remove)]
    public void ClearDetachesEveryRemovedItemForEitherResetBehavior(ResetBehavior resetBehavior)
    {
        var scheme = SchemeWithBrand();
        scheme.CustomColors.ResetBehavior = resetBehavior;
        var first = scheme.CustomColors[0];
        var second = new CustomColor("Accent", Colors.Blue);
        scheme.CustomColors.Add(second);
        var resources = new MaterialColorResources { Scheme = scheme };
        _ = Read(resources, new CustomColorKey(Key, CustomColorRole.Color));
        _ = Read(resources, new RefPaletteKey("Accent", 40));
        var changes = 0;
        scheme.SchemeChanged += (_, _) => changes++;
        scheme.CustomColors.Clear();
        Assert.True(changes > 0);
        AssertMissing(resources, Key);
        AssertMissing(resources, "Accent");
        changes = 0;
        first.Color = Colors.Teal;
        first.Name = "Renamed";
        second.Harmonize = false;
        Assert.Equal(0, changes);
        AssertMissing(resources, "Renamed");
    }

    [AvaloniaFact]
    public void DuplicateObjectIsSubscribedOnceUntilItsLastOccurrenceIsRemoved()
    {
        var scheme = SchemeWithBrand();
        var custom = scheme.CustomColors[0];
        scheme.CustomColors.Add(custom);
        var changes = 0;
        scheme.SchemeChanged += (_, _) => changes++;
        custom.Color = Colors.Blue;
        Assert.Equal(1, changes);
        scheme.CustomColors.RemoveAt(0);
        changes = 0;
        custom.Color = Colors.Teal;
        Assert.Equal(1, changes);
        scheme.CustomColors.RemoveAt(0);
        changes = 0;
        custom.Color = Colors.Green;
        Assert.Equal(0, changes);
    }

    [AvaloniaFact]
    public void RepeatedClearAndReaddDoNotMultiplySubscriptions()
    {
        var scheme = new TonalSpotScheme(Seed);
        var custom = new CustomColor(Key, Brand);
        for (var i = 0; i < 10; i++)
        {
            scheme.CustomColors.Add(custom);
            scheme.CustomColors.Clear();
        }
        scheme.CustomColors.Add(custom);
        var changes = 0;
        scheme.SchemeChanged += (_, _) => changes++;
        custom.Harmonize = false;
        Assert.Equal(1, changes);
    }

    [AvaloniaFact]
    public void StandardRolesAreUnaffectedByCustomColors()
    {
        var plain = new MaterialColorResources { Scheme = new TonalSpotScheme(Seed) };
        var custom = new MaterialColorResources { Scheme = SchemeWithBrand() };
        foreach (var role in Enum.GetValues<SysColorToken>())
        foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            Assert.Equal(Read(plain, role, theme), Read(custom, role, theme));
    }

    private static Color Read(MaterialColorResources resources, object key, ThemeVariant? theme = null)
    {
        Assert.True(resources.TryGetResource(key, theme ?? ThemeVariant.Light, out var value));
        return Assert.IsType<Color>(value);
    }

    private static void AssertMissing(MaterialColorResources resources, string name)
    {
        foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            foreach (var role in Enum.GetValues<CustomColorRole>())
            {
                Assert.False(resources.TryGetResource(new CustomColorKey(name, role), theme, out var roleValue));
                Assert.Null(roleValue);
            }
            Assert.False(resources.TryGetResource(new RefPaletteKey(name, 40), theme, out var paletteValue));
            Assert.Null(paletteValue);
        }
    }
}
