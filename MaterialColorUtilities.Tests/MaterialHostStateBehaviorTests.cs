using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MaterialColorResourceScopeTests
{
    [AvaloniaFact]
    public void ApplicationWindowAndNestedScopes_UseNativeNearestKeyLookup()
    {
        var app = Assert.IsType<HeadlessTestApplication>(Application.Current);
        var appScheme = new TonalSpotScheme(Colors.Red);
        var appProvider = new MaterialColorResources { Scheme = appScheme };
        app.Resources.MergedDictionaries.Add(appProvider);
        var outer = new Border();
        var nested = new Border();
        var nestedScope = new Border { Child = nested };
        var window = new Window { Content = new StackPanel { Children = { outer, nestedScope } } };
        try
        {
            outer.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
            nested.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
            window.Show();
            Assert.Equal(MaterialColorTestHelper.Primary(appScheme, window.ActualThemeVariant), MaterialColorTestHelper.BrushColor(outer.Background));
            Assert.Equal(MaterialColorTestHelper.BrushColor(outer.Background), MaterialColorTestHelper.BrushColor(nested.Background));

            var windowScheme = new TonalSpotScheme(Colors.Blue);
            window.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = windowScheme });
            Assert.Equal(MaterialColorTestHelper.Primary(windowScheme, window.ActualThemeVariant), MaterialColorTestHelper.BrushColor(outer.Background));
            nestedScope.Resources[SysColorToken.Primary] = Colors.Lime;
            Assert.Equal(Colors.Lime, MaterialColorTestHelper.BrushColor(nested.Background));
            Assert.Equal(MaterialColorTestHelper.Primary(windowScheme, window.ActualThemeVariant), MaterialColorTestHelper.BrushColor(outer.Background));
        }
        finally
        {
            window.Close();
            app.Resources.MergedDictionaries.Remove(appProvider);
        }
    }

    [AvaloniaFact]
    public void DirectThemeAndMergedEntries_FollowNativeResourcePriority()
    {
        var scope = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark };
        var first = new MaterialColorResources { Scheme = new TonalSpotScheme(Colors.Red) };
        var second = new MaterialColorResources { Scheme = new TonalSpotScheme(Colors.Blue) };
        scope.Resources.MergedDictionaries.Add(first);
        scope.Resources.MergedDictionaries.Add(second);
        Assert.Equal(MaterialColorTestHelper.Primary(second.Scheme!, ThemeVariant.Dark), MaterialColorTestHelper.ReadColor(scope, SysColorToken.Primary, ThemeVariant.Dark));

        scope.Resources.ThemeDictionaries[ThemeVariant.Default] = new ResourceDictionary { [SysColorToken.Primary] = Colors.Green };
        Assert.Equal(Colors.Green, MaterialColorTestHelper.ReadColor(scope, SysColorToken.Primary, ThemeVariant.Dark));
        scope.Resources.ThemeDictionaries[ThemeVariant.Dark] = new ResourceDictionary { [SysColorToken.Primary] = Colors.Navy };
        Assert.Equal(Colors.Navy, MaterialColorTestHelper.ReadColor(scope, SysColorToken.Primary, ThemeVariant.Dark));
        var customDark = new ThemeVariant("Inherited dark", ThemeVariant.Dark);
        Assert.Equal(Colors.Navy, MaterialColorTestHelper.ReadColor(scope, SysColorToken.Primary, customDark));

        scope.Resources[SysColorToken.Primary] = Colors.Gold;
        Assert.Equal(Colors.Gold, MaterialColorTestHelper.ReadColor(scope, SysColorToken.Primary, ThemeVariant.Dark));
        Assert.Equal(MaterialColorTestHelper.PrimaryContainer(second.Scheme!, ThemeVariant.Dark), MaterialColorTestHelper.ReadColor(scope, SysColorToken.PrimaryContainer, ThemeVariant.Dark));
    }

    [AvaloniaFact]
    public void MissingLocalCustomKey_FallsBackPerKeyToOuterProvider()
    {
        var outerScheme = new TonalSpotScheme(Colors.Red);
        outerScheme.CustomColors.Add(new CustomColor { Name = "Brand", Color = Colors.Orange });
        var target = new Border();
        var root = new Border { Child = target };
        root.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = outerScheme });
        target.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = new TonalSpotScheme(Colors.Blue) });
        var key = new CustomColorKey("brAND", CustomColorRole.Container);
        target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(key));

        Assert.Equal(MaterialColorTestHelper.ReadColor(root, key), MaterialColorTestHelper.BrushColor(target.Background));
        target.Resources[new CustomColorKey("BRAND", CustomColorRole.Container)] = Colors.Orange;
        Assert.Equal(Colors.Orange, MaterialColorTestHelper.BrushColor(target.Background));
    }

    [AvaloniaFact]
    public void ClearingSchemeOrSeed_RemovesResourceWithoutTransparentFallback()
    {
        var target = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        var provider = new MaterialColorResources { Scheme = scheme };
        target.Resources.MergedDictionaries.Add(provider);
        target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        Assert.NotNull(target.Background);

        scheme.Color = null;
        Assert.False(target.TryFindResource(SysColorToken.Primary, out _));
        Assert.Null(target.Background);
        scheme.Color = Colors.Blue;
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));
        provider.Scheme = null;
        Assert.False(provider.HasResources);
        Assert.False(target.TryFindResource(SysColorToken.Primary, out _));
        Assert.Null(target.Background);
    }

    [AvaloniaFact]
    public void ProviderRemovalAndReaddition_RequeriesDetachedSchemeChanges()
    {
        var target = new Border();
        var root = new Border { Child = target };
        root.Resources[SysColorToken.Primary] = Colors.Gold;
        var scheme = new TonalSpotScheme(Colors.Red);
        var provider = new MaterialColorResources { Scheme = scheme };
        target.Resources.MergedDictionaries.Add(provider);
        target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));

        target.Resources.MergedDictionaries.Remove(provider);
        Assert.Null(provider.Owner);
        Assert.Equal(Colors.Gold, MaterialColorTestHelper.BrushColor(target.Background));
        scheme.Color = Colors.Blue;
        target.Resources.MergedDictionaries.Add(provider);
        Assert.Same(target, provider.Owner);
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));
    }

    [AvaloniaFact]
    public void Reparenting_RebindsToNewNativeScope()
    {
        var target = new Border();
        var first = new StackPanel();
        var second = new StackPanel();
        first.Resources[SysColorToken.Primary] = Colors.Red;
        second.Resources[SysColorToken.Primary] = Colors.Blue;
        var window = new Window { Content = new StackPanel { Children = { first, second } } };
        try
        {
            window.Show();
            target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
            first.Children.Add(target);
            Assert.Equal(Colors.Red, MaterialColorTestHelper.BrushColor(target.Background));
            first.Children.Remove(target);
            second.Children.Add(target);
            Assert.Equal(Colors.Blue, MaterialColorTestHelper.BrushColor(target.Background));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void MovingProviderBetweenOwners_ExposesFreshValueDuringOwnerChanged()
    {
        var first = new Border();
        var second = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        var provider = new MaterialColorResources { Scheme = scheme };
        first.Resources.MergedDictionaries.Add(provider);
        _ = MaterialColorTestHelper.ReadColor(first, SysColorToken.Primary);
        first.Resources.MergedDictionaries.Remove(provider);
        scheme.Color = Colors.Blue;
        Color? valueDuringOwnerChanged = null;
        provider.OwnerChanged += (_, _) =>
        {
            if (provider.Owner != null)
            {
                Assert.True(provider.TryGetResource(SysColorToken.Primary, ThemeVariant.Light, out var value));
                valueDuringOwnerChanged = Assert.IsType<Color>(value);
            }
        };
        second.Resources.MergedDictionaries.Add(provider);
        Assert.Same(second, provider.Owner);
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), valueDuringOwnerChanged);
    }

    [AvaloniaFact]
    public void OneProviderCannotHaveTwoOwners_ButTwoProvidersCanShareScheme()
    {
        var first = new Border();
        var second = new Border();
        var scheme = new TonalSpotScheme(Colors.Red);
        var provider = new MaterialColorResources { Scheme = scheme };
        first.Resources.MergedDictionaries.Add(provider);
        Assert.Throws<InvalidOperationException>(() => ((IResourceProvider)provider).AddOwner(second));
        second.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        first.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        second.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        scheme.Color = Colors.Blue;
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(first.Background));
        Assert.Equal(MaterialColorTestHelper.BrushColor(first.Background), MaterialColorTestHelper.BrushColor(second.Background));
    }
}
