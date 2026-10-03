using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Gallery.Views;
using MaterialColorUtilities.Tests.Avalonia.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests;

public class GalleryImageSeedTests
{
    [AvaloniaFact]
    public void CompiledDemoSharesAnExplicitSourceAndKeepsItsSchemeScoped()
    {
        var view = new ImageSeedDemoView { DataContext = new object() };
        var source = Source(view);
        // Keep extraction paused to isolate the compiled bindings and fallback behavior.
        source.AutoUpdate = false;
        using var host = new WindowHost(view);
        var provider = view.Resources.MergedDictionaries.OfType<MaterialColorResources>().Single();
        var scheme = Assert.IsType<TonalSpotScheme>(provider.Scheme);
        var custom = Assert.Single(scheme.CustomColors);

        Assert.Equal(Color.Parse("#6750A4"), scheme.Color);
        Assert.Equal("ImageAccent", custom.Name);
        Assert.Equal(Color.Parse("#006C4C"), custom.Color);
        Assert.True(custom.Harmonize);
        Assert.Same(source.Image, view.FindControl<Image>("SampleImage")!.Source);
        Assert.Same(source.Result.Candidates, view.FindControl<ItemsControl>("CandidateItems")!.ItemsSource);

        source.FallbackColor = Colors.Coral;
        Assert.Equal(Colors.Coral, scheme.Color);
        Assert.Equal(Color.Parse("#006C4C"), custom.Color);
        Assert.True(view.FindControl<TextBlock>("FallbackLabel")!.IsVisible);
        var result = source.Result;
        var image = source.Image;
        var light = MaterialColorTestHelper.BrushColor(view.FindControl<Border>("ImagePrimaryBand")!.Background);
        host.Window.RequestedThemeVariant = ThemeVariant.Dark;
        Assert.NotEqual(light, MaterialColorTestHelper.BrushColor(view.FindControl<Border>("ImagePrimaryBand")!.Background));
        Assert.Same(result, source.Result);
        Assert.Same(image, source.Image);
        Assert.False(source.IsBusy);
    }

    [AvaloniaFact]
    public async Task RealRasterCandidatesUpdateBothConsumersAndSurviveSwitchClearAndReattach()
    {
        var view = new ImageSeedDemoView { Width = 830, VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top };
        var source = Source(view);
        var panel = new StackPanel { Children = { view } };
        using var host = new WindowHost(panel);
        var scheme = Assert.IsType<TonalSpotScheme>(
            view.Resources.MergedDictionaries.OfType<MaterialColorResources>().Single().Scheme);
        var custom = Assert.Single(scheme.CustomColors);

        await WaitForExtraction(source);
        Assert.True(source.Result.Candidates.Count >= 2);
        Assert.Equal(source.Result.SeedColor, scheme.Color);
        Assert.Equal(source.Result.Candidates[1], custom.Color);
        Assert.False(source.Result.IsFallback);
        var coastSeed = source.Result.SeedColor;
        var coastResult = source.Result;
        host.Window.RequestedThemeVariant = ThemeVariant.Dark;
        Assert.Same(coastResult, source.Result);
        Assert.False(source.IsBusy);
        host.Window.RequestedThemeVariant = ThemeVariant.Light;

        if (Environment.GetEnvironmentVariable("MCU_GALLERY_IMAGE_SNAPSHOT") is { Length: > 0 } snapshot)
        {
            host.Window.UpdateLayout();
            using var rendered = new RenderTargetBitmap(PixelSize.FromSize(view.Bounds.Size, 1));
            rendered.Render(view);
            rendered.Save(snapshot, PngBitmapEncoderOptions.Default);
        }

        Click(view, "DuskButton");
        await WaitForExtraction(source);
        Assert.NotEqual(coastSeed, source.Result.SeedColor);
        Assert.True(source.Result.Candidates.Count >= 2);
        Assert.Equal(source.Result.SeedColor, scheme.Color);
        Assert.Equal(source.Result.Candidates[1], custom.Color);

        // Invalidate a request before its queued capture, then allow older work to finish.
        Click(view, "CoastButton");
        Click(view, "DuskButton");
        Click(view, "ClearImageButton");
        await Task.Delay(50);
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(source.Result.Candidates);
        Assert.True(source.Result.IsFallback);
        Assert.Equal(Color.Parse("#6750A4"), scheme.Color);
        Assert.Equal(Color.Parse("#006C4C"), custom.Color);
        Assert.Null(source.Error);

        Click(view, "CoastButton");
        panel.Children.Remove(view);
        panel.Children.Add(view);
        await WaitForExtraction(source);
        Assert.Equal(coastSeed, source.Result.SeedColor);
        Assert.Equal(source.Result.Candidates[1], custom.Color);
    }

