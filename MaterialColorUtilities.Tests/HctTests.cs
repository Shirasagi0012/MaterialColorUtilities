using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.HCT;

public class HctTests
{
    private static readonly ArgbColor Black = new(0xff000000);
    private static readonly ArgbColor White = new(0xffffffff);
    private static readonly ArgbColor Red = new(0xffff0000);
    private static readonly ArgbColor Green = new(0xff00ff00);
    private static readonly ArgbColor Blue = new(0xff0000ff);
    private static readonly ArgbColor Midgray = new(0xff777777);

    private static bool ColorIsOnBoundary(ArgbColor argb)
    {
        return argb.Red == 0 ||
               argb.Red == 255 ||
               argb.Green == 0 ||
               argb.Green == 255 ||
               argb.Blue == 0 ||
               argb.Blue == 255;
    }

    [Fact]
    public void EqualsAndHashCodeBasics()
    {
        var hct1 = Hct.From(new ArgbColor(123));
        var hct2 = Hct.From(new ArgbColor(123));
        Assert.Equal(hct1, hct2);
        Assert.Equal(hct1.GetHashCode(), hct2.GetHashCode());
    }

    [Fact]
    public void ConversionsAreReflexive()
    {
        var cam = Cam16.FromArgb(Red);
        var color = cam.ViewedInConditions(ViewingConditions.Standard);
        color.AssertColorEquals(Red);
    }

    [Fact]
    public void Y_Midgray()
    {
        ColorUtils.YFromLstar(50.0).AssertCloseTo(18.418, 0.001);
    }

    [Fact]
    public void Y_Black()
    {
        ColorUtils.YFromLstar(0.0).AssertCloseTo(0.0, 0.001);
    }

    [Fact]
    public void Y_White()
    {
        ColorUtils.YFromLstar(100.0).AssertCloseTo(100.0, 0.001);
    }

    [Fact]
    public void Cam_Red()
    {
        var cam = Cam16.FromArgb(Red);
        cam.J.AssertCloseTo(46.445, 0.001);
        cam.Chroma.AssertCloseTo(113.357, 0.001);
        cam.Hue.AssertCloseTo(27.408, 0.001);
        cam.M.AssertCloseTo(89.494, 0.001);
        cam.S.AssertCloseTo(91.889, 0.001);
        cam.Q.AssertCloseTo(105.988, 0.001);
    }

    [Fact]
    public void Cam_Green()
    {
        var cam = Cam16.FromArgb(Green);
        cam.J.AssertCloseTo(79.331, 0.001);
        cam.Chroma.AssertCloseTo(108.410, 0.001);
        cam.Hue.AssertCloseTo(142.139, 0.001);
        cam.M.AssertCloseTo(85.587, 0.001);
        cam.S.AssertCloseTo(78.604, 0.001);
        cam.Q.AssertCloseTo(138.520, 0.001);
    }

    [Fact]
    public void Cam_Blue()
    {
        var cam = Cam16.FromArgb(Blue);
        cam.J.AssertCloseTo(25.465, 0.001);
        cam.Chroma.AssertCloseTo(87.230, 0.001);
        cam.Hue.AssertCloseTo(282.788, 0.001);
        cam.M.AssertCloseTo(68.867, 0.001);
        cam.S.AssertCloseTo(93.674, 0.001);
        cam.Q.AssertCloseTo(78.481, 0.001);
    }

    [Fact]
    public void Cam_Black()
    {
        var cam = Cam16.FromArgb(Black);
        cam.J.AssertCloseTo(0.0, 0.001);
        cam.Chroma.AssertCloseTo(0.0, 0.001);
        cam.Hue.AssertCloseTo(0.0, 0.001);
        cam.M.AssertCloseTo(0.0, 0.001);
        cam.S.AssertCloseTo(0.0, 0.001);
        cam.Q.AssertCloseTo(0.0, 0.001);
    }

    [Fact]
    public void Cam_White()
    {
        var cam = Cam16.FromArgb(White);
        cam.J.AssertCloseTo(100.0, 0.001);
        cam.Chroma.AssertCloseTo(2.869, 0.001);
        cam.Hue.AssertCloseTo(209.492, 0.001);
        cam.M.AssertCloseTo(2.265, 0.001);
        cam.S.AssertCloseTo(12.068, 0.001);
        cam.Q.AssertCloseTo(155.521, 0.001);
    }

    [Fact]
    public void GamutMap_Red()
    {
        var colorToTest = Red;
        var cam = Cam16.FromArgb(colorToTest);
        var color = Hct.From(cam.Hue, cam.Chroma, ColorUtils.LstarFromArgb(colorToTest));
        color.Argb.AssertColorEquals(colorToTest);
    }

    [Fact]
    public void GamutMap_Green()
    {
        var colorToTest = Green;
        var cam = Cam16.FromArgb(colorToTest);
        var color = Hct.From(cam.Hue, cam.Chroma, ColorUtils.LstarFromArgb(colorToTest));
        color.Argb.AssertColorEquals(colorToTest);
    }

    [Fact]
    public void GamutMap_Blue()
    {
        var colorToTest = Blue;
        var cam = Cam16.FromArgb(colorToTest);
        var color = Hct.From(cam.Hue, cam.Chroma, ColorUtils.LstarFromArgb(colorToTest));
        color.Argb.AssertColorEquals(colorToTest);
    }

