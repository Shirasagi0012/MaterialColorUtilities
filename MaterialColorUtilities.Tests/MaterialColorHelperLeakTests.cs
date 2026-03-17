using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorBindingLifecycleTests
{
    [AvaloniaFact]
    public void ReplacingSchemeInput_UnsubscribesFromOldScheme()
    {
        var target = new Border();
        var oldScheme = new TonalSpotScheme(Colors.Red);
        var newScheme = new TonalSpotScheme(Colors.Blue);
        MaterialColor.SetScheme(target, oldScheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty,
            target);
        target.Bind(Border.BackgroundProperty, binding);

        MaterialColor.SetScheme(target, newScheme);
        var countAfterSwap = Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color;

        oldScheme.Color = Colors.Green;

        Assert.Equal(countAfterSwap, Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);

        newScheme.Color = Colors.Purple;

        Assert.Equal(
            MaterialColorTestHelper.ResolveSys(newScheme, SysColorToken.Primary, ThemeVariant.Light),
            Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }

    [AvaloniaFact]
    public void DisposingBindingHandle_StopsFurtherUpdates()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(target, scheme);

        var binding = MaterialColorTestHelper.CreateBinding(
            new MdSysColorExtension(SysColorToken.Primary),
            target,
            Border.BackgroundProperty,
            target);
        var handle = target.Bind(Border.BackgroundProperty, binding);

        handle.Dispose();
        scheme.Color = Colors.Blue;

        Assert.Null(target.Background);
    }
}
