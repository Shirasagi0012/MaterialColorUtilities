using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Palettes;

public class PalettesTests
{
    [Fact]
    public void TonalPalette_OperatorEquals_FromConstructors()
    {
        var a1 = new TonalPalette(1, 1);
        var a2 = new TonalPalette(1, 1);
        var b1 = TonalPalette.FromList(
            TonalPalette.CommonTones.Select(e => new ArgbColor(0xDEADBEEF)).ToList());
        var b2 = TonalPalette.FromList(
            TonalPalette.CommonTones.Select(e => new ArgbColor(0xDEADBEEF)).ToList());

        Assert.False(a1 == b1);
        Assert.False(b1 == a1);
        Assert.True(a1 != b1);
        Assert.True(b1 != a1);
        Assert.True(a1 == a2);
        Assert.True(b1 == b2);

        var c1 = TonalPalette.FromList(
            TonalPalette.CommonTones.Select(e => new ArgbColor(123)).ToList());
        var c2 = TonalPalette.FromList(
            TonalPalette.CommonTones.Select(e => e < 15 ? new ArgbColor(456) : new ArgbColor(123)).ToList());

        Assert.Equal(c1.Get(50).Value, c2.Get(50).Value);
        Assert.False(c1 == c2);
    }

    [Fact]
    public void TonalPalette_TonesOfBlue()
    {
        var hct = Hct.From(new ArgbColor(0xff0000ff));
        var tones = new TonalPalette(hct.Hue, hct.Chroma);

        Assert.Equal(unchecked((int)0xff000000), tones.Get(0).Value);
        Assert.Equal(unchecked((int)0xff00006e), tones.Get(10).Value);
        Assert.Equal(unchecked((int)0xff0001ac), tones.Get(20).Value);
        Assert.Equal(unchecked((int)0xff0000ef), tones.Get(30).Value);
        Assert.Equal(unchecked((int)0xff343dff), tones.Get(40).Value);
        Assert.Equal(unchecked((int)0xff5a64ff), tones.Get(50).Value);
        Assert.Equal(unchecked((int)0xff7c84ff), tones.Get(60).Value);
        Assert.Equal(unchecked((int)0xff9da3ff), tones.Get(70).Value);
        Assert.Equal(unchecked((int)0xffbec2ff), tones.Get(80).Value);
        Assert.Equal(unchecked((int)0xffe0e0ff), tones.Get(90).Value);
        Assert.Equal(unchecked((int)0xfff1efff), tones.Get(95).Value);
        Assert.Equal(unchecked((int)0xfffffbff), tones.Get(99).Value);
        Assert.Equal(unchecked((int)0xffffffff), tones.Get(100).Value);

        // Tone not in CommonTones
        Assert.Equal(unchecked((int)0xff00003c), tones.Get(3).Value);
    }

    [Fact]
    public void TonalPalette_AsList()
    {
        var hct = Hct.From(new ArgbColor(0xff0000ff));
        var tones = new TonalPalette(hct.Hue, hct.Chroma);

        var expected = new[]
        {
            unchecked((int)0xff000000),
            unchecked((int)0xff00006e),
            unchecked((int)0xff0001ac),
            unchecked((int)0xff0000ef),
            unchecked((int)0xff343dff),
            unchecked((int)0xff5a64ff),
            unchecked((int)0xff7c84ff),
            unchecked((int)0xff9da3ff),
            unchecked((int)0xffbec2ff),
            unchecked((int)0xffe0e0ff),
            unchecked((int)0xfff1efff),
            unchecked((int)0xfffffbff),
            unchecked((int)0xffffffff),
        };

        var actual = tones.AsList().Select(c => c.Value).ToArray();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TonalPalette_OperatorEqualsAndHashCode()
    {
        var hctAB = Hct.From(new ArgbColor(0xff0000ff));
        var tonesA = new TonalPalette(hctAB.Hue, hctAB.Chroma);
        var tonesB = new TonalPalette(hctAB.Hue, hctAB.Chroma);
        var hctC = Hct.From(new ArgbColor(0xff123456));
        var tonesC = new TonalPalette(hctC.Hue, hctC.Chroma);

        Assert.Equal(tonesA, tonesB);
        Assert.NotEqual(tonesA, tonesC);
        Assert.NotEqual(tonesB, tonesC);
        Assert.Equal(tonesA.GetHashCode(), tonesB.GetHashCode());
    }

    [Fact]
    public void KeyColor_ExactChromaIsAvailable()
    {
        var palette = new TonalPalette(50, 60);
        var result = palette.KeyColor;
        
        result.Hue.AssertCloseTo(50, 10);
        result.Chroma.AssertCloseTo(60, 0.5);
        Assert.InRange(result.Tone, 0,100);
    }

    [Fact]
    public void KeyColor_RequestingUnusuallyHighChroma()
    {
        // Requested chroma is above what is achievable. For Hue 149, chroma peak is ~89.6.
        // The result key color's chroma should be close to the chroma peak (i.e., > 89.0).
        var palette = new TonalPalette(149, 200);
        var result = palette.KeyColor;

        result.Hue.AssertCloseTo(149, 10);
        Assert.True(result.Chroma > 89.0, $"Expected chroma > 89.0 but was {result.Chroma}");
        Assert.InRange(result.Tone, 0, 100);
    }

    [Fact]
    public void KeyColor_RequestingUnusuallyLowChroma()
    {
        // When requesting a very low chroma, the key color should be near tone 50 and match the
        // given low chroma.
        var palette = new TonalPalette(50, 3);
        var result = palette.KeyColor;

        result.Hue.AssertCloseTo(50, 10);
        result.Chroma.AssertCloseTo(3, 0.5);
        result.Tone.AssertCloseTo(50, 0.5);
    }
}
