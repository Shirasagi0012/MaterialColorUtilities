using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class ResourceKeyTests
{
    [Fact]
    public void StandardSystemRoleValuesRemainStableAndContainNoCustomSentinels()
    {
        string[] names =
        [
            "Background", "OnBackground", "Surface", "SurfaceDim", "SurfaceBright",
            "SurfaceContainerLowest", "SurfaceContainerLow", "SurfaceContainer", "SurfaceContainerHigh",
            "SurfaceContainerHighest", "OnSurface", "SurfaceVariant", "OnSurfaceVariant", "InverseSurface",
            "InverseOnSurface", "Outline", "OutlineVariant", "Shadow", "Scrim", "SurfaceTint", "Primary",
            "OnPrimary", "PrimaryContainer", "OnPrimaryContainer", "InversePrimary", "Secondary", "OnSecondary",
            "SecondaryContainer", "OnSecondaryContainer", "Tertiary", "OnTertiary", "TertiaryContainer",
            "OnTertiaryContainer", "Error", "OnError", "ErrorContainer", "OnErrorContainer", "PrimaryFixed",
            "PrimaryFixedDim", "OnPrimaryFixed", "OnPrimaryFixedVariant", "SecondaryFixed", "SecondaryFixedDim",
            "OnSecondaryFixed", "OnSecondaryFixedVariant", "TertiaryFixed", "TertiaryFixedDim", "OnTertiaryFixed",
            "OnTertiaryFixedVariant"
        ];

        Assert.Equal(names, Enum.GetNames<SysColorToken>());
        Assert.Equal(Enumerable.Range(0, 49), Enum.GetValues<SysColorToken>().Select(value => (int)value));
    }

    [Theory]
    [InlineData(RefPaletteToken.Primary)]
    [InlineData(RefPaletteToken.Secondary)]
    [InlineData(RefPaletteToken.Tertiary)]
    [InlineData(RefPaletteToken.Neutral)]
    [InlineData(RefPaletteToken.NeutralVariant)]
    [InlineData(RefPaletteToken.Error)]
    public void StandardPaletteKeysPreserveTheirValues(RefPaletteToken palette)
    {
        var key = new RefPaletteKey(palette, 60);
        Assert.Equal(palette, key.Palette);
        Assert.Equal((byte)60, key.Tone);
        Assert.Null(key.CustomName);
        Assert.Equal(key, new RefPaletteKey(palette, 60));
        Assert.Equal(key.GetHashCode(), new RefPaletteKey(palette, 60).GetHashCode());
        Assert.NotEqual(key, new RefPaletteKey(palette, 61));
    }

    [Fact]
    public void CustomNamesAreCaseInsensitiveAtTheValueKeyLevel()
    {
        var role = new CustomColorKey("Brand", CustomColorRole.Container);
        var equivalentRole = new CustomColorKey("brAND", CustomColorRole.Container);
        var palette = new RefPaletteKey("Brand", 60);
        var equivalentPalette = new RefPaletteKey("brAND", 60);

        Assert.Equal(role, equivalentRole);
        Assert.Equal(role.GetHashCode(), equivalentRole.GetHashCode());
        Assert.True(role.Equals((object)equivalentRole));
        Assert.Equal(palette, equivalentPalette);
        Assert.Equal(palette.GetHashCode(), equivalentPalette.GetHashCode());
        Assert.True(palette.Equals((object)equivalentPalette));
        Assert.Equal(RefPaletteToken.Custom, palette.Palette);
        Assert.Equal("Brand", palette.CustomName);
        Assert.NotEqual(role, new CustomColorKey("Brand", CustomColorRole.Color));
        Assert.NotEqual(role, new CustomColorKey("Other", CustomColorRole.Container));
        Assert.NotEqual(palette, new RefPaletteKey("Brand", 61));
        Assert.NotEqual(palette, new RefPaletteKey("Other", 60));
        Assert.False(role.Equals(palette));
        Assert.False(palette.Equals(role));

        var dictionary = new Dictionary<object, int> { [role] = 1, [palette] = 2 };
        Assert.Equal(1, dictionary[equivalentRole]);
        Assert.Equal(2, dictionary[equivalentPalette]);
    }

    [Fact]
    public void NameEqualityUsesOrdinalRulesAndDoesNotNormalizeUnicode()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(new CustomColorKey("INDIGO", CustomColorRole.Color),
                new CustomColorKey("indigo", CustomColorRole.Color));
            Assert.Equal(new RefPaletteKey("INDIGO", 40), new RefPaletteKey("indigo", 40));
            Assert.NotEqual(new CustomColorKey("\u00e9", CustomColorRole.Color),
                new CustomColorKey("e\u0301", CustomColorRole.Color));
            Assert.NotEqual(new RefPaletteKey("\u00e9", 40), new RefPaletteKey("e\u0301", 40));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData(" Brand")]
    [InlineData("Brand ")]
    [InlineData("\u00a0Brand")]
    [InlineData("Brand\u00a0")]
    public void ExplicitKeysRejectInvalidNames(string? name)
    {
        var roleError = Assert.ThrowsAny<ArgumentException>(() => new CustomColorKey(name!, CustomColorRole.Color));
        var paletteError = Assert.ThrowsAny<ArgumentException>(() => new RefPaletteKey(name!, 40));
        Assert.False(string.IsNullOrEmpty(roleError.ParamName));
        Assert.False(string.IsNullOrEmpty(paletteError.ParamName));
    }

    [Fact]
    public void InternalWhitespaceIsLegalAndPreserved()
    {
        Assert.Equal(new CustomColorKey("Brand Accent", CustomColorRole.Color),
            new CustomColorKey("brand accent", CustomColorRole.Color));
        Assert.Equal("Brand Accent", new RefPaletteKey("Brand Accent", 40).CustomName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(int.MaxValue)]
    public void UndefinedPaletteEnumsAreRejected(int palette)
    {
        Assert.ThrowsAny<ArgumentException>(() => new RefPaletteKey((RefPaletteToken)palette, 40));
    }

    [Fact]
    public void StandardPaletteConstructorRejectsCustomWithoutAName()
    {
        Assert.ThrowsAny<ArgumentException>(() => new RefPaletteKey(RefPaletteToken.Custom, 40));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(255)]
    public void ToneOutsideTheSupportedRangeIsRejected(byte tone)
    {
        Assert.ThrowsAny<ArgumentException>(() => new RefPaletteKey(RefPaletteToken.Primary, tone));
        Assert.ThrowsAny<ArgumentException>(() => new RefPaletteKey("Brand", tone));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(int.MaxValue)]
    public void UndefinedCustomRolesAreRejected(int role)
    {
        Assert.ThrowsAny<ArgumentException>(() => new CustomColorKey("Brand", (CustomColorRole)role));
    }

    [Fact]
    public void DefaultKeysHaveSafeEqualityAndHashing()
    {
        Assert.Equal(default(CustomColorKey), default(CustomColorKey));
        _ = default(CustomColorKey).GetHashCode();
        Assert.False(default(CustomColorKey).Equals(new CustomColorKey("Brand", CustomColorRole.Color)));
        Assert.False(default(CustomColorKey).Equals(null));
        Assert.Equal(new RefPaletteKey(RefPaletteToken.Primary, 0), default(RefPaletteKey));
        Assert.Equal(new RefPaletteKey(RefPaletteToken.Primary, 0).GetHashCode(), default(RefPaletteKey).GetHashCode());
    }

    [AvaloniaFact]
    public void NativeResourceDictionaryUsesCaseInsensitiveValueKeys()
    {
        var dictionary = new ResourceDictionary
        {
            [new CustomColorKey("Brand", CustomColorRole.Container)] = Colors.Red,
            [new RefPaletteKey("Brand", 60)] = Colors.Blue
        };

        Assert.True(dictionary.TryGetResource(new CustomColorKey("brAND", CustomColorRole.Container), ThemeVariant.Light,
            out var custom));
        Assert.Equal(Colors.Red, Assert.IsType<Color>(custom));
        Assert.True(dictionary.TryGetResource(new RefPaletteKey("brAND", 60), ThemeVariant.Light, out var palette));
        Assert.Equal(Colors.Blue, Assert.IsType<Color>(palette));
        dictionary[new CustomColorKey("BRAND", CustomColorRole.Container)] = Colors.Green;
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(Colors.Green, dictionary[new CustomColorKey("brand", CustomColorRole.Container)]);
    }
}
