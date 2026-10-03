using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MaterialColorUtilities.Avalonia;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

/// <summary>Opt-in stage measurements on real Skia bitmaps; never a wall-clock CI gate.</summary>
/// <remarks>
/// Set MCU_IMAGE_BENCHMARK=1 and optionally MCU_BENCHMARK_PHOTO to a real raster photo.
/// MCU_IMAGE_BENCHMARK_OUTPUT selects the CSV path. Decoding and fixture construction are
/// excluded from stage timings/allocations. Managed allocations are per executing thread,
/// summed across the measured stages; Task scheduling and test-harness allocations are excluded.
/// Process working set includes managed/native memory and is not a native-allocation measure.
/// </remarks>
public class ImageColorPerformanceTests(ITestOutputHelper output)
{
    private const int Warmups = 3;
    private const int Measurements = 20;

    [AvaloniaFact]
    public async Task OptInReportCaptureQuantizeScoreAndProcessMemory()
    {
        if (Environment.GetEnvironmentVariable("MCU_IMAGE_BENCHMARK") != "1")
        {
            output.WriteLine("Image benchmark not run. Set MCU_IMAGE_BENCHMARK=1 to enable it.");
            return;
        }

        var path = Environment.GetEnvironmentVariable("MCU_IMAGE_BENCHMARK_OUTPUT")
            ?? Path.Combine(Path.GetTempPath(), "material-image-benchmark.csv");
        var photo = Environment.GetEnvironmentVariable("MCU_BENCHMARK_PHOTO");
        var workloads = new List<(string Name, Func<Bitmap> Create, bool DefaultOnly)>();
        if (!string.IsNullOrWhiteSpace(photo))
        {
            Assert.True(File.Exists(photo), $"MCU_BENCHMARK_PHOTO does not exist: {photo}");
            workloads.Add(("photo", () => new Bitmap(photo), false));
        }
        else
            output.WriteLine("Photographic workload omitted: MCU_BENCHMARK_PHOTO is unset. Synthetic inputs are not a photo substitute.");
        workloads.Add(("transparent-png", () => CreatePattern(3840, 2160, Pattern.Transparent, pngRoundTrip: true), false));
        workloads.Add(("noise", () => CreatePattern(3840, 2160, Pattern.Noise), false));
        workloads.Add(("extreme-wide", () => CreatePattern(131072, 8, Pattern.ColorBands), false));
        workloads.Add(("grayscale", () => CreatePattern(3840, 2160, Pattern.Grayscale), false));
        foreach (var (width, height) in new[] { (64, 64), (512, 512), (3840, 2160), (7680, 4320) })
        foreach (var pattern in new[] { Pattern.Smooth, Pattern.Noise })
            workloads.Add(($"size-sweep-{pattern.ToString().ToLowerInvariant()}-{width}x{height}",
                () => CreatePattern(width, height, pattern), true));

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var writer = new StreamWriter(path, append: false);
        writer.AutoFlush = true;
        var header = "workload,width,height,sample_limit,max_colors,sample_width,sample_height,opaque_samples,candidates," +
            "warmups,measurements,capture_ms_median,capture_ms_p95,quantize_ms_median,quantize_ms_p95," +
            "score_ms_median,score_ms_p95,background_ms_median,background_ms_p95," +
            "capture_managed_bytes_median,capture_managed_bytes_p95,quantize_managed_bytes_median,quantize_managed_bytes_p95," +
            "score_managed_bytes_median,score_managed_bytes_p95,total_stage_managed_bytes_median,total_stage_managed_bytes_p95," +
            "process_working_set_before_bytes,process_working_set_snapshot_max_bytes,process_reported_peak_working_set_bytes," +
            "glibc_malloc_baseline_bytes,glibc_malloc_sampled_peak_bytes,glibc_malloc_sampled_peak_delta_bytes";
        await writer.WriteLineAsync(header);
        output.WriteLine(header);
        output.WriteLine("Time: monotonic stopwatch; p95: nearest rank. Managed bytes: current execution thread within each stage. " +
            "Capture runs on UI thread; quantization/scoring run on a worker. Working set is whole-process managed+native; " +
            "the reported process peak is not a per-case or native-only allocation measure. On Linux/glibc, mallinfo2 is sampled about every 1 ms " +
            "to report uordblks+hblkhd; it excludes other allocators/runtime mmap and can miss short-lived peaks. " +
            "Unavailable allocator counters are -1. These are not exact total native peaks.");

        foreach (var (name, create, defaultOnly) in workloads)
        {
            using var bitmap = create();
            foreach (var sampleLimit in defaultOnly ? new[] { 16384 } : new[] { 4096, 16384, 65536 })
            foreach (var maxColors in defaultOnly ? new[] { 128 } : new[] { 64, 128, 256 })
            {
                var options = new ImageColorOptions { MaxSamplePixels = sampleLimit, MaxColors = maxColors };
                var size = ImageColorExtractor.GetSampleSize(bitmap.PixelSize, sampleLimit);
                Assert.InRange(checked(size.Width * size.Height), 1, sampleLimit);
                for (var i = 0; i < Warmups; i++)
                    await RunOnce(bitmap, options);

                using var process = Process.GetCurrentProcess();
                process.Refresh();
                var workingSetBefore = process.WorkingSet64;
                var workingSetMax = workingSetBefore;
                var peakWorkingSet = process.PeakWorkingSet64;
                using var native = MallocSampler.TryStart();
                var rows = new Iteration[Measurements];
                for (var i = 0; i < Measurements; i++)
                {
                    rows[i] = await RunOnce(bitmap, options);
                    process.Refresh();
                    workingSetMax = Math.Max(workingSetMax, process.WorkingSet64);
                    peakWorkingSet = Math.Max(peakWorkingSet, process.PeakWorkingSet64);
                }
                native?.Stop();

                var fields = new List<string>
                {
                    name, N(bitmap.PixelSize.Width), N(bitmap.PixelSize.Height), N(sampleLimit), N(maxColors),
                    N(size.Width), N(size.Height), N(rows[0].OpaqueSamples), N(rows[0].Candidates), N(Warmups), N(Measurements)
                };
                AddStats(fields, rows.Select(row => row.CaptureMs));
                AddStats(fields, rows.Select(row => row.QuantizeMs));
                AddStats(fields, rows.Select(row => row.ScoreMs));
                AddStats(fields, rows.Select(row => row.QuantizeMs + row.ScoreMs));
                AddStats(fields, rows.Select(row => (double)row.CaptureBytes));
                AddStats(fields, rows.Select(row => (double)row.QuantizeBytes));
                AddStats(fields, rows.Select(row => (double)row.ScoreBytes));
                AddStats(fields, rows.Select(row => (double)(row.CaptureBytes + row.QuantizeBytes + row.ScoreBytes)));
                fields.AddRange([N(workingSetBefore), N(workingSetMax), N(peakWorkingSet)]);
                fields.AddRange([N(native?.Baseline ?? -1), N(native?.Peak ?? -1),
                    N(native is null ? -1 : native.Peak - native.Baseline)]);
                var line = string.Join(',', fields);
                await writer.WriteLineAsync(line);
                output.WriteLine(line);
            }
        }

        output.WriteLine($"Image benchmark CSV: {Path.GetFullPath(path)}");
    }

