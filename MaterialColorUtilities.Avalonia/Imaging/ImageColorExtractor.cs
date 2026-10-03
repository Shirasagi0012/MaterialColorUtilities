using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MaterialColorUtilities.Quantize;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Avalonia;

/// <summary>Synchronously extracts ranked Material colors from a decoded static bitmap.</summary>
/// <remarks>
/// Call on the UI thread and keep the caller-owned bitmap alive until this method returns.
/// The supported input is an ordinary immutable raster <see cref="Bitmap"/> on the Skia
/// backend, with BGRA8888 or RGBA8888 pixels. Mutable and render-target bitmaps are rejected.
/// Only opaque pixels in the bounded sample contribute colors; no background is composited.
/// The sample limit does not bound original decoding, platform resize workspaces, or quantization memory.
/// </remarks>
public static class ImageColorExtractor
{
    /// <summary>Captures, quantizes, and scores an image before returning its immutable result.</summary>
    /// <exception cref="ArgumentNullException">The image is null.</exception>
    /// <exception cref="ArgumentException">An option is invalid.</exception>
    /// <exception cref="NotSupportedException">The bitmap cannot be safely sampled.</exception>
    /// <exception cref="InvalidOperationException">The method is called off the UI thread.</exception>
    public static ImageColorResult Extract(Bitmap image, ImageColorOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(image);
        options ??= new ImageColorOptions();
        options.Validate();
        var samples = Capture(image, options);
        return Score(Quantize(samples, options.MaxColors), options);
    }

    // Capture is the only stage that touches Avalonia objects or native pixels. The other
    // stages can run on a worker with their own managed input and immutable option snapshot.
    internal static ArgbColor[] Capture(Bitmap image, ImageColorOptions options)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        Dispatcher.UIThread.VerifyAccess();

        // Reject subclasses too: their virtual CopyPixels implementation has unknown costs.
        if (image.GetType() != typeof(Bitmap))
            throw new NotSupportedException(
                "Image extraction requires an ordinary static Bitmap. WriteableBitmap, " +
                "RenderTargetBitmap, and custom bitmap subclasses are not supported. " +
                "Provide a separately created, immutable raster snapshot.");

        ValidateFormat(image.Format, image.AlphaFormat);
        var sampleSize = GetSampleSize(image.PixelSize, options.MaxSamplePixels);

        // Always take the platform's scaling path, even for a same-sized small image.
        // Skia accepts only ImmutableBitmap here and returns the requested bounded bitmap.
        // Its internal resize workspace/time can still depend on original image dimensions. Do not use CopyPixels(ILockedFramebuffer): that overload can render an
        // unreadable input to a full-original-size RenderTargetBitmap behind the scenes.
        using var sample = image.CreateScaledBitmap(sampleSize, BitmapInterpolationMode.MediumQuality);
        if (sample.PixelSize != sampleSize)
            throw new NotSupportedException("The bitmap backend did not honor the bounded sample size.");

