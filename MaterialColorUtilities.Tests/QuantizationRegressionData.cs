using System.Security.Cryptography;
using System.Text;
using MaterialColorUtilities.Quantize;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Tests.Quantize;

internal static class QuantizationRegressionData
{
    public static IEnumerable<(string Name, List<ArgbColor> Pixels, int MaxColors)> Cases()
    {
        yield return ("empty", [], 16);
        yield return ("one", [new(0xff123456)], 16);
        yield return ("repeated", Enumerable.Repeat(new ArgbColor(0xff123456), 64).ToList(), 16);
        yield return ("transparent", [new(0x00123456), new(0xfe123456), new(0x0000ff00)], 16);
        yield return ("duplicate-rgb-alpha", [new(0xffff0000), new(0x00ff0000)], 256);
        yield return ("mixed", [new(0xffff0000), new(0x00ff0000), new(0xffff0000), new(0xfe00ff00), new(0xff00ff00), new(0xff0000ff)], 2);
        foreach (var count in new[] { 1, 2, 16, 256 })
            yield return ($"noise-{count}", Noise(32 * 32), count);
        yield return ("gradient", Gradient(64), 16);
        yield return ("palette", Palette(4096), 16);
    }

    public static List<ArgbColor> Noise(int count)
    {
        // Explicit PRNG keeps input stable across runtime versions.
        uint state = 0x42688;
        return Enumerable.Range(0, count).Select(_ =>
        {
            state = unchecked(state * 1664525u + 1013904223u);
            return new ArgbColor(0xff000000u | (state & 0xffffff));
        }).ToList();
    }

    public static List<ArgbColor> Gradient(int side) => Enumerable.Range(0, side * side)
        .Select(i => new ArgbColor(0xff000000u | (uint)((i % side * 255 / (side - 1)) << 16)
            | (uint)((i / side * 255 / (side - 1)) << 8) | (uint)((i % side + i / side) * 255 / (2 * (side - 1)))))
        .ToList();

    public static List<ArgbColor> Palette(int count)
    {
        uint[] palette = [0xff001122, 0xffaabbcc, 0xffcc3322, 0xff336633, 0xff336699];
        return Enumerable.Range(0, count).Select(i => new ArgbColor(palette[i % palette.Length])).ToList();
    }

    public static string Fingerprint(QuantizerResult result)
    {
        // Intentionally retain dictionary iteration order, including the optional mapping.
        var value = string.Join(";", result.ColorToCount.Select(p => $"{p.Key.Value:X8}:{p.Value}"))
            + "|" + string.Join(";", result.InputPixelToClusterPixel.Select(p => $"{p.Key.Value:X8}:{p.Value.Value:X8}"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
