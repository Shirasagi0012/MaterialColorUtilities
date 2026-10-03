using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Markup;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class ImageSeedTests
{
    private static readonly Color SourceFallback = Color.Parse("#6750A4");
    private static readonly Color ConsumerFallback = Color.Parse("#006C4C");

    [AvaloniaFact]
    public void ProvideValueCreatesOnlyAOneWayBindingToTheWholeResult()
    {
        var source = new ImageColorSource { AutoUpdate = false };
        var initial = source.Result;
        var extension = new ImageSeedExtension(source) { Index = 1, FallbackColor = ConsumerFallback };

        var binding = extension.ProvideValue(null!);

        Assert.Equal(nameof(ImageColorSource.Result), binding.Path);
        Assert.Equal(BindingMode.OneWay, binding.Mode);
        Assert.Same(source, binding.Source);
        Assert.Same(initial, source.Result);
        Assert.False(source.IsBusy);
        Assert.Null(source.Error);

        // A binding keeps the options it was created with even if the extension is reused.
        extension.Index = 0;
        extension.FallbackColor = Colors.Gold;
        Assert.Equal(Colors.Blue, Convert(binding, new ImageColorResult([Colors.Red, Colors.Blue], null)));
        Assert.Equal(ConsumerFallback, Convert(binding, new ImageColorResult([Colors.Red], null)));
        Assert.Throws<NotSupportedException>(() => binding.Converter!.ConvertBack(
            Colors.Red, typeof(ImageColorResult), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaFact]
    public void MissingRankUsesConsumerFallbackThenSourceSeedThenNull()
    {
        var source = new ImageColorSource { AutoUpdate = false };
        var withFallback = new ImageSeedExtension(source)
        {
            Index = int.MaxValue,
            FallbackColor = ConsumerFallback
        }.ProvideValue(null!);
        var withoutFallback = new ImageSeedExtension(source) { Index = 1 }.ProvideValue(null!);

        Assert.Equal(ConsumerFallback, Convert(withFallback, new ImageColorResult([Colors.Red], SourceFallback)));
        Assert.Equal(ConsumerFallback, Convert(withFallback, new ImageColorResult([], SourceFallback)));
        Assert.Equal(Colors.Red, Convert(withoutFallback, new ImageColorResult([Colors.Red], SourceFallback)));
        Assert.Equal(SourceFallback, Convert(withoutFallback, new ImageColorResult([], SourceFallback)));
        Assert.Null(Convert(withoutFallback, new ImageColorResult([], null)));
        Assert.Equal(Colors.Blue, Convert(withoutFallback, new ImageColorResult([Colors.Red, Colors.Blue], null)));
    }

    [AvaloniaFact]
    public void InvalidSourcesAndOptionsAreRejectedWhenTheMarkupIsLoaded()
    {
        Assert.Throws<ArgumentException>(() => new ImageSeedExtension(null!));
        Assert.Throws<ArgumentException>(() => new ImageSeedExtension(new object()));
        var source = new ImageColorSource { AutoUpdate = false };
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ImageSeedExtension(source) { Index = -1 }.ProvideValue(null!));
        Assert.Throws<ArgumentException>(() =>
            new ImageSeedExtension(source) { FallbackColor = Colors.Transparent }.ProvideValue(null!));

        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ImageSeedNegativeIndexView().FindResource("InvalidColor"));
        Assert.ThrowsAny<ArgumentException>(() => new ImageSeedWrongSourceView().FindResource("InvalidColor"));
        Assert.ThrowsAny<ArgumentException>(() => new ImageSeedTransparentFallbackView().FindResource("InvalidColor"));
    }

    [AvaloniaFact]
    public void CompiledResourceCompositionTracksRanksAndExplicitSourceFallback()
    {
        var view = new ImageSeedView { DataContext = new { Result = "unrelated consumer context" } };
        var source = Assert.IsType<ImageColorSource>(view.FindResource("CoverColors"));
        Assert.Same(ImageSeedFixtureAssets.CoverBitmap, source.Image);
        Assert.Equal(16_384, source.MaxSamplePixels);
        Assert.Equal(128, source.MaxColors);
        Assert.Equal(4, source.DesiredColors);
        Assert.Equal(SourceFallback, source.FallbackColor);
        Assert.False(source.AutoUpdate);
        var provider = Assert.IsType<MaterialColorResources>(view.Resources.MergedDictionaries[1]);
        var scheme = Assert.IsType<TonalSpotScheme>(provider.Scheme);
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            foreach (var candidates in CandidateTransitions())
            {
                var result = new ImageColorResult(candidates, SourceFallback);
                Publish(source, result);
                Assert.Same(result, source.Result);
                Assert.Equal(result.SeedColor, scheme.Color);
                Assert.Equal(candidates.Length > 1 ? candidates[1] : ConsumerFallback, scheme.CustomColors[0].Color);
                Assert.Equal(candidates.Length > 1 ? candidates[1] : result.SeedColor, scheme.CustomColors[1].Color);
                Assert.Equal(result.SeedColor, scheme.CustomColors[2].Color);
                Assert.True(scheme.CustomColors[0].Harmonize);

                foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
                {
                    window.RequestedThemeVariant = theme;
                    AssertResourceBrushes(view, theme);
                    Assert.Same(result, source.Result);
                    Assert.False(source.IsBusy);
                }
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void CompiledNullSeedsRemoveResourcesAndRecoverWithoutStaleIndexedValues()
    {
        var view = new ImageSeedView();
        var source = Assert.IsType<ImageColorSource>(view.FindResource("CoverColors"));
        source.FallbackColor = null;
        var provider = Assert.IsType<MaterialColorResources>(view.Resources.MergedDictionaries[1]);
        var scheme = Assert.IsType<TonalSpotScheme>(provider.Scheme);
        var window = new Window { Content = view, RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            window.Show();
            foreach (var candidates in CandidateTransitions())
            {
                var result = new ImageColorResult(candidates, null);
                Publish(source, result);
                Assert.Equal(result.SeedColor, scheme.Color);
                Assert.Equal(candidates.Length > 1 ? candidates[1] : ConsumerFallback, scheme.CustomColors[0].Color);
                Assert.Equal(candidates.Length > 1 ? candidates[1] : result.SeedColor, scheme.CustomColors[1].Color);
                Assert.Equal(result.SeedColor, scheme.CustomColors[2].Color);

                if (candidates.Length == 0)
                {
                    Assert.Null(scheme.Color);
                    Assert.Null(scheme.CustomColors[1].Color);
                    Assert.Null(scheme.CustomColors[2].Color);
                    Assert.False(provider.TryGetResource(SysColorToken.Primary, ThemeVariant.Light, out _));
                    Assert.False(provider.TryGetResource(new CustomColorKey("CoverAccent", CustomColorRole.Container),
                        ThemeVariant.Light, out _));
                    foreach (var name in new[] { "SystemColor", "CustomColor", "PaletteColor" })
                        Assert.Null(view.FindControl<Border>(name)!.Background);
                }
                else
                    AssertResourceBrushes(view, ThemeVariant.Light);
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void EmptyCustomSeedIsSkippedWhileAFixedSchemeAndExplicitFallbackStayAvailable()
    {
        var source = new ImageColorSource { AutoUpdate = false };
        var selected = new CustomColor { Name = "Selected", Harmonize = false };
        var fallback = new CustomColor { Name = "Fallback", Harmonize = false };
        using var selectedBinding = selected.Bind(CustomColor.ColorProperty,
            new ImageSeedExtension(source) { Index = 1 }.ProvideValue(null!));
        using var fallbackBinding = fallback.Bind(CustomColor.ColorProperty,
            new ImageSeedExtension(source) { Index = 1, FallbackColor = ConsumerFallback }.ProvideValue(null!));
        var scheme = new TonalSpotScheme(SourceFallback) { CustomColors = { selected, fallback } };
        var provider = new MaterialColorResources { Scheme = scheme };
        var selectedKey = new CustomColorKey("Selected", CustomColorRole.Color);
        var fallbackKey = new CustomColorKey("Fallback", CustomColorRole.Color);

        foreach (var candidates in CandidateTransitions())
        {
            Publish(source, new ImageColorResult(candidates, null));
            Assert.Equal(candidates.Length != 0, provider.TryGetResource(selectedKey, ThemeVariant.Light, out _));
            Assert.True(provider.TryGetResource(fallbackKey, ThemeVariant.Light, out _));
            Assert.True(provider.TryGetResource(SysColorToken.Primary, ThemeVariant.Light, out _));
        }
    }

    private static Color[][] CandidateTransitions() =>
        [[Colors.Red], [Colors.Red, Colors.Blue], [Colors.Gold], [], [Colors.Green, Colors.Purple]];

    private static object? Convert(ReflectionBinding binding, ImageColorResult result) =>
        binding.Converter!.Convert(result, typeof(Color?), null, CultureInfo.InvariantCulture);

    private static void Publish(ImageColorSource source, ImageColorResult result)
    {
        // Keep scheduling/extraction out of binding tests without exposing a publication API.
        typeof(ImageColorSource).GetProperty(nameof(ImageColorSource.Result))!
            .GetSetMethod(nonPublic: true)!.Invoke(source, [result]);
    }

    private static void AssertResourceBrushes(ImageSeedView view, ThemeVariant theme)
    {
        var keys = new (string Name, object Key)[]
        {
            ("SystemColor", SysColorToken.Primary),
            ("CustomColor", new CustomColorKey("CoverAccent", CustomColorRole.Container)),
            ("PaletteColor", new RefPaletteKey(RefPaletteToken.Primary, 60))
        };
        foreach (var (name, key) in keys)
            Assert.Equal(MaterialColorTestHelper.ReadColor(view, key, theme),
                MaterialColorTestHelper.BrushColor(view.FindControl<Border>(name)!.Background));
    }
}

public partial class ImageSeedView : UserControl
{
    public ImageSeedView() => AvaloniaXamlLoader.Load(this);
}

public partial class ImageSeedNegativeIndexView : UserControl
{
    public ImageSeedNegativeIndexView() => AvaloniaXamlLoader.Load(this);
}

public partial class ImageSeedWrongSourceView : UserControl
{
    public ImageSeedWrongSourceView() => AvaloniaXamlLoader.Load(this);
}

public partial class ImageSeedTransparentFallbackView : UserControl
{
    public ImageSeedTransparentFallbackView() => AvaloniaXamlLoader.Load(this);
}

public static class ImageSeedFixtureAssets
{
    // Test-owned immutable resource, shared only to validate compiled StaticResource composition.
    // No extraction runs against the headless platform bitmap.
    public static Bitmap CoverBitmap { get; } = CreateBitmap();

    private static Bitmap CreateBitmap()
    {
        var pixel = Marshal.AllocHGlobal(4);
        try
        {
            Marshal.WriteInt32(pixel, unchecked((int)0xFFFF0000));
            return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, pixel,
                new PixelSize(1, 1), new Vector(96, 96), 4);
        }
        finally { Marshal.FreeHGlobal(pixel); }
    }
}

public sealed class ImageSeedBitmapExtension
{
    public Bitmap ProvideValue(IServiceProvider serviceProvider) => ImageSeedFixtureAssets.CoverBitmap;
}
