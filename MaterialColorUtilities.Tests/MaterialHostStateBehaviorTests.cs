using Avalonia;
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

public class MaterialColorResolverLifecycleTests
{
    [AvaloniaFact]
    public void SchemeInputChange_ReplacesTheSnapshot()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(target, scheme);

        var initial = MaterialColorTestHelper.GetResolvedScheme(target);

        scheme.Color = Colors.Blue;

        var updated = MaterialColorTestHelper.GetResolvedScheme(target);

        Assert.NotNull(initial);
        Assert.NotNull(updated);
        Assert.NotSame(initial, updated);
    }

    [AvaloniaFact]
    public void Snapshot_IsInheritedByDescendants()
    {
        var child = new Border();
        var root = new ThemeVariantScope { Child = child };
        MaterialColor.SetScheme(root, new TonalSpotScheme(Colors.Red));

        Assert.NotNull(MaterialColorTestHelper.GetResolvedScheme(child));
        Assert.Same(
            MaterialColorTestHelper.GetResolvedScheme(root),
            MaterialColorTestHelper.GetResolvedScheme(child));
    }

    [AvaloniaFact]
    public void ApplicationScheme_IsUsedWhenTheTargetInheritsNothing()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        var scheme = new TonalSpotScheme(Colors.Red);
        MaterialColor.SetScheme(application, scheme);

        try
        {
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
        finally
        {
            // The application outlives the test; leaving a scheme on it would leak into the
            // fallback path of every later test.
            MaterialColor.SetScheme(application, null);
        }
    }

    [AvaloniaFact]
    public void ClearingScheme_RemovesTheSnapshotAndFallsBackToTransparent()
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

        Assert.Null(MaterialColorTestHelper.GetResolvedScheme(target));
        Assert.Equal(Colors.Transparent, Assert.IsType<ImmutableSolidColorBrush>(target.Background).Color);
    }
}
