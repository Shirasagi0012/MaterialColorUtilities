using MaterialColorUtilities.Dislike;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Dislike;

public class DislikeAnalyzerTests
{
    [Fact]
    public void MonkSkinToneScaleColorsLiked()
    {
        // From https://skintone.google#/get-started
        var monkSkinToneScaleColors = new uint[]
        {
            0xfff6ede4,
            0xfff3e7db,
            0xfff7ead0,
            0xffeadaba,
            0xffd7bd96,
            0xffa07e56,
            0xff825c43,
            0xff604134,
            0xff3a312a,
            0xff292420
        };

        foreach (var color in monkSkinToneScaleColors)
        {
            var hct = Hct.From(new ArgbColor(color));
            Assert.False(DislikeAnalyzer.IsDisliked(hct));
        }
    }

    [Fact]
    public void BileColorsDisliked()
    {
        var unlikable = new uint[]
        {
            0xff95884B,
            0xff716B40,
            0xffB08E00,
            0xff4C4308,
            0xff464521
        };

        foreach (var color in unlikable)
        {
            var hct = Hct.From(new ArgbColor(color));
            Assert.True(DislikeAnalyzer.IsDisliked(hct), $"Color {color:X8} should be disliked");
        }
    }

    [Fact]
    public void BileColorsBecameLikable()
    {
        var unlikable = new uint[]
        {
            0xff95884B,
            0xff716B40,
            0xffB08E00,
            0xff4C4308,
            0xff464521
        };

        foreach (var color in unlikable)
        {
            var hct = Hct.From(new ArgbColor(color));
            Assert.True(DislikeAnalyzer.IsDisliked(hct));
            
            var likable = DislikeAnalyzer.FixIfDisliked(hct);
            Assert.False(DislikeAnalyzer.IsDisliked(likable));
        }
    }

    [Fact]
    public void Tone67NotDisliked()
    {
        var color = Hct.From(100.0, 50.0, 67.0);
        Assert.False(DislikeAnalyzer.IsDisliked(color));
        Assert.Equal(color.Argb.Value, DislikeAnalyzer.FixIfDisliked(color).Argb.Value);
    }
}
