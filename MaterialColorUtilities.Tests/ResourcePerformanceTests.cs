using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Tokens;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

/// <summary>Repeatable, diagnostic-only measurements, not timing-sensitive performance gates.</summary>
public class ResourcePerformanceTests(ITestOutputHelper output)
{
    [AvaloniaFact]
    public void ReportFirstCachedAndBatchThemeResolution()
    {
        var scheme = new TonalSpotScheme(Colors.Teal);
        object key = SysColorToken.Primary;
        var warmed = new MaterialColorResources { Scheme = scheme };
        Assert.True(warmed.TryGetResource(key, ThemeVariant.Light, out _));
        Assert.True(warmed.TryGetResource(key, ThemeVariant.Dark, out _));

        const int firstCount = 100;
        var providers = Enumerable.Range(0, firstCount)
            .Select(_ => new MaterialColorResources { Scheme = scheme }).ToArray();
        var allocationStart = GC.GetAllocatedBytesForCurrentThread();
        var timer = Stopwatch.StartNew();
        foreach (var provider in providers)
            Assert.True(provider.TryGetResource(key, ThemeVariant.Light, out _));
        timer.Stop();
        output.WriteLine($"First resolution ({firstCount} providers, JIT warmed): {timer.Elapsed.TotalMilliseconds:F3} ms; " +
            $"{(GC.GetAllocatedBytesForCurrentThread() - allocationStart) / firstCount} bytes/provider.");

        const int cachedCount = 100_000;
        allocationStart = GC.GetAllocatedBytesForCurrentThread();
        timer.Restart();
        for (var i = 0; i < cachedCount; i++)
            warmed.TryGetResource(key, (i & 1) == 0 ? ThemeVariant.Light : ThemeVariant.Dark, out _);
        timer.Stop();
        output.WriteLine($"Cached resolution ({cachedCount}, alternating Light/Dark): {timer.Elapsed.TotalMilliseconds:F3} ms; " +
            $"{(GC.GetAllocatedBytesForCurrentThread() - allocationStart) / cachedCount} bytes/lookup.");

        const int consumerCount = 200;
        const int switches = 50;
        var panel = new StackPanel();
        var scope = new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Light, Child = panel };
        scope.Resources.MergedDictionaries.Add(warmed);
        for (var i = 0; i < consumerCount; i++)
        {
            var target = new Border();
            panel.Children.Add(target);
            target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(key));
        }

        allocationStart = GC.GetAllocatedBytesForCurrentThread();
        timer.Restart();
        for (var i = 0; i < switches; i++)
            scope.RequestedThemeVariant = (i & 1) == 0 ? ThemeVariant.Dark : ThemeVariant.Light;
        timer.Stop();
        output.WriteLine($"Native DynamicResource batch ({consumerCount} IBrush targets, {switches} theme switches): " +
            $"{timer.Elapsed.TotalMilliseconds:F3} ms; {GC.GetAllocatedBytesForCurrentThread() - allocationStart} bytes total.");

        warmed.TryGetResource(key, ThemeVariant.Light, out var expected);
        foreach (var target in panel.Children.Cast<Border>())
            Assert.Equal(Assert.IsType<Color>(expected), Assert.IsAssignableFrom<ISolidColorBrush>(target.Background).Color);
    }
}
