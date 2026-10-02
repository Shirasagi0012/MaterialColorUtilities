using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using MaterialColorUtilities.Avalonia.Markup;
using MaterialColorUtilities.Avalonia.Tokens;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MarkupExtensionValidationTests
{
    private sealed class EmptyServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static readonly IServiceProvider Services = new EmptyServices();
    private static object Key(BindingBase binding) => Assert.IsType<DynamicResourceExtension>(binding).ResourceKey!;

    [Fact]
    public void PositionalAndNamedSystemTokenFormsProduceIdenticalNativeKeys()
    {
        Assert.Equal(SysColorToken.Primary, Key(new MdSysColorExtension(SysColorToken.Primary).ProvideValue(Services)));
        Assert.Equal(SysColorToken.Primary, Key(new MdSysColorExtension { Token = SysColorToken.Primary }.ProvideValue(Services)));
        Assert.Equal(SysColorToken.Background, Key(new MdSysColorExtension().ProvideValue(Services)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void UndefinedSystemTokenIsRejected(int value) => Assert.Throws<ArgumentOutOfRangeException>(() =>
        new MdSysColorExtension((SysColorToken)value).ProvideValue(Services));

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void StandardAndCustomPaletteToneBoundariesAreValid(int tone)
    {
        var expected = new RefPaletteKey(RefPaletteToken.Primary, (byte)tone);
        Assert.Equal(expected, Key(new MdRefPaletteExtension(RefPaletteToken.Primary, tone).ProvideValue(Services)));
        Assert.Equal(expected, Key(new MdRefPaletteExtension { Palette = RefPaletteToken.Primary, Tone = tone }.ProvideValue(Services)));
        Assert.Equal(new RefPaletteKey("Brand", (byte)tone), Key(new MdRefPaletteExtension { CustomName = "Brand", Tone = tone }.ProvideValue(Services)));
    }

    [Fact]
    public void OmittedPaletteAndToneUsePrimaryAtZero() => Assert.Equal(
        new RefPaletteKey(RefPaletteToken.Primary, 0), Key(new MdRefPaletteExtension().ProvideValue(Services)));

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(256)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void InvalidIntegerToneIsRejectedBeforeNarrowing(int tone)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MdRefPaletteExtension(RefPaletteToken.Primary, tone).ProvideValue(Services));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MdRefPaletteExtension { CustomName = "Brand", Tone = tone }.ProvideValue(Services));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void UndefinedPaletteIsRejected(int value) => Assert.Throws<ArgumentOutOfRangeException>(() =>
        new MdRefPaletteExtension((RefPaletteToken)value, 60).ProvideValue(Services));

    [Fact]
    public void CustomPaletteSentinelCannotReplaceCustomName() => Assert.Throws<ArgumentOutOfRangeException>(() =>
        new MdRefPaletteExtension(RefPaletteToken.Custom, 60).ProvideValue(Services));

    [Theory]
    [InlineData(RefPaletteToken.Primary)]
    [InlineData(RefPaletteToken.Secondary)]
    [InlineData(RefPaletteToken.Custom)]
    public void AnyExplicitPaletteConflictsWithCustomName(RefPaletteToken palette)
    {
        Assert.Throws<ArgumentException>(() => new MdRefPaletteExtension(palette, 60) { CustomName = "Brand" }.ProvideValue(Services));
        Assert.Throws<ArgumentException>(() => new MdRefPaletteExtension { CustomName = "Brand", Palette = palette, Tone = 60 }.ProvideValue(Services));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void EmptyOrWhitespaceCustomPaletteNameIsRejected(string name) => Assert.ThrowsAny<ArgumentException>(() =>
        new MdRefPaletteExtension { CustomName = name, Tone = 60 }.ProvideValue(Services));

    [Fact]
    public void NullCustomPaletteNameMeansNoCustomSelector() => Assert.Equal(new RefPaletteKey(RefPaletteToken.Secondary, 60),
        Key(new MdRefPaletteExtension(RefPaletteToken.Secondary, 60) { CustomName = null }.ProvideValue(Services)));

    [Theory]
    [InlineData(CustomColorRole.Color)]
    [InlineData(CustomColorRole.OnColor)]
    [InlineData(CustomColorRole.Container)]
    [InlineData(CustomColorRole.OnContainer)]
    public void PositionalAndNamedCustomColorFormsProduceIdenticalKeys(CustomColorRole role)
    {
        var expected = new CustomColorKey("Brand", role);
        Assert.Equal(expected, Key(new MdCustomColorExtension("Brand", role).ProvideValue(Services)));
        Assert.Equal(expected, Key(new MdCustomColorExtension { Name = "brand", Role = role }.ProvideValue(Services)));
    }

    [Fact]
    public void CustomRoleDefaultsToColor() => Assert.Equal(new CustomColorKey("Brand", CustomColorRole.Color),
        Key(new MdCustomColorExtension { Name = "Brand" }.ProvideValue(Services)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void MissingOrWhitespaceCustomColorNameIsRejected(string? name) => Assert.ThrowsAny<ArgumentException>(() =>
        new MdCustomColorExtension { Name = name! }.ProvideValue(Services));

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void UndefinedCustomColorRoleIsRejected(int value) => Assert.Throws<ArgumentOutOfRangeException>(() =>
        new MdCustomColorExtension("Brand", (CustomColorRole)value).ProvideValue(Services));

    [Fact]
    public void EachProvideValueCallCreatesANewNativeBinding()
    {
        var extension = new MdSysColorExtension(SysColorToken.Primary);
        Assert.NotSame(extension.ProvideValue(Services), extension.ProvideValue(Services));
    }
}
