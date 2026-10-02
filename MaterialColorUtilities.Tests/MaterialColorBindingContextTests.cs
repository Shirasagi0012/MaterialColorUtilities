using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorBindingIntegrationTests
{
    [AvaloniaFact]
    public void DynamicResource_ResolvesOwnResourcesAndRefreshesOnSeedChange()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        target.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));

        AssertPrimary(target, scheme, ThemeVariant.Light);
        scheme.Color = Colors.Blue;
        AssertPrimary(target, scheme, ThemeVariant.Light);
        Assert.IsType<ImmutableSolidColorBrush>(target.Background);
        Assert.IsType<Color>(target.FindResource(SysColorToken.Primary));
    }

    [AvaloniaFact]
    public void DynamicResource_FollowsAncestorAndActualThemeVariant()
    {
        var target = new Border();
        var root = new ThemeVariantScope { Child = target, RequestedThemeVariant = ThemeVariant.Light };
        var scheme = new TonalSpotScheme(Colors.Red);
        root.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));

        AssertPrimary(target, scheme, ThemeVariant.Light);
        root.RequestedThemeVariant = ThemeVariant.Dark;
        AssertPrimary(target, scheme, ThemeVariant.Dark);

        root.RequestedThemeVariant = new ThemeVariant("Deep custom", new ThemeVariant("Custom dark", ThemeVariant.Dark));
        AssertPrimary(target, scheme, ThemeVariant.Dark);
    }

    [AvaloniaFact]
    public void LightAndDarkScopes_ShareOneProviderWithoutChangingEachOthersResults()
    {
        var lightTarget = new Border();
        var darkTarget = new Border();
        var root = new StackPanel
        {
            Children =
            {
                new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Light, Child = lightTarget },
                new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark, Child = darkTarget }
            }
        };
        var scheme = new TonalSpotScheme(Colors.Red);
        root.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        lightTarget.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        darkTarget.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));

        AssertPrimary(lightTarget, scheme, ThemeVariant.Light);
        AssertPrimary(darkTarget, scheme, ThemeVariant.Dark);
        scheme.Color = Colors.Blue;
        AssertPrimary(darkTarget, scheme, ThemeVariant.Dark);
        AssertPrimary(lightTarget, scheme, ThemeVariant.Light);
    }

    [AvaloniaFact]
    public void DynamicResource_ColorTargetGetsColorAndBrushTargetUsesNativeConversion()
    {
        var target = new NativeColorTarget();
        var scheme = new TonalSpotScheme(Colors.Red);
        target.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        target.Bind(NativeColorTarget.ValueProperty, new DynamicResourceExtension(SysColorToken.Primary));

        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), target.Value);
        scheme.Color = Colors.Blue;
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), target.Value);
    }

    [AvaloniaFact]
    public void DynamicResource_PaletteToneFollowsThemeAndRefreshesOnSeedChange()
    {
        var target = new Border();
        var root = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Light, Child = target };
        var scheme = new TonalSpotScheme(Colors.Red) { SpecVersion = global::MaterialColorUtilities.DynamicColors.ColorSpec.SpecVersion.Spec2025 };
        root.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(new RefPaletteKey(RefPaletteToken.Primary, 60)));

        Assert.Equal(scheme.CreateScheme(ThemeVariant.Light).PrimaryPalette.Get(60).ToAvaloniaColor(), MaterialColorTestHelper.BrushColor(target.Background));
        root.RequestedThemeVariant = ThemeVariant.Dark;
        Assert.Equal(scheme.CreateScheme(ThemeVariant.Dark).PrimaryPalette.Get(60).ToAvaloniaColor(), MaterialColorTestHelper.BrushColor(target.Background));
        scheme.Color = Colors.Blue;
        Assert.Equal(scheme.CreateScheme(ThemeVariant.Dark).PrimaryPalette.Get(60).ToAvaloniaColor(), MaterialColorTestHelper.BrushColor(target.Background));
    }

    [AvaloniaFact]
    public void TryFindResource_WithoutThemeUsesLightRatherThanHostsActualTheme()
    {
        var root = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark };
        var scheme = new TonalSpotScheme(Colors.Red);
        root.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });

        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.ReadColor(root, SysColorToken.Primary));
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.ReadColor(root, SysColorToken.Primary, ThemeVariant.Default));
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), MaterialColorTestHelper.ReadColor(root, SysColorToken.Primary, root.ActualThemeVariant));
    }

    private static void AssertPrimary(Border target, ColorScheme scheme, ThemeVariant theme) =>
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, theme), MaterialColorTestHelper.BrushColor(target.Background));
}

public sealed class NativeColorTarget : Control
{
    public static readonly StyledProperty<Color> ValueProperty = AvaloniaProperty.Register<NativeColorTarget, Color>(nameof(Value));
    public Color Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
}
