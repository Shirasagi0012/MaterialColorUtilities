using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Markup;
using MaterialColorUtilities.Avalonia.Tokens;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

/// <summary>
/// Real Skia bitmap integration tests. The private gate/cache observations make queue ordering and
/// phase reuse assertions deterministic without adding instrumentation to the public API.
/// </summary>
public class ImageColorSourceTests(ITestOutputHelper output)
{
    [AvaloniaFact]
    public void NewAndClearedSource_HaveOnlyExplicitFallback()
    {
        var source = new ImageColorSource();
        Assert.NotNull(source.Result);
        Assert.Null(source.Result.SeedColor);
        Assert.Empty(source.Result.Candidates);
        Assert.False(source.Result.IsFallback);
        Assert.False(source.IsBusy);
        Assert.Null(source.Error);

        source.FallbackColor = Colors.Orange;
        Assert.Equal(Colors.Orange, source.Result.SeedColor);
        Assert.True(source.Result.IsFallback);
        Assert.Empty(source.Result.Candidates);
        source.FallbackColor = null;
        Assert.Null(source.Result.SeedColor);
        Assert.False(source.Result.IsFallback);
    }

    [AvaloniaFact]
    public void InitAndSameTurnChanges_CoalesceBeforeCapturingFinalOptions()
    {
        using var bitmap = CreateBitmap(Colors.Red, Colors.Blue);
        var source = new ImageColorSource();
        var successCount = CountSuccesses(source);
        source.BeginInit();
        source.BeginInit();
        source.Image = bitmap;
        source.MaxColors = 2; // Temporarily conflicts with the default DesiredColors=4.
        Dispatcher.UIThread.RunJobs();
        Assert.False(State<bool>(source, "_running"));
        Assert.Null(State<object?>(source, "_samples"));
        source.DesiredColors = 2;
        source.MaxSamplePixels = 64;
        source.MaxSamplePixels = 32;
        source.Filter = false;
        source.EndInit();
        Dispatcher.UIThread.RunJobs();
        Assert.False(State<bool>(source, "_running"));
        source.EndInit();
        WaitForIdle(source);

        Assert.Null(source.Error);
        Assert.NotEmpty(source.Result.Candidates);
        Assert.Equal(1, successCount());
        Assert.InRange(Assert.IsAssignableFrom<Array>(State<object>(source, "_samples")).Length, 1, 32);
        Assert.Throws<InvalidOperationException>(source.EndInit);
    }

