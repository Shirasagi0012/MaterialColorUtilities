using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Utils;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia;

public class ImageColorExtractorTests
{
    [Fact]
    public void OptionsEnforceAllBudgetsAndOpaqueFallback()
    {
        new ImageColorOptions().Validate();
        new ImageColorOptions { MaxSamplePixels = 1, MaxColors = 1, DesiredColors = 1 }.Validate();
        new ImageColorOptions { MaxSamplePixels = 65_536, MaxColors = 256, DesiredColors = 16 }.Validate();
        foreach (var options in new[]
        {
            new ImageColorOptions { MaxSamplePixels = 0 },
            new ImageColorOptions { MaxSamplePixels = 65_537 },
            new ImageColorOptions { MaxColors = 0 },
            new ImageColorOptions { MaxColors = 257 },
            new ImageColorOptions { DesiredColors = 0 },
            new ImageColorOptions { DesiredColors = 17 },
            new ImageColorOptions { MaxColors = 3, DesiredColors = 4 }
        })
            Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
        Assert.Throws<ArgumentException>(() => new ImageColorOptions { FallbackColor = Colors.Transparent }.Validate());
    }

    [Fact]
    public void SampleDimensionsStayBoundedWithoutUpscalingEvenForThinImages()
    {
        foreach (var width in new[] { 1, 2, 127, 4096, 65_537, int.MaxValue })
        foreach (var height in new[] { 1, 3, 128, 2160, 65_537, int.MaxValue })
        foreach (var budget in new[] { 1, 2, 128, 4096, 16_384, 65_536 })
        {
            var sample = ImageColorExtractor.GetSampleSize(new PixelSize(width, height), budget);
            Assert.InRange(sample.Width, 1, width);
            Assert.InRange(sample.Height, 1, height);
            Assert.InRange((long)sample.Width * sample.Height, 1, budget);
            if ((long)width * height <= budget)
                Assert.Equal(new PixelSize(width, height), sample);
        }
        Assert.Equal(new PixelSize(160, 90), ImageColorExtractor.GetSampleSize(new PixelSize(3840, 2160), 14_400));
        Assert.Equal(new PixelSize(65_536, 1), ImageColorExtractor.GetSampleSize(new PixelSize(int.MaxValue, 1), 65_536));
        Assert.Throws<ArgumentException>(() => ImageColorExtractor.GetSampleSize(new PixelSize(0, 1), 1));
    }

    [Theory]
    [InlineData(true, AlphaFormat.Premul)]
    [InlineData(false, AlphaFormat.Premul)]
    [InlineData(true, AlphaFormat.Unpremul)]
    [InlineData(false, AlphaFormat.Unpremul)]
    public void ByteDecodingUsesFormatAndStrideAndRejectsEveryNonopaquePixel(bool bgra, AlphaFormat alpha)
    {
        var format = bgra ? PixelFormats.Bgra8888 : PixelFormats.Rgba8888;
        byte[] bytes = [10, 20, 30, 255, 1, 2, 3, 254, 91, 92, 93, 94,
                         40, 50, 60, 0, 70, 80, 90, 255, 95, 96, 97, 98];
        var colors = ImageColorExtractor.DecodeSamples(bytes, new PixelSize(2, 2), 12, format, alpha);
        Assert.Equal(new[]
        {
            new ArgbColor(255, bgra ? (byte)30 : (byte)10, 20, bgra ? (byte)10 : (byte)30),
            new ArgbColor(255, bgra ? (byte)90 : (byte)70, 80, bgra ? (byte)70 : (byte)90)
        }, colors);
    }

    [Fact]
    public void OpaqueMetadataDoesNotInterpretUnusedAlphaStorage()
    {
        var samples = ImageColorExtractor.DecodeSamples([10, 20, 30, 0], new PixelSize(1, 1), 4,
            PixelFormats.Rgba8888, AlphaFormat.Opaque);
        Assert.Equal(new ArgbColor(255, 10, 20, 30), Assert.Single(samples));
        Assert.Throws<NotSupportedException>(() => ImageColorExtractor.DecodeSamples(
            [0, 0, 0, 0], new PixelSize(1, 1), 4, PixelFormats.Rgb565, AlphaFormat.Opaque));
        Assert.Throws<NotSupportedException>(() => ImageColorExtractor.DecodeSamples(
            [0, 0, 0, 0], new PixelSize(1, 1), 4, PixelFormats.Rgba8888, (AlphaFormat)99));
        Assert.Throws<ArgumentException>(() => ImageColorExtractor.DecodeSamples(
            [0, 0, 0], new PixelSize(1, 1), 4, PixelFormats.Rgba8888, AlphaFormat.Unpremul));
    }

    [Fact]
    public void ResultDefensivelyCopiesCandidatesAndNeverAddsFallbackToThem()
    {
        Color[] candidates = [Colors.Red, Colors.Blue];
        var result = new ImageColorResult(candidates, Colors.Red);
        candidates[0] = Colors.Green;
        Assert.Equal(Colors.Red, result.SeedColor);
        Assert.False(result.IsFallback);
        Assert.Equal(new[] { Colors.Red, Colors.Blue }, result.Candidates);
        Assert.Throws<NotSupportedException>(() => ((IList<Color>)result.Candidates)[0] = Colors.Green);
        Assert.Same(result, result.WithFallback(Colors.Purple));

        var empty = ImageColorResult.CreateEmpty();
        Assert.Null(empty.SeedColor);
        Assert.Empty(empty.Candidates);
        Assert.False(empty.IsFallback);
        var fallback = empty.WithFallback(Colors.Red);
        Assert.Equal(Colors.Red, fallback.SeedColor);
        Assert.Empty(fallback.Candidates);
        Assert.True(fallback.IsFallback);
        Assert.Null(fallback.WithFallback(null).SeedColor);
    }

