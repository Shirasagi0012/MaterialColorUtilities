using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorBindingIntegrationTests
{
    [AvaloniaFact]
    public void MdSysColor_ResolvesAgainstTheTargetsOwnScheme()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(target, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty,
            target);

        target.Bind(Border.BackgroundProperty, binding);

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Light),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);

        scheme.Color = Colors.Blue;

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Light),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void MdSysColor_InheritsTheSchemeFromAnAncestor()
    {
        var target = new Border();
        var root = new ThemeVariantScope { Child = target };
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(root, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty,
            root);

        target.Bind(Border.BackgroundProperty, binding);

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Light),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void MdSysColor_FollowsTheTargetsActualThemeVariant()
    {
        var target = new Border();
        var root = new ThemeVariantScope
        {
            Child = target,
            RequestedThemeVariant = ThemeVariant.Light
        };
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(root, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty,
            root);

        target.Bind(Border.BackgroundProperty, binding);

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Light),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);

        root.RequestedThemeVariant = ThemeVariant.Dark;

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Dark),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void MdSysColor_HonorsAnExplicitlyPinnedVariant()
    {
        var target = new Border();
        var root = new ThemeVariantScope
        {
            Child = target,
            RequestedThemeVariant = ThemeVariant.Light
        };
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(root, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary) { Theme = ThemeVariant.Dark },
            target,
            Border.BackgroundProperty,
            root);

        target.Bind(Border.BackgroundProperty, binding);

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Dark),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);

        // A pinned variant ignores the ambient one, in both directions.
        root.RequestedThemeVariant = ThemeVariant.Dark;

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Dark),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void MdSysColor_ResolvesColorTargetsWithoutWrappingInABrush()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(target, scheme);

        // A Color-typed target property makes the extension yield a Color rather than a brush.
        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            SolidColorBrush.ColorProperty,
            target);

        target.Bind(Border.TagProperty, binding);

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Light),
            Assert.IsType<Color>(target.Tag));
    }

    [AvaloniaFact]
    public void MdRefPalette_ResolvesToneAndRefreshesOnSchemeChange()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(target, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdRefPaletteExtension(RefPaletteToken.Primary, 60),
            target,
            Border.BackgroundProperty,
            target);

        target.Bind(Border.BackgroundProperty, binding);

        Assert.Equal(
            MaterialColorTestHelper.ResolveRef(scheme, RefPaletteToken.Primary, 60),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);

        scheme.Color = Colors.Blue;

        Assert.Equal(
            MaterialColorTestHelper.ResolveRef(scheme, RefPaletteToken.Primary, 60),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }
}