    [AvaloniaFact]
    public void OptInCompareResizeInterpolation()
    {
        if (Environment.GetEnvironmentVariable("MCU_IMAGE_RESIZE_COMPARE") != "1")
        {
            output.WriteLine("Resize comparison not run. Set MCU_IMAGE_RESIZE_COMPARE=1 to enable it.");
            return;
        }

        var path = Environment.GetEnvironmentVariable("MCU_IMAGE_RESIZE_OUTPUT")
            ?? Path.Combine(Path.GetTempPath(), "material-image-resize-comparison.csv");
        var photo = Environment.GetEnvironmentVariable("MCU_BENCHMARK_PHOTO");
        var workloads = new List<(string Name, Func<Bitmap> Create)>();
        if (!string.IsNullOrWhiteSpace(photo))
        {
            Assert.True(File.Exists(photo), $"MCU_BENCHMARK_PHOTO does not exist: {photo}");
            workloads.Add(("photo", () => new Bitmap(photo)));
        }
        else
            output.WriteLine("Photographic resize comparison omitted: MCU_BENCHMARK_PHOTO is unset.");
        workloads.Add(("noise-8k", () => CreatePattern(7680, 4320, Pattern.Noise)));

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var writer = new StreamWriter(path, append: false);
        writer.AutoFlush = true;
        var header = "workload,width,height,quality,sample_width,sample_height,warmups,measurements," +
            "resize_ms_median,resize_ms_p95,resize_managed_bytes_median,resize_managed_bytes_p95," +
            "glibc_malloc_baseline_bytes,glibc_malloc_sampled_peak_bytes,glibc_malloc_sampled_peak_delta_bytes,candidates";
        writer.WriteLine(header);
        output.WriteLine(header);
        output.WriteLine("Diagnostic comparison only: production remains MediumQuality. Timed region is CreateScaledBitmap only; " +
            "disposal, extraction and scoring are outside it. Candidate lists use normal filtered scoring on the bounded resize. " +
            "Allocator values sample Linux/glibc mallinfo2 at about 1 ms and can miss short peaks; -1 means unavailable.");

        foreach (var (name, create) in workloads)
        {
            using var bitmap = create();
            var size = ImageColorExtractor.GetSampleSize(bitmap.PixelSize, 16_384);
            foreach (var quality in new[] { BitmapInterpolationMode.LowQuality, BitmapInterpolationMode.MediumQuality })
            {
                for (var i = 0; i < Warmups; i++)
                    using (bitmap.CreateScaledBitmap(size, quality)) { }
                var timings = new double[Measurements];
                var allocations = new double[Measurements];
                using var native = MallocSampler.TryStart();
                for (var i = 0; i < Measurements; i++)
                {
                    var measured = Measure(() => bitmap.CreateScaledBitmap(size, quality));
                    using var resized = measured.Value;
                    timings[i] = measured.Milliseconds;
                    allocations[i] = measured.Bytes;
                }
                native?.Stop();
                using var preview = bitmap.CreateScaledBitmap(size, quality);
                var colors = ImageColorExtractor.Extract(preview);
                var fields = new List<string>
                {
                    name, N(bitmap.PixelSize.Width), N(bitmap.PixelSize.Height), quality.ToString(),
                    N(size.Width), N(size.Height), N(Warmups), N(Measurements)
                };
                AddStats(fields, timings);
                AddStats(fields, allocations);
                fields.AddRange([N(native?.Baseline ?? -1), N(native?.Peak ?? -1),
                    N(native is null ? -1 : native.Peak - native.Baseline),
                    string.Join('|', colors.Candidates.Select(color => color.ToString()))]);
                var line = string.Join(',', fields);
                writer.WriteLine(line);
                output.WriteLine(line);
            }
        }
        output.WriteLine($"Resize comparison CSV: {Path.GetFullPath(path)}");
    }

