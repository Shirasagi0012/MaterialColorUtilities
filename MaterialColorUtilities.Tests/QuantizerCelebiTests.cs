using MaterialColorUtilities.Quantize;
using MaterialColorUtilities.Utils;
using Xunit;

namespace MaterialColorUtilities.Tests.Quantize;

public class QuantizerCelebiTests
{
    private static readonly ArgbColor Red = new(0xffff0000);
    private static readonly ArgbColor Green = new(0xff00ff00);
    private static readonly ArgbColor Blue = new(0xff0000ff);
    private const int MaxColors = 256;

    [Fact]
    public async Task OneRed()
    {
        var celebi = new QuantizerCelebi();
        var result = await celebi.QuantizeAsync(new List<ArgbColor> { Red }, MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(Red.Value, colors[0].Value);
    }

    [Fact]
    public async Task OneGreen()
    {
        var celebi = new QuantizerCelebi();
        var result = await celebi.QuantizeAsync(new List<ArgbColor> { Green }, MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(Green.Value, colors[0].Value);
    }

    [Fact]
    public async Task OneBlue()
    {
        var celebi = new QuantizerCelebi();
        var result = await celebi.QuantizeAsync(new List<ArgbColor> { Blue }, MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(Blue.Value, colors[0].Value);
    }

    [Fact]
    public async Task FiveBlue()
    {
        var celebi = new QuantizerCelebi();
        var result = await celebi.QuantizeAsync(
            new List<ArgbColor> { Blue, Blue, Blue, Blue, Blue },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();
        Assert.Single(colors);
        Assert.Equal(Blue.Value, colors[0].Value);
    }

    [Fact]
    public async Task OneRedOneGreenOneBlue()
    {
        var celebi = new QuantizerCelebi();
        var result = await celebi.QuantizeAsync(
            new List<ArgbColor> { Red, Green, Blue },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();

        Assert.Equal(3, colors.Distinct().Count());
        Assert.Equal(Blue.Value, colors[0].Value);
        Assert.Equal(Red.Value, colors[1].Value);
        Assert.Equal(Green.Value, colors[2].Value);
    }

    [Fact]
    public async Task TwoRedThreeGreen()
    {
        var celebi = new QuantizerCelebi();
        var result = await celebi.QuantizeAsync(
            new List<ArgbColor> { Red, Red, Green, Green, Green },
            MaxColors);
        var colors = result.ColorToCount.Keys.ToList();

        Assert.Equal(2, colors.Distinct().Count());
        Assert.Equal(Green.Value, colors[0].Value);
        Assert.Equal(Red.Value, colors[1].Value);
    }
}
