using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorResolverLifecycleTests
{
    [AvaloniaFact]
    public void SchemeInputChange_ReplacesResolverSnapshot()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(target, scheme);

        var initialRefPaletteTokenResolver = MaterialColorTestHelper.GetRefPaletteTokenResolver(target);
        var initialSysColorTokenResolver = MaterialColorTestHelper.GetSysColorTokenResolver(target);

        scheme.Color = Colors.Blue;

        var updatedRefPaletteTokenResolver = MaterialColorTestHelper.GetRefPaletteTokenResolver(target);
        var updatedSysColorTokenResolver = MaterialColorTestHelper.GetSysColorTokenResolver(target);

        Assert.NotNull(initialRefPaletteTokenResolver);
        Assert.NotNull(initialSysColorTokenResolver);
        Assert.NotNull(updatedRefPaletteTokenResolver);
        Assert.NotNull(updatedSysColorTokenResolver);
        Assert.NotSame(initialRefPaletteTokenResolver, updatedRefPaletteTokenResolver);
        Assert.NotSame(initialSysColorTokenResolver, updatedSysColorTokenResolver);
    }

    [AvaloniaFact]
    public void ApplicationResolver_IsUsedWhenLocalSchemeMissing()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(application, scheme);

        var target = new Border();
        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty);

        target.Bind(Border.BackgroundProperty, binding);

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(scheme, SysColorToken.Primary, ThemeVariant.Light),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void ClearingScheme_RemovesResolversAndFallsBackToTransparent()
    {
        var target = new Border();
        MaterialColor.SetScheme(target, new TonalSpotScheme(Colors.Red));

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty,
            target);

        target.Bind(Border.BackgroundProperty, binding);
        MaterialColor.SetScheme(target, null);

        Assert.Null(MaterialColorTestHelper.GetRefPaletteTokenResolver(target));
        Assert.Null(MaterialColorTestHelper.GetSysColorTokenResolver(target));
        Assert.Equal(Colors.Transparent, Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }
}