    private static async Task<Iteration> RunOnce(Bitmap bitmap, ImageColorOptions options)
    {
        var capture = Measure(() => ImageColorExtractor.Capture(bitmap, options));
        // Only independently owned samples and immutable options cross to the worker.
        var samples = capture.Value;
        var background = await Task.Run(() =>
        {
            var quantize = Measure(() => ImageColorExtractor.Quantize(samples, options.MaxColors));
            var score = Measure(() => ImageColorExtractor.Score(quantize.Value, options));
            return (quantize.Milliseconds, quantize.Bytes, ScoreMs: score.Milliseconds,
                ScoreBytes: score.Bytes, Candidates: score.Value.Candidates.Count);
        });
        return new Iteration(samples.Length, background.Candidates, capture.Milliseconds, background.Milliseconds,
            background.ScoreMs, capture.Bytes, background.Bytes, background.ScoreBytes);
    }

    private static (T Value, double Milliseconds, long Bytes) Measure<T>(Func<T> action)
    {
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        var started = Stopwatch.GetTimestamp();
        var value = action();
        var milliseconds = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        return (value, milliseconds, GC.GetAllocatedBytesForCurrentThread() - allocated);
    }

    private static void AddStats(List<string> fields, IEnumerable<double> values)
    {
        var ordered = values.Order().ToArray();
        var median = (ordered[(ordered.Length - 1) / 2] + ordered[ordered.Length / 2]) / 2;
        var p95 = ordered[(int)Math.Ceiling(ordered.Length * 0.95) - 1];
        fields.Add(median.ToString("F3", CultureInfo.InvariantCulture));
        fields.Add(p95.ToString("F3", CultureInfo.InvariantCulture));
    }

