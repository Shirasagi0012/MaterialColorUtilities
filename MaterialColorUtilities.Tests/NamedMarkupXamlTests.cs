using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class NamedMarkupXamlTests
{
    [AvaloniaFact]
    public void CompiledNamedAndDefaultFormsResolveNativeTypedKeysAndContinueUpdating()
    {
        var view = new NamedMarkupView();
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        var targets = new (string Name, object Key)[]
        {
            ("SystemNamed", SysColorToken.Primary),
            ("PaletteNamed", new RefPaletteKey(RefPaletteToken.Primary, 60)),
            ("CustomPaletteNamed", new RefPaletteKey("Brand", 60)),
            ("CustomRoleNamed", new CustomColorKey("Brand", CustomColorRole.Container)),
            ("DefaultCustomRole", new CustomColorKey("Brand", CustomColorRole.Color)),
            ("DefaultSystem", SysColorToken.Background),
            ("DefaultPalette", new RefPaletteKey(RefPaletteToken.Primary, 0)),
            ("PaletteLowerBound", new RefPaletteKey(RefPaletteToken.Primary, 0)),
            ("PaletteUpperBound", new RefPaletteKey(RefPaletteToken.Primary, 100))
        };
        void AssertColors()
        {
            foreach (var (name, key) in targets)
                Assert.Equal(MaterialColorTestHelper.ReadColor(view, key, view.ActualThemeVariant),
                    MaterialColorTestHelper.BrushColor(view.FindControl<Border>(name)!.Background));
        }
        try
        {
            window.Show();
            AssertColors();
            var scheme = ((MaterialColorResources)view.Resources.MergedDictionaries[0]).Scheme!;
            scheme.Color = Colors.Blue;
            AssertColors();
            window.RequestedThemeVariant = ThemeVariant.Dark;
            AssertColors();
            scheme.CustomColors[0].Color = Colors.Green;
            AssertColors();
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void CompiledOutOfRangeIntegerTonesReachExplicitMarkupValidation()
    {
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new InvalidNegativeToneView());
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new InvalidOversizedToneView());
    }

    [AvaloniaFact]
    public void CompiledExplicitDefaultPaletteStillConflictsWithCustomName() =>
        Assert.ThrowsAny<ArgumentException>(() => new ConflictingPaletteView());
}

public partial class NamedMarkupView : UserControl
{
    public NamedMarkupView() => AvaloniaXamlLoader.Load(this);
}
public partial class InvalidNegativeToneView : UserControl
{
    public InvalidNegativeToneView() => AvaloniaXamlLoader.Load(this);
}
public partial class InvalidOversizedToneView : UserControl
{
    public InvalidOversizedToneView() => AvaloniaXamlLoader.Load(this);
}
public partial class ConflictingPaletteView : UserControl
{
    public ConflictingPaletteView() => AvaloniaXamlLoader.Load(this);
}
