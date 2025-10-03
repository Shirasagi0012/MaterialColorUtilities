using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Utils;

public class ColorUtilsTests
{
    private static List<double> Range(double start, double stop, int caseCount)
    {
        var stepSize = (stop - start) / (caseCount - 1);
        return Enumerable.Range(0, caseCount)
            .Select(index => start + stepSize * index)
            .ToList();
    }

    private static List<int> RgbRange =>
        Range(0.0, 255.0, 8).Select(e => (int)Math.Round(e, MidpointRounding.AwayFromZero)).ToList();

    private static List<int> FullRgbRange =>
        Enumerable.Range(0, 256).ToList();

    [Fact]
    public void Range_Integrity()
    {
        var range = Range(3.0, 9999.0, 1234);
        for (var i = 0; i < 1234; i++)
        {
            range[i].AssertCloseTo(3 + 8.1070559611 * i, 1e-5);
        }
    }

    [Fact]
    public void ArgbFromRgb_ReturnsCorrectValueForBlack()
    {
        var argb = ColorUtils.ArgbFromRgb(0, 0, 0);
        Assert.Equal(0xff000000u, (uint)argb.Value);
        Assert.Equal(4278190080u, (uint)argb.Value);
    }

    [Fact]
    public void ArgbFromRgb_ReturnsCorrectValueForWhite()
    {
        var argb = ColorUtils.ArgbFromRgb(255, 255, 255);
        Assert.Equal(0xffffffffu, (uint)argb.Value);
        Assert.Equal(4294967295u, (uint)argb.Value);
    }

    [Fact]
    public void ArgbFromRgb_ReturnsCorrectValueForRandomColor()
    {
        var argb = ColorUtils.ArgbFromRgb(50, 150, 250);
        Assert.Equal(0xff3296fau, (uint)argb.Value);
        Assert.Equal(4281505530u, (uint)argb.Value);
    }

    [Fact]
    public void YToLstarToY()
    {
        foreach (var y in Range(0, 100, 1001))
        {
            ColorUtils.YFromLstar(ColorUtils.LstarFromY(y)).AssertCloseTo(y, 1e-5);
        }
    }

    [Fact]
    public void LstarToYToLstar()
    {
        foreach (var lstar in Range(0, 100, 1001))
        {
            ColorUtils.LstarFromY(ColorUtils.YFromLstar(lstar)).AssertCloseTo(lstar, 1e-5);
        }
    }

    [Fact]
    public void YFromLstar()
    {
        ColorUtils.YFromLstar(0.0).AssertCloseTo(0.0, 1e-5);
        ColorUtils.YFromLstar(0.1).AssertCloseTo(0.0110705, 1e-5);
        ColorUtils.YFromLstar(0.2).AssertCloseTo(0.0221411, 1e-5);
        ColorUtils.YFromLstar(0.3).AssertCloseTo(0.0332116, 1e-5);
        ColorUtils.YFromLstar(0.4).AssertCloseTo(0.0442822, 1e-5);
        ColorUtils.YFromLstar(0.5).AssertCloseTo(0.0553528, 1e-5);
        ColorUtils.YFromLstar(1.0).AssertCloseTo(0.1107056, 1e-5);
        ColorUtils.YFromLstar(2.0).AssertCloseTo(0.2214112, 1e-5);
        ColorUtils.YFromLstar(3.0).AssertCloseTo(0.3321169, 1e-5);
        ColorUtils.YFromLstar(4.0).AssertCloseTo(0.4428225, 1e-5);
        ColorUtils.YFromLstar(5.0).AssertCloseTo(0.5535282, 1e-5);
        ColorUtils.YFromLstar(8.0).AssertCloseTo(0.8856451, 1e-5);
        ColorUtils.YFromLstar(10.0).AssertCloseTo(1.1260199, 1e-5);
        ColorUtils.YFromLstar(15.0).AssertCloseTo(1.9085832, 1e-5);
        ColorUtils.YFromLstar(20.0).AssertCloseTo(2.9890524, 1e-5);
        ColorUtils.YFromLstar(25.0).AssertCloseTo(4.4154767, 1e-5);
        ColorUtils.YFromLstar(30.0).AssertCloseTo(6.2359055, 1e-5);
        ColorUtils.YFromLstar(40.0).AssertCloseTo(11.2509737, 1e-5);
        ColorUtils.YFromLstar(50.0).AssertCloseTo(18.4186518, 1e-5);
        ColorUtils.YFromLstar(60.0).AssertCloseTo(28.1233342, 1e-5);
        ColorUtils.YFromLstar(70.0).AssertCloseTo(40.7494157, 1e-5);
        ColorUtils.YFromLstar(80.0).AssertCloseTo(56.6812907, 1e-5);
        ColorUtils.YFromLstar(90.0).AssertCloseTo(76.3033539, 1e-5);
        ColorUtils.YFromLstar(95.0).AssertCloseTo(87.6183294, 1e-5);
        ColorUtils.YFromLstar(99.0).AssertCloseTo(97.4360239, 1e-5);
        ColorUtils.YFromLstar(100.0).AssertCloseTo(100.0, 1e-5);
    }