    private static string N(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static Bitmap CreatePattern(int width, int height, Pattern pattern, bool pngRoundTrip = false)
    {
        var pixels = new byte[checked(width * height * 4)];
        uint state = 0x5EED1234;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = checked((y * width + x) * 4);
            state = unchecked(state * 1664525u + 1013904223u);
            pixels[index] = pattern == Pattern.Grayscale ? (byte)(x % 256) : (byte)(state >> 24);
            pixels[index + 1] = pattern == Pattern.Grayscale ? pixels[index] : (byte)(y * 255L / Math.Max(1, height - 1));
            pixels[index + 2] = pattern == Pattern.Grayscale ? pixels[index] : (byte)(x * 255L / Math.Max(1, width - 1));
            if (pattern == Pattern.Noise)
            {
                pixels[index + 1] = (byte)(state >> 16);
                pixels[index + 2] = (byte)(state >> 8);
            }
            if (pattern == Pattern.ColorBands)
            {
                pixels[index] = (byte)((x / 2048 % 4) * 85);
                pixels[index + 1] = (byte)((x / 1024 % 4) * 85);
                pixels[index + 2] = (byte)((x / 512 % 4) * 85);
            }
            if (pattern == Pattern.Smooth)
                pixels[index] = (byte)((pixels[index + 1] + pixels[index + 2]) / 2);
            // Most of this PNG is transparent, with a translucent strip and an opaque island.
            pixels[index + 3] = pattern != Pattern.Transparent ? (byte)255
                : x < width / 10 && y < height / 2 ? (byte)255
                : x > width * 9 / 10 ? (byte)128 : (byte)0;
        }

        Bitmap bitmap;
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            bitmap = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul, handle.AddrOfPinnedObject(),
                new PixelSize(width, height), new Vector(96, 96), checked(width * 4));
        }
        finally { handle.Free(); }
        if (!pngRoundTrip)
            return bitmap;
        using (bitmap)
        using (var stream = new MemoryStream())
        {
            bitmap.Save(stream, new PngBitmapEncoderOptions());
            stream.Position = 0;
            return new Bitmap(stream);
        }
    }

    private enum Pattern { Transparent, Noise, ColorBands, Grayscale, Smooth }
    private readonly record struct Iteration(int OpaqueSamples, int Candidates,
        double CaptureMs, double QuantizeMs, double ScoreMs, long CaptureBytes, long QuantizeBytes, long ScoreBytes);

    private sealed class MallocSampler : IDisposable
    {
        private readonly Thread _thread;
        private int _stop;
        private long _peak;

        private MallocSampler(long baseline)
        {
            Baseline = _peak = baseline;
            _thread = new Thread(() =>
            {
                while (Volatile.Read(ref _stop) == 0)
                {
                    _peak = Math.Max(_peak, ReadInUse());
                    Thread.Sleep(1);
                }
            }) { IsBackground = true, Name = "Image benchmark native sampler" };
            _thread.Start();
        }

        internal long Baseline { get; }
        internal long Peak => Interlocked.Read(ref _peak);

        internal static MallocSampler? TryStart()
        {
            if (!OperatingSystem.IsLinux())
                return null;
            try { return new MallocSampler(ReadInUse()); }
            catch (DllNotFoundException) { return null; }
            catch (EntryPointNotFoundException) { return null; }
        }

        internal void Stop()
        {
            if (Interlocked.Exchange(ref _stop, 1) == 0)
                _thread.Join();
        }

        public void Dispose() => Stop();

        private static long ReadInUse()
        {
            var info = MallInfo2();
            return checked((long)(info.Uordblks + info.Hblkhd));
        }

        [DllImport("libc", EntryPoint = "mallinfo2")]
        private static extern MallocInfo MallInfo2();

        [StructLayout(LayoutKind.Sequential)]
        private struct MallocInfo
        {
            public nuint Arena, Ordblks, Smblks, Hblks, Hblkhd, Usmblks, Fsmblks, Uordblks, Fordblks, Keepcost;
        }
    }
}