    [AvaloniaFact]
    public void QueuedAToBToC_SkipsObsoleteDisposedInputsAndPublishesOnlyC()
    {
        using var first = CreateBitmap(Colors.Red);
        using var second = CreateBitmap(Colors.Green);
        using var latest = CreateBitmap(Colors.Blue);
        var source = new ImageColorSource();
        var successCount = CountSuccesses(source);
        var errors = 0;
        source.PropertyChanged += (_, change) =>
        {
            if (change.Property == ImageColorSource.ErrorProperty && source.Error is not null)
                errors++;
        };
        WithBlockedWorker(() =>
        {
            source.Image = first;
            WaitUntil(() => State<bool>(source, "_running"));
            first.Dispose();
            source.Image = second;
            second.Dispose();
            for (var i = 0; i < 100; i++)
                source.MaxSamplePixels = 64 + i;
            source.Image = latest;
            Dispatcher.UIThread.RunJobs();
            Assert.True(source.IsBusy);
            Assert.True(State<bool>(source, "_running"));
            Assert.True(State<bool>(source, "_pending"));
            Assert.Null(State<object?>(source, "_samples"));
        });
        WaitForIdle(source);

        Assert.Equal(Colors.Blue, source.Result.SeedColor);
        Assert.Equal(1, successCount());
        Assert.Equal(0, errors);
        Assert.Null(source.Error);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RequestAfterCapture_DropsObsoleteSuccessAndFailure(bool oldInputFails, bool clearImage)
    {
        using var oldInput = CreateBitmap(Colors.Red);
        using var latest = CreateBitmap(Colors.Blue);
        if (oldInputFails)
            oldInput.Dispose();
        var source = new ImageColorSource();
        var publishedErrors = 0;
        var publishedSeeds = new List<Color?>();
        source.PropertyChanged += (_, change) =>
        {
            if (change.Property == ImageColorSource.ErrorProperty && source.Error is not null)
                publishedErrors++;
            if (change.Property == ImageColorSource.ResultProperty && source.Result.Candidates.Count > 0)
                publishedSeeds.Add(source.Result.SeedColor);
        };
        WithBlockedWorker(() =>
        {
            source.Image = oldInput;
            WaitUntil(() => State<bool>(source, "_running"));
        });
        var capture = WaitForCaptureOperation();
        var captured = false;
        capture.Completed += (_, _) =>
        {
            captured = true;
            // The old capture has really succeeded/failed, but publication cannot yet run.
            source.Image = clearImage ? null : latest;
        };
        WaitForIdle(source);
        Assert.True(captured);
        Assert.Equal(0, publishedErrors);
        if (clearImage)
        {
            Assert.Empty(publishedSeeds);
            Assert.Null(source.Result.SeedColor);
            Assert.Null(State<object?>(source, "_samples"));
            Assert.Null(State<object?>(source, "_histogram"));
        }
        else
        {
            Assert.Equal(new Color?[] { Colors.Blue }, publishedSeeds);
            Assert.Equal(Colors.Blue, source.Result.SeedColor);
        }
        Assert.Null(source.Error);
    }

    [AvaloniaFact]
    public void ClearingImage_InvalidatesQueuedWorkAndAllCachedStages()
    {
        using var first = CreateBitmap(Colors.Red);
        using var pending = CreateBitmap(Colors.Blue);
        var source = new ImageColorSource { Image = first, FallbackColor = Colors.Orange };
        WaitForIdle(source);
        Assert.Equal(Colors.Red, source.Result.SeedColor);
        WithBlockedWorker(() =>
        {
            source.Image = pending;
            WaitUntil(() => State<bool>(source, "_running"));
            Assert.Equal(Colors.Red, source.Result.SeedColor);
            source.Image = null;
            Assert.False(source.IsBusy);
            Assert.Null(source.Error);
            Assert.Equal(Colors.Orange, source.Result.SeedColor);
            Assert.Empty(source.Result.Candidates);
            Assert.True(source.Result.IsFallback);
            Assert.Null(State<object?>(source, "_samples"));
            Assert.Null(State<object?>(source, "_histogram"));
            Assert.Null(State<object?>(source, "_scoredResult"));
        });
        WaitForIdle(source);
        Assert.Equal(Colors.Orange, source.Result.SeedColor);
        Assert.True(source.Result.IsFallback);
        AssertReadable(first);
        AssertReadable(pending);
    }

    [AvaloniaFact]
    public void AutoUpdateFalse_InvalidatesAutomaticWorkButRefreshRequestsOneExtraction()
    {
        using var first = CreateBitmap(Colors.Red);
        using var latest = CreateBitmap(Colors.Blue);
        var source = new ImageColorSource { AutoUpdate = false, Image = first };
        Dispatcher.UIThread.RunJobs();
        Assert.False(source.IsBusy);
        Assert.False(State<bool>(source, "_running"));
        Assert.Null(source.Result.SeedColor);
        source.Refresh();
        WaitForIdle(source);
        Assert.Equal(Colors.Red, source.Result.SeedColor);
        var result = source.Result;
        source.Image = latest;
        source.DesiredColors = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.Same(result, source.Result);
        Assert.False(source.IsBusy);
        Assert.False(State<bool>(source, "_running"));
        source.Refresh();
        WaitForIdle(source);
        Assert.Equal(Colors.Blue, source.Result.SeedColor);
        Assert.False(source.AutoUpdate);
    }

    [AvaloniaFact]
    public void TurningOffAutoUpdateWhileQueued_DiscardsTheAutomaticRequest()
    {
        using var bitmap = CreateBitmap(Colors.Blue);
        var source = new ImageColorSource();
        WithBlockedWorker(() =>
        {
            source.Image = bitmap;
            WaitUntil(() => State<bool>(source, "_running"));
            source.AutoUpdate = false;
            Assert.False(source.IsBusy);
            Assert.False(State<bool>(source, "_pending"));
        });
        WaitForIdle(source);
        Assert.Null(source.Result.SeedColor);
        Assert.Null(State<object?>(source, "_samples"));
        source.AutoUpdate = true;
        WaitForIdle(source);
        Assert.Equal(Colors.Blue, source.Result.SeedColor);
    }

    [AvaloniaFact]
    public void ReadFailure_PreservesPreviousValidResultThenSuccessfulRequestClearsError()
    {
        using var valid = CreateBitmap(Colors.Red);
        using var disposed = CreateBitmap(Colors.Green);
        using var recovery = CreateBitmap(Colors.Blue);
        var source = new ImageColorSource { Image = valid, FallbackColor = Colors.Orange };
        WaitForIdle(source);
        var previous = source.Result;
        disposed.Dispose();
        source.Image = disposed;
        WaitForIdle(source);
        Assert.NotNull(source.Error);
        Assert.Same(previous, source.Result);
        Assert.False(source.Result.IsFallback);
        source.Image = recovery;
        Assert.Same(previous, source.Result);
        WaitForIdle(source);
        Assert.Null(source.Error);
        Assert.Equal(Colors.Blue, source.Result.SeedColor);
    }

    [AvaloniaFact]
    public void FirstFailure_IsAnErrorEvenWhenExplicitFallbackIsPresent()
    {
        using var disposed = CreateBitmap(Colors.Red);
        disposed.Dispose();
        var source = new ImageColorSource { FallbackColor = Colors.Orange, Image = disposed };
        WaitForIdle(source);
        Assert.NotNull(source.Error);
        Assert.Empty(source.Result.Candidates);
        Assert.Equal(Colors.Orange, source.Result.SeedColor);
        Assert.True(source.Result.IsFallback);
        source.Image = null;
        Assert.Null(source.Error);
        Assert.False(source.IsBusy);
    }

    [AvaloniaFact]
    public void UnsupportedMutableBitmap_IsAnErrorAndIsNeverDisposedBySource()
    {
        using var mutable = new WriteableBitmap(new PixelSize(4, 4), new Vector(96, 96),
            PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        var source = new ImageColorSource { Image = mutable };
        WaitForIdle(source);
        Assert.IsAssignableFrom<NotSupportedException>(source.Error);
        Assert.Null(source.Result.SeedColor);
        source.Image = null;
        using var framebuffer = mutable.Lock();
        Assert.NotEqual(IntPtr.Zero, framebuffer.Address);
    }

    [AvaloniaFact]
    public void TransparentAndFilteredImages_AreSuccessfulEmptyResults()
    {
        using var transparent = CreateBitmap(Colors.Transparent);
        using var gray = CreateBitmap(Colors.Gray);
        var source = new ImageColorSource { Image = transparent };
        WaitForIdle(source);
        Assert.Null(source.Error);
        Assert.Null(source.Result.SeedColor);
        Assert.Empty(source.Result.Candidates);
        source.Image = gray;
        WaitForIdle(source);
        Assert.Null(source.Error);
        Assert.Null(source.Result.SeedColor);
        source.FallbackColor = Colors.Orange;
        WaitForIdle(source);
        Assert.True(source.Result.IsFallback);
        Assert.Equal(Colors.Orange, source.Result.SeedColor);
        Assert.Empty(source.Result.Candidates);
        source.Filter = false;
        WaitForIdle(source);
        Assert.Null(source.Error);
        Assert.Equal(Colors.Gray, source.Result.SeedColor);
        Assert.False(source.Result.IsFallback);
    }

    [AvaloniaFact]
    public void OptionChanges_InvalidateOnlyTheirDependentCachedStages()
    {
        using var bitmap = CreateBitmap(Colors.Red, Colors.Blue, Colors.Lime, Colors.Orange);
        var source = new ImageColorSource { Image = bitmap, Filter = false, MaxSamplePixels = 64 };
        WaitForIdle(source);
        var samples = State<object>(source, "_samples");
        var histogram = State<object>(source, "_histogram");
        var result = source.Result;

        source.FallbackColor = Colors.Purple;
        WaitForIdle(source);
        Assert.Same(samples, State<object>(source, "_samples"));
        Assert.Same(histogram, State<object>(source, "_histogram"));
        Assert.Same(result, source.Result);

        source.DesiredColors = 1;
        WaitForIdle(source);
        Assert.Same(samples, State<object>(source, "_samples"));
        Assert.Same(histogram, State<object>(source, "_histogram"));
        Assert.NotSame(result, source.Result);
        Assert.Single(source.Result.Candidates);
        result = source.Result;

        source.Filter = true;
        WaitForIdle(source);
        Assert.Same(samples, State<object>(source, "_samples"));
        Assert.Same(histogram, State<object>(source, "_histogram"));
        Assert.NotSame(result, source.Result);

        source.MaxColors = 16;
        WaitForIdle(source);
        Assert.Same(samples, State<object>(source, "_samples"));
        Assert.NotSame(histogram, State<object>(source, "_histogram"));
        histogram = State<object>(source, "_histogram");

        source.MaxSamplePixels = 32;
        WaitForIdle(source);
        Assert.NotSame(samples, State<object>(source, "_samples"));
        Assert.NotSame(histogram, State<object>(source, "_histogram"));
        Assert.Null(source.Error);
    }

    [AvaloniaFact]
    public void EmptyResultFallbackChanges_ReuseSamplingAndQuantization()
    {
        using var bitmap = CreateBitmap(Colors.Gray);
        var source = new ImageColorSource { Image = bitmap };
        WaitForIdle(source);
        var samples = State<object>(source, "_samples");
        var histogram = State<object>(source, "_histogram");
        source.FallbackColor = Colors.Red;
        WaitForIdle(source);
        Assert.Equal(Colors.Red, source.Result.SeedColor);
        Assert.True(source.Result.IsFallback);
        source.FallbackColor = Colors.Blue;
        WaitForIdle(source);
        Assert.Equal(Colors.Blue, source.Result.SeedColor);
        source.FallbackColor = null;
        WaitForIdle(source);
        Assert.Null(source.Result.SeedColor);
        Assert.False(source.Result.IsFallback);
        Assert.Empty(source.Result.Candidates);
        Assert.Same(samples, State<object>(source, "_samples"));
        Assert.Same(histogram, State<object>(source, "_histogram"));
    }

    [AvaloniaFact]
    public void SameBitmapAssignment_ReusesResultWhileRefreshInvalidatesEveryStage()
    {
        using var bitmap = CreateBitmap(Colors.Red);
        var source = new ImageColorSource { Image = bitmap };
        WaitForIdle(source);
        var samples = State<object>(source, "_samples");
        var histogram = State<object>(source, "_histogram");
        var result = source.Result;
        var generation = State<long>(source, "_generation");
        source.Image = bitmap;
        Dispatcher.UIThread.RunJobs();
        Assert.False(source.IsBusy);
        Assert.Equal(generation, State<long>(source, "_generation"));
        Assert.Same(result, source.Result);
        source.Refresh();
        Assert.Same(result, source.Result);
        Assert.True(source.IsBusy);
        WaitForIdle(source);
        Assert.NotSame(samples, State<object>(source, "_samples"));
        Assert.NotSame(histogram, State<object>(source, "_histogram"));
        Assert.NotSame(result, source.Result);
        Assert.Equal(Colors.Red, source.Result.SeedColor);
        source.Image = null;
        AssertReadable(bitmap);
    }

    [AvaloniaFact]
    public void InvalidCombinedOptions_ReportErrorAndRecoverAfterCorrection()
    {
        using var bitmap = CreateBitmap(Colors.Red);
        var source = new ImageColorSource { Image = bitmap, MaxColors = 1 };
        WaitForIdle(source);
        Assert.IsType<ArgumentOutOfRangeException>(source.Error);
        Assert.Null(source.Result.SeedColor);
        source.DesiredColors = 1;
        WaitForIdle(source);
        Assert.Null(source.Error);
        Assert.Equal(Colors.Red, source.Result.SeedColor);
        Assert.Throws<ArgumentException>(() => source.FallbackColor = Color.FromArgb(128, 1, 2, 3));
        Assert.Throws<ArgumentException>(() => source.MaxSamplePixels = 0);
        Assert.Throws<ArgumentException>(() => source.MaxColors = 257);
        Assert.Throws<ArgumentException>(() => source.DesiredColors = 17);
    }

    [AvaloniaFact]
    public void ResultNotificationMayRequestNewImageWithoutPublishingFalseIdle()
    {
        using var first = CreateBitmap(Colors.Red);
        using var next = CreateBitmap(Colors.Blue);
        var source = new ImageColorSource();
        var changed = false;
        var becameIdleBeforeLatest = false;
        source.PropertyChanged += (_, change) =>
        {
            if (change.Property == ImageColorSource.ResultProperty && !changed && source.Result.Candidates.Count > 0)
            {
                changed = true;
                source.Image = next;
            }
            if (change.Property == ImageColorSource.IsBusyProperty && changed && !source.IsBusy &&
                source.Result.SeedColor != Colors.Blue)
                becameIdleBeforeLatest = true;
        };
        source.Image = first;
        WaitForIdle(source);
        Assert.True(changed);
        Assert.False(becameIdleBeforeLatest);
        Assert.Equal(Colors.Blue, source.Result.SeedColor);
    }

    [AvaloniaTheory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void SharedSeedConsumersAndThemeSwitches_DoNotRepeatImageWork(int count)
    {
        using var bitmap = CreateBitmap(Colors.Red, Colors.Blue);
        var source = new ImageColorSource();
        var successCount = CountSuccesses(source);
        var schemes = new List<TonalSpotScheme>();
        var targets = new List<Border>();
        var scopes = new List<ThemeVariantScope>();
        var bindings = new List<IDisposable>();
        try
        {
            for (var i = 0; i < count; i++)
            {
                var scheme = new TonalSpotScheme();
                bindings.Add(scheme.Bind(ColorScheme.ColorProperty,
                    new ReflectionBinding("Result.SeedColor") { Source = source, Mode = BindingMode.OneWay }));
                var accent = new CustomColor { Name = "Shared", Harmonize = false };
                bindings.Add(accent.Bind(CustomColor.ColorProperty,
                    new ImageSeedExtension(source) { Index = 1 }.ProvideValue(null!)));
                scheme.CustomColors.Add(accent);
                var target = new Border();
                var scope = new ThemeVariantScope { Child = target, RequestedThemeVariant = ThemeVariant.Light };
                scope.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
                bindings.Add(target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary)));
                schemes.Add(scheme);
                scopes.Add(scope);
                targets.Add(target);
            }
            source.Image = bitmap;
            WaitForIdle(source);
            var samples = State<object>(source, "_samples");
            var histogram = State<object>(source, "_histogram");
            var result = source.Result;
            var generation = State<long>(source, "_generation");
            foreach (var scheme in schemes)
            {
                Assert.Equal(result.SeedColor, scheme.Color);
                Assert.Equal(result.Candidates.Count > 1 ? result.Candidates[1] : result.SeedColor,
                    scheme.CustomColors[0].Color);
            }
            foreach (var target in targets)
                Assert.IsAssignableFrom<ISolidColorBrush>(target.Background);
            foreach (var scope in scopes)
                scope.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();
            foreach (var scope in scopes)
                scope.RequestedThemeVariant = ThemeVariant.Light;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, successCount());
            Assert.Equal(generation, State<long>(source, "_generation"));
            Assert.Same(samples, State<object>(source, "_samples"));
            Assert.Same(histogram, State<object>(source, "_histogram"));
            Assert.Same(result, source.Result);
            Assert.False(source.IsBusy);
        }
        finally
        {
            foreach (var binding in bindings)
                binding.Dispose();
        }
    }

