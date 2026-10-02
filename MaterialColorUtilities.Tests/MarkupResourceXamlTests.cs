using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class MarkupNativeResourceXamlTests
{
    [AvaloniaFact]
    public void CompiledXaml_ResolvesContentPropertyEnumKeysColorTargetsAndStyleSetters()
    {
        var view = new MarkupNativeResourceView();
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            var scheme = Assert.IsType<TonalSpotScheme>(Assert.IsType<MaterialColorResources>(view.Resources.MergedDictionaries[0]).Scheme);
            Assert.Single(scheme.CustomColors);
            Assert.Equal(MaterialColorTestHelper.ReadColor(view, NativeResourceKeys.Primary60), MaterialColorTestHelper.BrushColor(view.FindControl<Border>("PaletteTarget")!.Background));
            Assert.Equal(MaterialColorTestHelper.ReadColor(view, NativeResourceKeys.BrandContainer), MaterialColorTestHelper.BrushColor(view.FindControl<Border>("CustomTarget")!.Background));
            var direct = view.FindControl<Border>("DirectTarget")!;
            var color = view.FindControl<NativeColorTarget>("ColorTarget")!;
            var styled = view.FindControl<Border>("StyleTarget")!;
            var dark = view.FindControl<Border>("DarkTarget")!;
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(direct.Background));
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), color.Value);
            Assert.Equal(MaterialColorTestHelper.PrimaryContainer(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(styled.Background));
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), MaterialColorTestHelper.BrushColor(dark.Background));

            scheme.Color = Colors.Blue;
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), color.Value);
            Assert.Equal(MaterialColorTestHelper.PrimaryContainer(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(styled.Background));
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), MaterialColorTestHelper.BrushColor(dark.Background));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void CompiledSharedControlThemeAndTemplate_ResolveEachInstancesScopeAndTheme()
    {
        var view = new MarkupNativeResourceView();
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            var scheme = ((MaterialColorResources)view.Resources.MergedDictionaries[0]).Scheme!;
            var light = view.FindControl<Button>("LightButton")!;
            var dark = view.FindControl<Button>("DarkButton")!;
            light.ApplyTemplate();
            dark.ApplyTemplate();
            var lightBorder = light.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_ResourceBorder");
            var darkBorder = dark.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_ResourceBorder");
            Assert.NotSame(lightBorder, darkBorder);
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(light.Background));
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), MaterialColorTestHelper.BrushColor(dark.Background));
            Assert.Equal(MaterialColorTestHelper.PrimaryContainer(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(lightBorder.Background));
            Assert.Equal(MaterialColorTestHelper.PrimaryContainer(scheme, ThemeVariant.Dark), MaterialColorTestHelper.BrushColor(darkBorder.Background));

            dark.Resources[SysColorToken.PrimaryContainer] = Colors.Gold;
            Assert.Equal(Colors.Gold, MaterialColorTestHelper.BrushColor(darkBorder.Background));
            scheme.Color = Colors.Red;
            Assert.Equal(MaterialColorTestHelper.PrimaryContainer(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(lightBorder.Background));
            Assert.Equal(Colors.Gold, MaterialColorTestHelper.BrushColor(darkBorder.Background));
            light.Template = null;
            light.ApplyTemplate();
            light.Template = dark.Template;
            light.ApplyTemplate();
            var replacement = light.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_ResourceBorder");
            Assert.NotSame(lightBorder, replacement);
            Assert.Equal(MaterialColorTestHelper.PrimaryContainer(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(replacement.Background));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void CompiledDataTemplate_UsesEachCreatedControlsNativeResourceScope()
    {
        var view = new MarkupNativeResourceView();
        var template = Assert.IsAssignableFrom<IDataTemplate>(view.FindResource("ResourceDataTemplate"));
        var first = Assert.IsType<Border>(template.Build("first"));
        var second = Assert.IsType<Border>(template.Build("second"));
        var light = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Light, Child = first };
        var dark = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark, Child = second };
        var scheme = new TonalSpotScheme(Colors.Red);
        var host = new StackPanel { Children = { light, dark } };
        host.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(first.Background));
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), MaterialColorTestHelper.BrushColor(second.Background));
        scheme.Color = Colors.Blue;
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(first.Background));
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), MaterialColorTestHelper.BrushColor(second.Background));
    }

    [AvaloniaFact]
    public void DeferredSharedBrush_StaysInDeclarationScopeRatherThanConsumerScope()
    {
        var view = new MarkupNativeResourceView();
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            var scheme = ((MaterialColorResources)view.Resources.MergedDictionaries[0]).Scheme!;
            var light = view.FindControl<Border>("SharedLightTarget")!;
            var dark = view.FindControl<Border>("SharedDarkTarget")!;
            var brush = Assert.IsType<SolidColorBrush>(light.Background);
            Assert.Same(brush, dark.Background);
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), brush.Color);
            dark.Resources[SysColorToken.Primary] = Colors.Gold;
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), brush.Color);
            scheme.Color = Colors.Blue;
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), brush.Color);
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), brush.Color);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void InlineThemeDictionaryBrushes_UseDeclaringHostsThemeInAvalonia12()
    {
        var view = new MarkupThemeDictionaryView();
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            var scheme = ((MaterialColorResources)view.Resources.MergedDictionaries[0]).Scheme!;
            var light = Assert.IsType<SolidColorBrush>(view.FindControl<Border>("LightTarget")!.Background);
            var dark = Assert.IsType<SolidColorBrush>(view.FindControl<Border>("DarkTarget")!.Background);
            Assert.NotSame(light, dark);
            // Inline native DynamicResource chooses the enclosing StyledElement anchor.
            // Avalonia 12.1 only preserves the dictionary variant for a provider anchor.
            // Both shared brushes consequently use the declaring view's actual theme.
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), light.Color);
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), dark.Color);
            scheme.Color = Colors.Blue;
            window.RequestedThemeVariant = ThemeVariant.Dark;
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), light.Color);
            Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), dark.Color);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ExternalThemeDictionaryBrushes_KeepDictionaryVariantOnSeedAndOwnerThemeChanges()
    {
        var resources = new ResourceInclude(new Uri("avares://MaterialColorUtilities.Tests/"))
        {
            Source = new Uri("avares://MaterialColorUtilities.Tests/Fixtures/MarkupExternalThemeResources.axaml")
        };
        var host = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Light };
        host.Resources.MergedDictionaries.Add(resources);
        Assert.True(host.TryFindResource("ThemedBrush", ThemeVariant.Dark, out var darkValue));
        Assert.True(host.TryFindResource("ThemedBrush", ThemeVariant.Light, out var lightValue));
        var dark = Assert.IsType<SolidColorBrush>(darkValue);
        var light = Assert.IsType<SolidColorBrush>(lightValue);
        var scheme = new TonalSpotScheme(Color.Parse("#6750A4"));
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), light.Color);
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), dark.Color);
        host.Resources["SeedColor"] = Colors.Blue;
        scheme.Color = Colors.Blue;
        host.RequestedThemeVariant = ThemeVariant.Dark;
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Light), light.Color);
        Assert.Equal(MaterialColorTestHelper.Primary(scheme, ThemeVariant.Dark), dark.Color);
    }

    [AvaloniaFact]
    public void CompiledDynamicResourceSeed_ResolvesAndUpdatesWithoutLookupRecursion()
    {
        var view = new MarkupDynamicSeedView();
        var target = view.FindControl<Border>("Target")!;
        var provider = Assert.IsType<MaterialColorResources>(view.Resources.MergedDictionaries[0]);
        Assert.Equal(Color.Parse("#6750A4"), provider.Scheme!.Color);
        Assert.Equal(MaterialColorTestHelper.Primary(provider.Scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));
        view.Resources["SeedColor"] = Colors.Blue;
        Assert.Equal(Colors.Blue, provider.Scheme.Color);
        Assert.Equal(MaterialColorTestHelper.Primary(provider.Scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));
        view.Resources.Remove("SeedColor");
        Assert.Null(provider.Scheme.Color);
        Assert.Null(target.Background);
        view.Resources["SeedColor"] = Colors.Red;
        Assert.Equal(MaterialColorTestHelper.Primary(provider.Scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));
    }

    [AvaloniaFact]
    public void CompiledExplicitSourceBinding_DoesNotDependOnConsumersDataContext()
    {
        var view = new MarkupExplicitSourceView { DataContext = new ThemeSettings { SeedColor = Colors.Green } };
        var settings = Assert.IsType<ThemeSettings>(view.FindResource("ThemeSettings"));
        var provider = Assert.IsType<MaterialColorResources>(view.Resources.MergedDictionaries[1]);
        var target = view.FindControl<Border>("Target")!;
        Assert.Equal(settings.SeedColor, provider.Scheme!.Color);
        settings.SeedColor = Colors.Blue;
        Assert.Equal(Colors.Blue, provider.Scheme.Color);
        Assert.Equal(MaterialColorTestHelper.Primary(provider.Scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));
    }

    [AvaloniaFact]
    public void CompiledElementNameSeed_UsesNativeAmbientAnchor()
    {
        var view = new MarkupElementNameSeedView();
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            var source = view.FindControl<NativeColorTarget>("SeedTarget")!;
            var target = view.FindControl<Border>("Target")!;
            var provider = Assert.IsType<MaterialColorResources>(view.Resources.MergedDictionaries[0]);
            Assert.Equal(source.Value, provider.Scheme!.Color);
            source.Value = Colors.Blue;
            Assert.Equal(Colors.Blue, provider.Scheme.Color);
            Assert.Equal(MaterialColorTestHelper.Primary(provider.Scheme, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(target.Background));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void CompiledExternalDictionary_UsesExplicitSourceAndFollowsOwnerChanges()
    {
        var resources = new ResourceInclude(new Uri("avares://MaterialColorUtilities.Tests/"))
        {
            Source = new Uri("avares://MaterialColorUtilities.Tests/Fixtures/MarkupExternalResources.axaml")
        };
        var first = new Border { DataContext = new ThemeSettings { SeedColor = Colors.Green } };
        var second = new Border();
        first.Resources.MergedDictionaries.Add(resources);
        first.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        var settings = Assert.IsType<ThemeSettings>(first.FindResource("ThemeSettings"));
        var expected = new TonalSpotScheme(settings.SeedColor);
        Assert.Equal(MaterialColorTestHelper.Primary(expected, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(first.Background));
        first.Resources.MergedDictionaries.Remove(resources);
        settings.SeedColor = Colors.Blue;
        second.Resources.MergedDictionaries.Add(resources);
        second.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        expected.Color = Colors.Blue;
        Assert.Equal(MaterialColorTestHelper.Primary(expected, ThemeVariant.Light), MaterialColorTestHelper.BrushColor(second.Background));
    }
}

public partial class MarkupNativeResourceView : UserControl
{
    public MarkupNativeResourceView() => AvaloniaXamlLoader.Load(this);
}
public partial class MarkupDynamicSeedView : UserControl
{
    public MarkupDynamicSeedView() => AvaloniaXamlLoader.Load(this);
}
public partial class MarkupExplicitSourceView : UserControl
{
    public MarkupExplicitSourceView() => AvaloniaXamlLoader.Load(this);
}
public partial class MarkupThemeDictionaryView : UserControl
{
    public MarkupThemeDictionaryView() => AvaloniaXamlLoader.Load(this);
}
public partial class MarkupElementNameSeedView : UserControl
{
    public MarkupElementNameSeedView() => AvaloniaXamlLoader.Load(this);
}