    [Fact]
    public void GamutMap_White()
    {
        var colorToTest = White;
        var cam = Cam16.FromArgb(colorToTest);
        var color = Hct.From(cam.Hue, cam.Chroma, ColorUtils.LstarFromArgb(colorToTest));
        color.Argb.AssertColorEquals(colorToTest);
    }

    [Fact]
    public void GamutMap_Midgray()
    {
        var colorToTest = Green;
        var cam = Cam16.FromArgb(colorToTest);
        var color = Hct.From(cam.Hue, cam.Chroma, ColorUtils.LstarFromArgb(colorToTest));
        color.Argb.AssertColorEquals(colorToTest);
    }

    [Fact]
    public void HctReturnsSufficientlyCloseColor()
    {
        for (var hue = 15; hue < 360; hue += 30)
        {
            for (var chroma = 0; chroma <= 100; chroma += 10)
            {
                for (var tone = 20; tone <= 80; tone += 10)
                {
                    var hctColor = Hct.From(hue, chroma, tone);

                    if (chroma > 0)
                    {
                        Assert.True(
                            Math.Abs(hctColor.Hue - hue) < 4.0,
                            $"Hue should be close for H{hue} C{chroma} T{tone}");
                    }

                    Assert.True(
                        hctColor.Chroma >= 0.0 && hctColor.Chroma <= chroma + 2.5,
                        $"Chroma should be close or less for H{hue} C{chroma} T{tone}");

                    if (hctColor.Chroma < chroma - 2.5)
                    {
                        Assert.True(
                            ColorIsOnBoundary(hctColor.Argb),
                            $"HCT request for non-sRGB color should return a color on the boundary " +
                            $"of the sRGB cube for H{hue} C{chroma} T{tone}, but got " +
                            $"{ColorMatcher.HexFromArgb(hctColor.Argb)} instead");
                    }

                    Assert.True(
                        Math.Abs(hctColor.Tone - tone) < 0.5,
                        $"Tone should be close for H{hue} C{chroma} T{tone}");
                }
            }
        }
    }

    [Fact]
    public void Cam16ToXyz()
    {
        var colorToTest = Red;
        var cam = Cam16.FromArgb(colorToTest);
        var xyz = cam.XyzInViewingConditions(ViewingConditions.SRgb);
        xyz.X.AssertCloseTo(41.23, 0.01);
        xyz.Y.AssertCloseTo(21.26, 0.01);
        xyz.Z.AssertCloseTo(1.93, 0.01);
    }

    [Fact]
    public void ColorRelativity_RedInBlack()
    {
        var colorToTest = Red;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 0.0))
            .Argb.AssertColorEquals(new ArgbColor(0xff9F5C51));
    }

    [Fact]
    public void ColorRelativity_RedInWhite()
    {
        var colorToTest = Red;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 100.0))
            .Argb.AssertColorEquals(new ArgbColor(0xffFF5D48));
    }

    [Fact]
    public void ColorRelativity_GreenInBlack()
    {
        var colorToTest = Green;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 0.0))
            .Argb.AssertColorEquals(new ArgbColor(0xffACD69D));
    }

    [Fact]
    public void ColorRelativity_GreenInWhite()
    {
        var colorToTest = Green;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 100.0))
            .Argb.AssertColorEquals(new ArgbColor(0xff8EFF77));
    }

    [Fact]
    public void ColorRelativity_BlueInBlack()
    {
        var colorToTest = Blue;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 0.0))
            .Argb.AssertColorEquals(new ArgbColor(0xff343654));
    }

    [Fact]
    public void ColorRelativity_BlueInWhite()
    {
        var colorToTest = Blue;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 100.0))
            .Argb.AssertColorEquals(new ArgbColor(0xff3F49FF));
    }

    [Fact]
    public void ColorRelativity_WhiteInBlack()
    {
        var colorToTest = White;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 0.0))
            .Argb.AssertColorEquals(new ArgbColor(0xffFFFFFF));
    }

    [Fact]
    public void ColorRelativity_WhiteInWhite()
    {
        var colorToTest = White;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 100.0))
            .Argb.AssertColorEquals(new ArgbColor(0xffFFFFFF));
    }

    [Fact]
    public void ColorRelativity_MidgrayInBlack()
    {
        var colorToTest = Midgray;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 0.0))
            .Argb.AssertColorEquals(new ArgbColor(0xff605F5F));
    }

    [Fact]
    public void ColorRelativity_MidgrayInWhite()
    {
        var colorToTest = Midgray;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 100.0))
            .Argb.AssertColorEquals(new ArgbColor(0xff8E8E8E));
    }

    [Fact]
    public void ColorRelativity_BlackInBlack()
    {
        var colorToTest = Black;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 0.0))
            .Argb.AssertColorEquals(new ArgbColor(0xff000000));
    }

    [Fact]
    public void ColorRelativity_BlackInWhite()
    {
        var colorToTest = Black;
        var hct = Hct.From(colorToTest);
        hct.InViewingConditions(ViewingConditions.Make(backgroundLstar: 100.0))
            .Argb.AssertColorEquals(new ArgbColor(0xff000000));
    }
}