    /// <summary>Opt-in diagnostic, never a timing-sensitive CI gate.</summary>
    [AvaloniaTheory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void ReportSharedResultPublicationFanout(int consumerCount)
    {
        if (Environment.GetEnvironmentVariable("MCU_IMAGE_BENCHMARK") != "1")
        {
            output.WriteLine("Set MCU_IMAGE_BENCHMARK=1 to measure shared result publication fanout.");
            return;
        }

        const int warmups = 3;
        const int measurements = 20;
        var first = new ImageColorResult([Colors.Red, Colors.Blue]);
        var second = new ImageColorResult([Colors.Lime, Colors.Orange]);
        var publish = typeof(ImageColorSource).GetProperty(nameof(ImageColorSource.Result))!.GetSetMethod(true)!
            .CreateDelegate<Action<ImageColorSource, ImageColorResult>>();
        var source = new ImageColorSource { AutoUpdate = false };
        publish(source, first);
        var scheme = new TonalSpotScheme();
        var provider = new MaterialColorResources { Scheme = scheme };
        var panel = new StackPanel();
        var scope = new ThemeVariantScope { Child = panel, RequestedThemeVariant = ThemeVariant.Light };
        var bindings = new List<IDisposable>();
        var changedCount = 0;
        scheme.SchemeChanged += (_, _) => changedCount++;
        var providerGeneration = typeof(MaterialColorResources).GetField("_generation", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            // Exactly consumerCount seed bindings share one result: one scheme seed plus
            // consumerCount-1 custom seeds. Every seed has its own dynamic-resource brush target.
            bindings.Add(scheme.Bind(ColorScheme.ColorProperty,
                new ReflectionBinding("Result.SeedColor") { Source = source, Mode = BindingMode.OneWay }));
            for (var i = 1; i < consumerCount; i++)
            {
                var accent = new CustomColor { Name = $"Seed{i}", Harmonize = false };
                bindings.Add(accent.Bind(CustomColor.ColorProperty,
                    new ImageSeedExtension(source) { Index = 1 }.ProvideValue(null!)));
                scheme.CustomColors.Add(accent);
            }
            scope.Resources.MergedDictionaries.Add(provider);
            for (var i = 0; i < consumerCount; i++)
            {
                var target = new Border();
                panel.Children.Add(target);
                object token = i == 0 ? SysColorToken.Primary : new CustomColorKey($"Seed{i}", CustomColorRole.Color);
                bindings.Add(target.Bind(Border.BackgroundProperty, new DynamicResourceExtension(token)));
                Assert.IsAssignableFrom<ISolidColorBrush>(target.Background);
            }

            var milliseconds = new double[measurements];
            var allocations = new long[measurements];
            var schemeChanges = new int[measurements];
            var invalidations = new int[measurements];
            for (var i = -warmups; i < measurements; i++)
            {
                var next = (i & 1) == 0 ? first : second;
                var changesBefore = changedCount;
                var generationBefore = (int)providerGeneration.GetValue(provider)!;
                var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                var started = Stopwatch.GetTimestamp();
                publish(source, next);
                Dispatcher.UIThread.RunJobs();
                var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
                var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
                if (i >= 0)
                {
                    milliseconds[i] = elapsed;
                    allocations[i] = allocated;
                    schemeChanges[i] = changedCount - changesBefore;
                    invalidations[i] = (int)providerGeneration.GetValue(provider)! - generationBefore;
                }
                Assert.Equal(next.SeedColor, scheme.Color);
                Assert.All(scheme.CustomColors, custom => Assert.Equal(next.Candidates[1], custom.Color));
                Assert.All(panel.Children.Cast<Border>(), target => Assert.IsAssignableFrom<ISolidColorBrush>(target.Background));
            }

            Array.Sort(milliseconds);
            Array.Sort(allocations);
            var report = $"Shared result publication: {consumerCount} seed consumers, 1 provider, {consumerCount} brush targets; " +
                $"{warmups} warmups + {measurements} measurements. " +
                $"UI time median={Median(milliseconds):F3} ms, p95={milliseconds[(int)Math.Ceiling(measurements * 0.95) - 1]:F3} ms; " +
                $"current-thread managed allocation median={(allocations[9] + allocations[10]) / 2:N0} B, " +
                $"p95={allocations[(int)Math.Ceiling(measurements * 0.95) - 1]:N0} B; " +
                $"SchemeChanged/publication={schemeChanges.Min()}..{schemeChanges.Max()}, " +
                $"provider invalidations/publication={invalidations.Min()}..{invalidations.Max()}. " +
                "Includes native resource resolution and brush updates; excludes extraction and original decode.";
            output.WriteLine(report);
            var reportPath = Environment.GetEnvironmentVariable("MCU_IMAGE_FANOUT_REPORT");
            if (!string.IsNullOrEmpty(reportPath))
                File.AppendAllText(reportPath, report + Environment.NewLine);
            Assert.False(source.IsBusy);
            Assert.Null(State<object?>(source, "_samples"));
            Assert.Null(State<object?>(source, "_histogram"));
        }
        finally
        {
            foreach (var binding in bindings)
                binding.Dispose();
        }
    }

    private static double Median(double[] sorted) =>
        (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2;

    [AvaloniaFact]
    public void DispatcherPost_DoesNotKeepUnreferencedSourceAlive()
    {
        using var bitmap = CreateBitmap(Colors.Red);
        var weak = CreatePostedSource(bitmap);
        ForceCollection();
        Assert.False(weak.TryGetTarget(out _));
        Dispatcher.UIThread.RunJobs();
        AssertReadable(bitmap);
    }

    [AvaloniaFact]
    public void GlobalWorkerQueue_DoesNotKeepReleasedViewSourceOrBitmapAlive()
    {
        WeakReference<ImageColorSource> weakSource = null!;
        WeakReference<Border> weakView = null!;
        WeakReference<Bitmap> weakBitmap = null!;
        WithBlockedWorker(() =>
        {
            (weakSource, weakView, weakBitmap) = CreateQueuedView();
            ForceCollection();
            Assert.False(weakView.TryGetTarget(out _));
            Assert.False(weakSource.TryGetTarget(out _));
            Assert.False(weakBitmap.TryGetTarget(out _));
        });
        // The abandoned queue entry must release the shared permit for a live source as well.
        using var bitmap = CreateBitmap(Colors.Blue);
        var live = new ImageColorSource { Image = bitmap };
        WaitForIdle(live);
        Assert.Equal(Colors.Blue, live.Result.SeedColor);
    }

    [AvaloniaFact]
    public void InFlightWorkAndItsCompletionCallback_DoNotRetainSourceOrOriginalBitmap()
    {
        var (weakSource, weakBitmap) = CreateCapturedSource();
        // Do not pump the UI dispatcher: any completion callback remains queued during collection.
        ForceCollection();
        Assert.False(weakSource.TryGetTarget(out _));
        Assert.False(weakBitmap.TryGetTarget(out _));
        using var bitmap = CreateBitmap(Colors.Blue);
        var live = new ImageColorSource { Image = bitmap };
        WaitForIdle(live);
        Assert.Equal(Colors.Blue, live.Result.SeedColor);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<ImageColorSource> CreatePostedSource(Bitmap bitmap)
    {
        var source = new ImageColorSource { Image = bitmap };
        Assert.True(State<bool>(source, "_posted"));
        return new WeakReference<ImageColorSource>(source);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference<ImageColorSource>, WeakReference<Border>, WeakReference<Bitmap>) CreateQueuedView()
    {
        var bitmap = CreateBitmap(Colors.Red);
        var source = new ImageColorSource { Image = bitmap };
        var view = new Border();
        view.Resources.Add("ImageColors", source);
        var scheme = new TonalSpotScheme();
        scheme.Bind(ColorScheme.ColorProperty, new ReflectionBinding("Result.SeedColor") { Source = source });
        view.Resources.MergedDictionaries.Add(new MaterialColorResources { Scheme = scheme });
        view.Bind(Border.BackgroundProperty, new DynamicResourceExtension(SysColorToken.Primary));
        var parent = new StackPanel();
        parent.Children.Add(view);
        parent.Children.Remove(view);
        WaitUntil(() => State<bool>(source, "_running"));
        return (new WeakReference<ImageColorSource>(source), new WeakReference<Border>(view), new WeakReference<Bitmap>(bitmap));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference<ImageColorSource>, WeakReference<Bitmap>) CreateCapturedSource()
    {
        var bitmap = CreateNoiseBitmap();
        var source = new ImageColorSource { MaxSamplePixels = 65_536, MaxColors = 256 };
        WithBlockedWorker(() =>
        {
            source.Image = bitmap;
            WaitUntil(() => State<bool>(source, "_running"));
        });
        var capture = WaitForCaptureOperation();
        var captured = false;
        capture.Completed += (_, _) => captured = true;
        // Only invoke this queued capture. Its awaiter may run managed computation concurrently,
        // but any eventual completion is held at a lower priority until after the GC assertion.
        capture.Priority = DispatcherPriority.Send;
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Send);
        Assert.True(captured);
        Assert.True(State<bool>(source, "_running"));
        Assert.Null(State<object?>(source, "_samples")); // Completion has not published its cache.
        return (new WeakReference<ImageColorSource>(source), new WeakReference<Bitmap>(bitmap));
    }

    // Observe the real queued UI capture without running it. Dispatcher queue introspection is
    // deliberately confined to tests; no hook or counter is added to production extraction code.
    private static DispatcherOperation WaitForCaptureOperation()
    {
        var getJobs = typeof(Dispatcher).GetMethod("GetJobs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        var callbackField = typeof(DispatcherOperation).GetField("Callback", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var timeout = Stopwatch.StartNew();
        do
        {
            var jobs = (IEnumerable<DispatcherOperation>)getJobs.Invoke(Dispatcher.UIThread, null)!;
            var capture = jobs.FirstOrDefault(job => callbackField.GetValue(job) is Delegate callback &&
                callback.Method.ReturnType.DeclaringType == typeof(ImageColorSource));
            if (capture is not null)
                return capture;
            Thread.Sleep(1);
        } while (timeout.Elapsed < TimeSpan.FromSeconds(15));
        throw new TimeoutException("The image worker did not enqueue its UI capture.");
    }

    private static Func<int> CountSuccesses(ImageColorSource source)
    {
        var count = 0;
        source.PropertyChanged += (_, change) =>
        {
            if (change.Property == ImageColorSource.ResultProperty && source.Result.Candidates.Count > 0)
                count++;
        };
        return () => count;
    }

    private static void WaitForIdle(ImageColorSource source) => WaitUntil(() =>
        !source.IsBusy && !State<bool>(source, "_running") && !State<bool>(source, "_pending") &&
        !State<bool>(source, "_posted"));

    private static void WaitUntil(Func<bool> condition)
    {
        var timeout = Stopwatch.StartNew();
        do
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
                return;
            Thread.Sleep(1);
        } while (timeout.Elapsed < TimeSpan.FromSeconds(15));
        Assert.True(condition(), "Image source work did not reach the expected state within 15 seconds.");
    }

    private static void WithBlockedWorker(Action action)
    {
        WaitUntil(() => WorkerGate.Wait(0));
        try { action(); }
        finally { WorkerGate.Release(); }
    }

    private static SemaphoreSlim WorkerGate =>
        (SemaphoreSlim)typeof(ImageColorSource).GetField("WorkerGate", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;

    private static T State<T>(ImageColorSource source, string field) =>
        (T)typeof(ImageColorSource).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(source)!;

    private static void ForceCollection()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private static Bitmap CreateBitmap(params Color[] colors) => CreateBitmap(16, 16,
        (x, _) => colors[x * colors.Length / 16]);

    private static Bitmap CreateNoiseBitmap() => CreateBitmap(256, 256, (x, y) =>
        Color.FromRgb((byte)(x * 31 + y * 17), (byte)(x * 13 + y * 47), (byte)(x * 71 + y * 7)));

    private static Bitmap CreateBitmap(int width, int height, Func<int, int, Color> pixel)
    {
        var bytes = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var color = pixel(x, y);
            var offset = (y * width + x) * 4;
            bytes[offset] = color.B;
            bytes[offset + 1] = color.G;
            bytes[offset + 2] = color.R;
            bytes[offset + 3] = color.A;
        }
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul, pointer,
                new PixelSize(width, height), new Vector(96, 96), width * 4);
        }
        finally { Marshal.FreeHGlobal(pointer); }
    }

    private static void AssertReadable(Bitmap bitmap)
    {
        var stride = checked(bitmap.PixelSize.Width * 4);
        var size = checked(stride * bitmap.PixelSize.Height);
        var pointer = Marshal.AllocHGlobal(size);
        try { bitmap.CopyPixels(new PixelRect(bitmap.PixelSize), pointer, size, stride); }
        finally { Marshal.FreeHGlobal(pointer); }
    }
}
