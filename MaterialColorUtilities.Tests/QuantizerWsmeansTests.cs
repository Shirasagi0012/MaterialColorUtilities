using MaterialColorUtilities.Quantize;
using MaterialColorUtilities.Utils;
using Xunit;

namespace MaterialColorUtilities.Tests.Quantize;

public class QuantizerWsmeansTests
{
    private static readonly ArgbColor Red = new(0xffff0000);
    private static readonly ArgbColor Green = new(0xff00ff00);
    private static readonly ArgbColor Blue = new(0xff0000ff);
    private const int MaxColors = 256;

    [Fact]
    public void OneRando()
    {
        var result = QuantizerWsmeans.Quantize(
            new List<ArgbColor> { new ArgbColor(0xff141216) },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(unchecked((int)0xff141216), colors[0].Value);
    }

    [Fact]
    public void OneRed_FirstTest()
    {
        var result = QuantizerWsmeans.Quantize(
            new List<ArgbColor> { Red },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
    }

    [Fact]
    public void OneRed()
    {
        var result = QuantizerWsmeans.Quantize(
            new List<ArgbColor> { Red },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(Red.Value, colors[0].Value);
    }

    [Fact]
    public void OneGreen()
    {
        var result = QuantizerWsmeans.Quantize(
            new List<ArgbColor> { Green },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(Green.Value, colors[0].Value);
    }

    [Fact]
    public void OneBlue()
    {
        var result = QuantizerWsmeans.Quantize(
            new List<ArgbColor> { Blue },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(Blue.Value, colors[0].Value);
    }

    [Fact]
    public void FiveBlue()
    {
        var result = QuantizerWsmeans.Quantize(
            new List<ArgbColor> { Blue, Blue, Blue, Blue, Blue },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(Blue.Value, colors[0].Value);
    }
}
