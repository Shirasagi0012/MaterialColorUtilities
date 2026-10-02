using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using Xunit;
namespace MaterialColorUtilities.Tests.Avalonia;
public class MarkupWrapperParityTests
{
    private static Color? Brush(IBrush? b) => (b as ISolidColorBrush)?.Color;
    [AvaloniaFact]
    public void CompiledWrappersMatchNativeAcrossReplacementOverridesMissingAndRecovery()
    {
        var native = new NativeResourceView();
        var wrapped = new MarkupNativeResourceView();
        var host = new StackPanel { Children = { native, wrapped } };
        var window = new Window { Content = host, RequestedThemeVariant = ThemeVariant.Light };
        void Equal()
        {
            foreach (var name in new[] { "PaletteTarget", "CustomTarget", "DirectTarget", "StyleTarget", "DarkTarget", "SharedLightTarget", "SharedDarkTarget" })
                Assert.Equal(Brush(native.FindControl<Border>(name)!.Background), Brush(wrapped.FindControl<Border>(name)!.Background));
            Assert.Equal(native.FindControl<NativeColorTarget>("ColorTarget")!.Value, wrapped.FindControl<NativeColorTarget>("ColorTarget")!.Value);
        }
        try
        {
            window.Show(); Equal();
            foreach(var v in new UserControl[] {native,wrapped}) ((MaterialColorResources)v.Resources.MergedDictionaries[0]).Scheme!.Color = Colors.Blue;
            Equal(); window.RequestedThemeVariant = ThemeVariant.Dark; Equal();
            foreach(var v in new UserControl[] {native,wrapped}) v.Resources[SysColorToken.Primary] = Colors.Gold;
            Equal(); Assert.Equal(Colors.Gold, Brush(wrapped.FindControl<Border>("DirectTarget")!.Background));
            foreach(var v in new UserControl[] {native,wrapped}) v.Resources.Remove(SysColorToken.Primary);
            Equal();
            foreach(var v in new UserControl[] {native,wrapped}) v.Resources.MergedDictionaries.Clear();
            Equal(); Assert.Null(wrapped.FindControl<Border>("DirectTarget")!.Background);
            foreach(var v in new UserControl[] {native,wrapped}) v.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = new TonalSpotScheme(Colors.Green) });
            Equal(); Assert.NotNull(wrapped.FindControl<Border>("DirectTarget")!.Background);
            Assert.Null(wrapped.FindControl<Border>("CustomTarget")!.Background);
            foreach(var v in new UserControl[] {native,wrapped}) ((MaterialColorResources)v.Resources.MergedDictionaries[0]).Scheme!.CustomColors.Add(new CustomColor { Name = "brand", Color = Colors.Red });
            Equal(); Assert.NotNull(wrapped.FindControl<Border>("CustomTarget")!.Background);
            foreach(var v in new UserControl[] {native,wrapped}) ((MaterialColorResources)v.Resources.MergedDictionaries[0]).Scheme!.Color = null;
            Equal(); Assert.Null(wrapped.FindControl<Border>("DirectTarget")!.Background);
            foreach(var v in new UserControl[] {native,wrapped}) ((MaterialColorResources)v.Resources.MergedDictionaries[0]).Scheme!.Color = Colors.Red;
            Equal();
        }
        finally {window.Close();}
    }
    [AvaloniaFact]
    public void CompiledConstructorConversionPassesTypedArgumentsAndRejectsInvalidToneAtKeyValidation()
    {
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new InvalidToneView());
    }
}
public partial class InvalidToneView : UserControl
{
    public InvalidToneView() => AvaloniaXamlLoader.Load(this);
}

public class MarkupWrapperConversionTests
{
    [AvaloniaFact] public void UndefinedNumericPalette_IsRejectedAtKeyConstruction() => Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new InvalidNumericPaletteView());
    [AvaloniaFact] public void UndefinedNumericRole_IsRejectedAtKeyConstruction() => Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new InvalidNumericRoleView());
    [AvaloniaFact] public void UndefinedNumericSys_IsRejectedAtMarkupValidation() => Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new UndefinedNumericSysView());
}
public partial class InvalidNumericPaletteView : UserControl { public InvalidNumericPaletteView() => AvaloniaXamlLoader.Load(this); }
public partial class InvalidNumericRoleView : UserControl { public InvalidNumericRoleView() => AvaloniaXamlLoader.Load(this); }
public partial class UndefinedNumericSysView : UserControl { public UndefinedNumericSysView() => AvaloniaXamlLoader.Load(this); }