    [AvaloniaFact]
    public void RepeatedSwitchAndClearReleaseOldInputsAndRestoreExplicitFallbacks()
    {
        var view = new ImageSeedDemoView();
        var source = Source(view);
        source.AutoUpdate = false;
        using var host = new WindowHost(view);

        for (var cycle = 0; cycle < 3; cycle++)
        {
            var coast = Assert.IsType<Bitmap>(source.Image);
            Click(view, "DuskButton");
            var dusk = Assert.IsType<Bitmap>(source.Image);
            Assert.NotSame(coast, dusk);
            AssertDisposed(coast);
            Assert.Same(dusk, view.FindControl<Image>("SampleImage")!.Source);

            Click(view, "ClearImageButton");
            Assert.Null(source.Image);
            Assert.Null(view.FindControl<Image>("SampleImage")!.Source);
            AssertDisposed(dusk);
            Assert.Empty(source.Result.Candidates);
            Assert.True(source.Result.IsFallback);
            Assert.Equal(Color.Parse("#6750A4"), source.Result.SeedColor);
            Assert.Null(source.Error);
            Assert.False(source.IsBusy);
            Click(view, "CoastButton");
        }
    }

    [AvaloniaFact]
    public void DetachDisposesInputAndReattachReloadsSelectionWithoutRevivingClearedInput()
    {
        var view = new ImageSeedDemoView();
        var source = Source(view);
        source.AutoUpdate = false;
        var panel = new StackPanel { Children = { view } };
        using var host = new WindowHost(panel);
        Click(view, "DuskButton");

        for (var cycle = 0; cycle < 3; cycle++)
        {
            var before = Assert.IsType<Bitmap>(source.Image);
            panel.Children.Remove(view);
            Assert.Null(source.Image);
            AssertDisposed(before);
            panel.Children.Add(view);
            var after = Assert.IsType<Bitmap>(source.Image);
            Assert.NotSame(before, after);
            Assert.Same(after, view.FindControl<Image>("SampleImage")!.Source);
        }

        Click(view, "ClearImageButton");
        panel.Children.Remove(view);
        panel.Children.Add(view);
        Assert.Null(source.Image);
        Assert.True(source.Result.IsFallback);
    }

    private static ImageColorSource Source(ImageSeedDemoView view)
    {
        Assert.True(view.Resources.TryGetResource("DemoImageColors", null, out var source));
        return Assert.IsType<ImageColorSource>(source);
    }

    private static void Click(ImageSeedDemoView view, string name) =>
        view.FindControl<Button>(name)!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    private static void AssertDisposed(Bitmap bitmap) =>
        Assert.Throws<ObjectDisposedException>(() => bitmap.PixelSize);

    private static async Task WaitForExtraction(ImageColorSource source)
    {
        var timeout = DateTime.UtcNow.AddSeconds(10);
        while (source.IsBusy && DateTime.UtcNow < timeout)
            await Task.Delay(10);
        Assert.False(source.IsBusy);
        Assert.Null(source.Error);
        Assert.NotEmpty(source.Result.Candidates);
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(Control content)
        {
            Window = new Window { Content = content, Width = 900, Height = 600, RequestedThemeVariant = ThemeVariant.Light };
            Window.Show();
            Window.UpdateLayout();
        }

        public Window Window { get; }
        public void Dispose() => Window.Close();
    }
}