        ValidateFormat(sample.Format, sample.AlphaFormat);
        var stride = checked(sampleSize.Width * 4);
        var bytes = new byte[checked(stride * sampleSize.Height)];
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            // This non-rendering overload respects the source framebuffer's real row stride
            // while copying into our bounded, explicitly specified destination stride.
            sample.CopyPixels(new PixelRect(sampleSize), handle.AddrOfPinnedObject(), bytes.Length, stride);
        }
        finally
        {
            handle.Free();
        }

        return DecodeSamples(bytes, sampleSize, stride, sample.Format!.Value, sample.AlphaFormat!.Value);
    }

    internal static IReadOnlyDictionary<ArgbColor, int> Quantize(ArgbColor[] samples, int maxColors)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (maxColors is < 1 or > 256)
            throw new ArgumentOutOfRangeException(nameof(maxColors));
        if (samples.Length == 0)
            return new Dictionary<ArgbColor, int>();

        // Celebi owns a fresh Wu instance for each call. Nothing mutable is shared by jobs.
        return new QuantizerCelebi().Quantize(new List<ArgbColor>(samples), maxColors).ColorToCount;
    }

    internal static ImageColorResult Score(IReadOnlyDictionary<ArgbColor, int> histogram, ImageColorOptions options)
    {
        ArgumentNullException.ThrowIfNull(histogram);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        if (histogram.Count == 0)
            return ImageColorResult.CreateEmpty(options.FallbackColor);

        // Score's historical default fallback must never leak through this adapter. Its
        // candidates are opaque, so transparent zero is an unambiguous internal sentinel.
        var sentinel = new ArgbColor(0u);
        var ranked = global::MaterialColorUtilities.Score.Score.CalculateScore(
            new Dictionary<ArgbColor, int>(histogram), options.DesiredColors, sentinel, options.Filter);
        return new ImageColorResult(
            ranked.Where(color => color != sentinel)
                .Select(color => new Color(color.Alpha, color.Red, color.Green, color.Blue)),
            options.FallbackColor);
    }

    internal static PixelSize GetSampleSize(PixelSize source, int maxSamplePixels)
    {
        if (source.Width <= 0 || source.Height <= 0)
            throw new ArgumentException("The bitmap must have positive pixel dimensions.", nameof(source));
        if (maxSamplePixels is < 1 or > 65_536)
            throw new ArgumentOutOfRangeException(nameof(maxSamplePixels));

        var area = checked((long)source.Width * source.Height);
        if (area <= maxSamplePixels)
            return source;

        var scale = Math.Sqrt((double)maxSamplePixels / area);
        var width = Math.Max(1, checked((int)Math.Floor(source.Width * scale)));
        var height = Math.Max(1, checked((int)Math.Floor(source.Height * scale)));

        // Clamping a sub-pixel dimension to one can exceed the budget for a very thin
        // image. Shrink the other dimension as well, without ever enlarging either axis.
        if (checked((long)width * height) > maxSamplePixels)
        {
            if (width >= height)
                width = Math.Max(1, maxSamplePixels / height);
            else
                height = Math.Max(1, maxSamplePixels / width);
        }

        return new PixelSize(width, height);
    }

    internal static ArgbColor[] DecodeSamples(
        ReadOnlySpan<byte> bytes, PixelSize size, int stride, PixelFormat format, AlphaFormat alphaFormat)
    {
        ValidateFormat(format, alphaFormat);
        if (size.Width <= 0 || size.Height <= 0 || stride < checked(size.Width * 4))
            throw new ArgumentException("The pixel dimensions and stride are invalid.", nameof(stride));
        if (bytes.Length < checked(stride * size.Height))
            throw new ArgumentException("The pixel buffer is shorter than its dimensions and stride.", nameof(bytes));

        var colors = new ArgbColor[checked(size.Width * size.Height)];
        var count = 0;
        var isBgra = format == PixelFormats.Bgra8888;
        for (var y = 0; y < size.Height; y++)
        {
            var row = bytes.Slice(checked(y * stride), checked(size.Width * 4));
            for (var x = 0; x < row.Length; x += 4)
            {
                // Opaque metadata means the stored alpha byte is unused. For Premul and
                // Unpremul the alpha byte is authoritative. No unpremultiplication is
                // needed after rejecting alpha < 255: at alpha 255 the RGB is unchanged.
                var alpha = alphaFormat == AlphaFormat.Opaque ? (byte)255 : row[x + 3];
                if (alpha != 255)
                    continue;
                colors[count++] = new ArgbColor(255,
                    row[x + (isBgra ? 2 : 0)], row[x + 1], row[x + (isBgra ? 0 : 2)]);
            }
        }

        if (count != colors.Length)
            Array.Resize(ref colors, count);
        return colors;
    }

    private static void ValidateFormat(PixelFormat? format, AlphaFormat? alphaFormat)
    {
        if (format != PixelFormats.Bgra8888 && format != PixelFormats.Rgba8888)
            throw new NotSupportedException("Image extraction supports readable BGRA8888 and RGBA8888 bitmaps only.");
        if (alphaFormat is not (AlphaFormat.Opaque or AlphaFormat.Premul or AlphaFormat.Unpremul))
            throw new NotSupportedException("The bitmap does not provide a supported alpha format.");
    }
}
