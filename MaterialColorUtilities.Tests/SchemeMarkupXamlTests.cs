using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class SchemeMarkupXamlTests
{
    private static readonly string[] Variants = ["TonalSpot", "Content", "Expressive", "Fidelity", "FruitSalad", "Monochrome", "Neutral", "Rainbow", "Vibrant", "Cmf"];

    [AvaloniaFact]
    public void CompiledSchemeShorthandMatchesObjectElementsForAllVariantsAndUpdates()
    {
        var view = new SchemeMarkupView();
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark, ThemeVariant.Light })
            {
                window.RequestedThemeVariant = theme;
                foreach (var seed in new[] { Color.Parse("#6750A4"), Colors.Blue })
                foreach (var variant in Variants)
                {
                    var shorthand = view.FindControl<Border>(variant + "String")!;
                    var element = view.FindControl<Border>(variant + "Object")!;
                    var scheme = Scheme(shorthand);
                    var objectScheme = Scheme(element);
                    Assert.Equal(objectScheme.GetType(), scheme.GetType());
                    Assert.Equal(objectScheme.SpecVersion, scheme.SpecVersion);
                    scheme.Color = objectScheme.Color = seed;
                    Assert.Equal(MaterialColorTestHelper.Primary(scheme, theme), MaterialColorTestHelper.BrushColor(shorthand.Background));
                    Assert.Equal(MaterialColorTestHelper.BrushColor(element.Background), MaterialColorTestHelper.BrushColor(shorthand.Background));
                }
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void CompiledPositionalAndNamedExplicitBindingsAndDynamicResourcesUpdate()
    {
        var view = new SchemeMarkupView();
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            foreach (var seed in new[] { Colors.Red, Colors.Blue })
            {
                window.RequestedThemeVariant = theme;
                view.Seed = seed;
                view.Resources["SeedColor"] = seed;
                foreach (var name in new[] { "PositionalBinding", "NamedBinding", "DynamicSeed" })
                {
                    var target = view.FindControl<Border>(name)!;
                    var scheme = Scheme(target);
                    Assert.Equal(seed, scheme.Color);
                    Assert.Equal(MaterialColorTestHelper.Primary(scheme, theme), MaterialColorTestHelper.BrushColor(target.Background));
                }
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void CompiledImplicitBindingUsesItsDeclarationAnchorAndTracksDataContext()
    {
        var view = new SchemeMarkupView();
        var window = new Window { Content = view };
        try
        {
            view.DataContext = new { Seed = Colors.Red };
            window.Show();
            Assert.Equal(Colors.Red, Scheme(view.FindControl<Border>("ImplicitBinding")!).Color);
            Assert.Equal(Colors.Red, Scheme(view.FindControl<Border>("ReadmeBinding")!).Color);
            view.DataContext = new { Seed = Colors.Blue };
            Assert.Equal(Colors.Blue, Scheme(view.FindControl<Border>("ImplicitBinding")!).Color);
            Assert.Equal(Colors.Blue, Scheme(view.FindControl<Border>("ReadmeBinding")!).Color);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ProgrammaticBindingDoesNotAcquireAnOwnerDataContext()
    {
        Assert.Throws<InvalidOperationException>(() => new TonalSpotScheme(new ReflectionBinding("Seed")));
        var scheme = new TonalSpotScheme(new ReflectionBinding("Seed") { Source = new { Seed = Colors.Red } });
        Assert.Equal(Colors.Red, scheme.Color);
    }

    [AvaloniaFact]
    public void ExternalDictionaryImplicitBindingDoesNotAcquireTheConsumersDataContext()
    {
        var resources = new ResourceInclude(new Uri("avares://MaterialColorUtilities.Tests/"))
        {
            Source = new Uri("avares://MaterialColorUtilities.Tests/Fixtures/ExternalSchemeBinding.axaml")
        };
        var target = new Border { DataContext = new { Seed = Colors.Red } };
        target.Resources.MergedDictionaries.Add(resources);
        var dictionary = Assert.IsType<ResourceDictionary>(resources.Loaded);
        Assert.Null(((MaterialColorResources)dictionary.MergedDictionaries[0]).Scheme!.Color);
    }

    [AvaloniaFact]
    public void CmfStillRequiresSpec2026AndAcceptsSecondarySeed()
    {
        var scheme = new CmfScheme("#6750A4");
        Assert.Throws<InvalidOperationException>(() => scheme.CreateScheme(ThemeVariant.Light));
        scheme.SpecVersion = global::MaterialColorUtilities.DynamicColors.ColorSpec.SpecVersion.Spec2026;
        scheme.SecondaryColor = Colors.Green;
        Assert.NotNull(scheme.CreateScheme(ThemeVariant.Light));
        Assert.NotNull(scheme.CreateScheme(ThemeVariant.Dark));
    }

    [AvaloniaFact]
    public void EverySchemeColorConstructorMatchesStringConstructor()
    {
        foreach (var name in Variants)
        {
            var type = typeof(ColorScheme).Assembly.GetType($"MaterialColorUtilities.Avalonia.{name}Scheme")!;
            var fromColor = (ColorScheme)Activator.CreateInstance(type, Colors.Red)!;
            var fromString = (ColorScheme)Activator.CreateInstance(type, "Red")!;
            Assert.Equal(fromString.Color, fromColor.Color);
            Assert.Equal(fromString.SpecVersion, fromColor.SpecVersion);
        }
    }

    [Fact]
    public void ReadmeCoreSnippetUsesTheCurrentApi()
    {
        var seed = global::MaterialColorUtilities.HCT.Hct.From(new global::MaterialColorUtilities.Utils.ArgbColor(0xFF6750A4u));
        var scheme = new global::MaterialColorUtilities.Scheme.SchemeTonalSpot(seed, isDark: false, contrastLevel: 0);
        global::MaterialColorUtilities.Utils.ArgbColor primary = scheme.Primary;
        global::MaterialColorUtilities.Utils.ArgbColor onPrimary = scheme.OnPrimary;
        Assert.NotEqual(primary, onPrimary);
    }

    private static ColorScheme Scheme(Border target) => ((MaterialColorResources)target.Resources.MergedDictionaries[0]).Scheme!;
}

public partial class SchemeMarkupView : UserControl
{
    public static readonly StyledProperty<Color> SeedProperty = AvaloniaProperty.Register<SchemeMarkupView, Color>(nameof(Seed), Colors.Red);
    public Color Seed { get => GetValue(SeedProperty); set => SetValue(SeedProperty, value); }
    public SchemeMarkupView() => AvaloniaXamlLoader.Load(this);
}
