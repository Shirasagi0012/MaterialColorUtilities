using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using DesignTokens;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorBindingIntegrationTests
{
    [AvaloniaFact]
    public void CreateObservable_UsesCapturedProviderOwner()
    {
        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(owner, scheme);

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner);

        var observer = new RecordingColorObserver();
        var context = TokenBinding.CaptureContext(new TestParentStackProvider([provider, owner]), null, null);
        using var subscription = TokenBinding
            .CreateObservable(context, new TokenKey<Color, SysColorTokenKey>(new SysColorTokenKey(SysColorToken.Primary)),
                Colors.Transparent)
            .Subscribe(observer);

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Light),
            observer.Values[^1]);
    }

    [AvaloniaFact]
    public void MdSysColor_ProvideValue_BindsBrushTargetsAndRefreshesOnSchemeChange()
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
    public void MdSysColor_ProvideValue_BindsColorTargetsAndHonorsExplicitTheme()
    {
        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(owner, scheme);

        var brush = new SolidColorBrush();
        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary)
            {
                Theme = ThemeVariant.Dark
            },
            brush,
            SolidColorBrush.ColorProperty,
            owner);

        brush.Bind(SolidColorBrush.ColorProperty, binding);

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Dark),
            brush.Color);

        owner.RequestedThemeVariant = ThemeVariant.Dark;
        scheme.Color = Colors.Blue;

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Dark),
            brush.Color);
    }

    [AvaloniaFact]
    public void MdRefPalette_ProvideValue_BindsBrushTargetsAndRefreshesOnSchemeChange()
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
