using MaterialColorUtilities.DynamicColor;
using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.DynamicColor;

public class DynamicSchemeTests
{
    [Fact]
    public void ZeroLengthInput()
    {
        var hue = DynamicScheme.GetRotatedHue(
            Hct.From(43, 16, 16),
            [],
            []);
        hue.AssertCloseTo(43, 1.0);
    }

    [Fact]
    public void OneLengthInputNoRotation()
    {
        var hue = DynamicScheme.GetRotatedHue(
            Hct.From(43, 16, 16),
            [0],
            [0]);
        hue.AssertCloseTo(43, 1.0);
    }

    [Fact]
    public void InputLengthMismatchThrows()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            DynamicScheme.GetRotatedHue(
                Hct.From(43, 16, 16),
                [0, 1],
                [0]);
        });
    }

    [Fact]
    public void OnBoundaryRotationCorrect()
    {
        var hue = DynamicScheme.GetRotatedHue(
            Hct.From(43, 16, 16),
            [0, 42, 360],
            [0, 15, 0]);
        hue.AssertCloseTo(43 + 15, 1.0);
    }

    [Fact]
    public void RotationResultLargerThan360DegreesWraps()
    {
        var hue = DynamicScheme.GetRotatedHue(
            Hct.From(43, 16, 16),
            [0, 42, 360],
            [0, 480, 0]);
        hue.AssertCloseTo(163, 1.0);
    }
}