    [Fact]
    public void LstarFromY()
    {
        ColorUtils.LstarFromY(0.0).AssertCloseTo(0.0, 1e-5);
        ColorUtils.LstarFromY(0.1).AssertCloseTo(0.9032962, 1e-5);
        ColorUtils.LstarFromY(0.2).AssertCloseTo(1.8065925, 1e-5);
        ColorUtils.LstarFromY(0.3).AssertCloseTo(2.7098888, 1e-5);
        ColorUtils.LstarFromY(0.4).AssertCloseTo(3.6131851, 1e-5);
        ColorUtils.LstarFromY(0.5).AssertCloseTo(4.5164814, 1e-5);
        ColorUtils.LstarFromY(0.8856451).AssertCloseTo(8.0, 1e-5);
        ColorUtils.LstarFromY(1.0).AssertCloseTo(8.9914424, 1e-5);
        ColorUtils.LstarFromY(2.0).AssertCloseTo(15.4872443, 1e-5);
        ColorUtils.LstarFromY(3.0).AssertCloseTo(20.0438970, 1e-5);
        ColorUtils.LstarFromY(4.0).AssertCloseTo(23.6714419, 1e-5);
        ColorUtils.LstarFromY(5.0).AssertCloseTo(26.7347653, 1e-5);
        ColorUtils.LstarFromY(10.0).AssertCloseTo(37.8424304, 1e-5);
        ColorUtils.LstarFromY(15.0).AssertCloseTo(45.6341970, 1e-5);
        ColorUtils.LstarFromY(20.0).AssertCloseTo(51.8372115, 1e-5);
        ColorUtils.LstarFromY(25.0).AssertCloseTo(57.0754208, 1e-5);
        ColorUtils.LstarFromY(30.0).AssertCloseTo(61.6542222, 1e-5);
        ColorUtils.LstarFromY(40.0).AssertCloseTo(69.4695307, 1e-5);
        ColorUtils.LstarFromY(50.0).AssertCloseTo(76.0692610, 1e-5);
        ColorUtils.LstarFromY(60.0).AssertCloseTo(81.8381891, 1e-5);
        ColorUtils.LstarFromY(70.0).AssertCloseTo(86.9968642, 1e-5);
        ColorUtils.LstarFromY(80.0).AssertCloseTo(91.6848609, 1e-5);
        ColorUtils.LstarFromY(90.0).AssertCloseTo(95.9967686, 1e-5);
        ColorUtils.LstarFromY(95.0).AssertCloseTo(98.0335184, 1e-5);
        ColorUtils.LstarFromY(99.0).AssertCloseTo(99.6120372, 1e-5);
        ColorUtils.LstarFromY(100.0).AssertCloseTo(100.0, 1e-5);
    }

    [Fact]
    public void Y_Continuity()
    {
        var epsilon = 1e-6;
        var delta = 1e-8;
        var left = 8.0 - delta;
        var mid = 8.0;
        var right = 8.0 + delta;
        ColorUtils.YFromLstar(left).AssertCloseTo(ColorUtils.YFromLstar(mid), epsilon);
        ColorUtils.YFromLstar(right).AssertCloseTo(ColorUtils.YFromLstar(mid), epsilon);
    }

    [Fact]
    public void RgbToXyzToRgb()
    {
        foreach (var r in RgbRange)
        {
            foreach (var g in RgbRange)
            {
                foreach (var b in RgbRange)
                {
                    var argb = ColorUtils.ArgbFromRgb(r, g, b);
                    var xyz = ColorUtils.XyzFromArgb(argb);
                    var converted = ColorUtils.ArgbFromXyz(xyz);
                    ((double)converted.Red).AssertCloseTo(r, 1.5);
                    ((double)converted.Green).AssertCloseTo(g, 1.5);
                    ((double)converted.Blue).AssertCloseTo(b, 1.5);
                }
            }
        }
    }

    [Fact]
    public void RgbToLabToRgb()
    {
        foreach (var r in RgbRange)
        {
            foreach (var g in RgbRange)
            {
                foreach (var b in RgbRange)
                {
                    var argb = ColorUtils.ArgbFromRgb(r, g, b);
                    var lab = ColorUtils.LabFromArgb(argb);
                    var converted = ColorUtils.ArgbFromLab(lab);
                    ((double)converted.Red).AssertCloseTo(r, 1.5);
                    ((double)converted.Green).AssertCloseTo(g, 1.5);
                    ((double)converted.Blue).AssertCloseTo(b, 1.5);
                }
            }
        }
    }

    [Fact]
    public void RgbToLstarToRgb()
    {
        foreach (var component in FullRgbRange)
        {
            var argb = ColorUtils.ArgbFromRgb(component, component, component);
            var lstar = ColorUtils.LstarFromArgb(argb);
            var converted = ColorUtils.ArgbFromLstar(lstar);
            Assert.Equal(argb.Value, converted.Value);
        }
    }

    [Fact]
    public void RgbToLstarToYCommutes()
    {
        foreach (var r in RgbRange)
        {
            foreach (var g in RgbRange)
            {
                foreach (var b in RgbRange)
                {
                    var argb = ColorUtils.ArgbFromRgb(r, g, b);
                    var lstar = ColorUtils.LstarFromArgb(argb);
                    var y = ColorUtils.YFromLstar(lstar);
                    var y2 = ColorUtils.XyzFromArgb(argb).Y;
                    y.AssertCloseTo(y2, 1e-5);
                }
            }
        }
    }

    [Fact]
    public void LstarToRgbToYCommutes()
    {
        foreach (var lstar in Range(0, 100, 1001))
        {
            var argb = ColorUtils.ArgbFromLstar(lstar);
            var y = ColorUtils.XyzFromArgb(argb).Y;
            var y2 = ColorUtils.YFromLstar(lstar);
            y.AssertCloseTo(y2, 1);
        }
    }

    [Fact]
    public void LinearizeDelinearize()
    {
        foreach (var rgbComponent in FullRgbRange)
        {
            var converted = ColorUtils.Delinearized(ColorUtils.Linearized(new ArgbColor(new Vector3D(rgbComponent,rgbComponent,rgbComponent))));
            converted.AssertColorEquals(new ArgbColor(new Vector3D(rgbComponent, rgbComponent, rgbComponent)));
        }
    }
}