    [Fact]
    public void EmptyAndFilteredHistogramsDoNotLeakScoresDefaultFallback()
    {
        var gray = new Dictionary<ArgbColor, int> { [new(0xff777777u)] = 20 };
        var empty = ImageColorExtractor.Score(gray, new ImageColorOptions());
        Assert.Empty(empty.Candidates);
        Assert.Null(empty.SeedColor);
        Assert.False(empty.IsFallback);
        var fallback = ImageColorExtractor.Score(gray, new ImageColorOptions { FallbackColor = Colors.Red });
        Assert.Empty(fallback.Candidates);
        Assert.Equal(Colors.Red, fallback.SeedColor);
        Assert.True(fallback.IsFallback);
        var unfiltered = ImageColorExtractor.Score(gray, new ImageColorOptions { Filter = false });
        Assert.Single(unfiltered.Candidates);
        Assert.Equal(Color.Parse("#777777"), unfiltered.SeedColor);
        Assert.False(unfiltered.IsFallback);
        Assert.Empty(ImageColorExtractor.Quantize([], 128));
        Assert.Null(ImageColorExtractor.Score(new Dictionary<ArgbColor, int>(), new ImageColorOptions()).SeedColor);
    }

    [AvaloniaTheory]
    [InlineData(true, AlphaFormat.Premul)]
    [InlineData(false, AlphaFormat.Premul)]
    [InlineData(true, AlphaFormat.Unpremul)]
    [InlineData(false, AlphaFormat.Unpremul)]
    public void RealSkiaBitmapExtractionRespectsFormatAlphaAndCallerOwnership(bool bgra, AlphaFormat alpha)
    {
        // Distinct R and B values make a byte-order bug visible. Alpha 128 pixels never contribute.
        var format = bgra ? PixelFormats.Bgra8888 : PixelFormats.Rgba8888;
        byte[] bytes = bgra ? [20, 40, 220, 255, 100, 20, 40, 128] : [220, 40, 20, 255, 40, 20, 100, 128];
        using var bitmap = CreateBitmap(bytes, new PixelSize(2, 1), 8, format, alpha);
        var options = new ImageColorOptions { Filter = false };
        var samples = ImageColorExtractor.Capture(bitmap, options);
        Assert.Equal(new ArgbColor(255, 220, 40, 20), Assert.Single(samples));
        var result = ImageColorExtractor.Extract(bitmap, options);
        Assert.Equal(Color.FromRgb(220, 40, 20), result.SeedColor);
        Assert.False(result.IsFallback);
        // A second extraction proves the caller-owned bitmap was not disposed by the first.
        Assert.Equal(result.SeedColor, ImageColorExtractor.Extract(bitmap, options).SeedColor);
    }

    [AvaloniaFact]
    public void RealSkiaTransparentAndSemitransparentBitmapsHaveNoSeed()
    {
        foreach (var alpha in new byte[] { 0, 1, 127, 128, 254 })
        {
            using var bitmap = CreateBitmap([100, 50, 25, alpha], new PixelSize(1, 1), 4,
                PixelFormats.Rgba8888, AlphaFormat.Unpremul);
            var result = ImageColorExtractor.Extract(bitmap, new ImageColorOptions { Filter = false });
            Assert.Empty(result.Candidates);
            Assert.Null(result.SeedColor);
            Assert.False(result.IsFallback);
        }
    }

    [AvaloniaFact]
    public void RealSkiaSamplingIsBoundedForExtremeAspectRatio()
    {
        const int width = 100_000;
        var bytes = new byte[width * 4];
        for (var i = 0; i < bytes.Length; i += 4)
        {
            bytes[i] = 240;
            bytes[i + 3] = 255;
        }
        using var bitmap = CreateBitmap(bytes, new PixelSize(width, 1), width * 4, PixelFormats.Rgba8888, AlphaFormat.Unpremul);
        var samples = ImageColorExtractor.Capture(bitmap, new ImageColorOptions { MaxSamplePixels = 127 });
        Assert.Equal(127, samples.Length);
        Assert.All(samples, color => Assert.Equal(new ArgbColor(255, 240, 0, 0), color));
    }

    [AvaloniaFact]
    public async Task RejectsMutableRenderTargetsDisposedInputsAndOffThreadCapture()
    {
        using var writeable = new WriteableBitmap(new PixelSize(1, 1), new Vector(96, 96));
        using var renderTarget = new RenderTargetBitmap(new PixelSize(1, 1));
        Assert.Throws<NotSupportedException>(() => ImageColorExtractor.Extract(writeable));
        Assert.Throws<NotSupportedException>(() => ImageColorExtractor.Extract(renderTarget));
        using var bitmap = CreateBitmap([255, 0, 0, 255], new PixelSize(1, 1), 4,
            PixelFormats.Rgba8888, AlphaFormat.Unpremul);
        await Task.Run(() => Assert.Throws<InvalidOperationException>(() => ImageColorExtractor.Extract(bitmap)));
        bitmap.Dispose();
        Assert.ThrowsAny<Exception>(() => ImageColorExtractor.Extract(bitmap));
        Assert.Throws<ArgumentNullException>(() => ImageColorExtractor.Extract(null!));
    }

    private static Bitmap CreateBitmap(byte[] bytes, PixelSize size, int stride, PixelFormat format, AlphaFormat alphaFormat)
    {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return new Bitmap(format, alphaFormat, handle.AddrOfPinnedObject(), size, new Vector(96, 96), stride); }
        finally { handle.Free(); }
    }
}
