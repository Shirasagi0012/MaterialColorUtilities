using MaterialColorUtilities.Utils;
using Xunit;

namespace MaterialColorUtilities.Tests.Utils;

public class MathUtilsTests
{
    /// <summary>
    /// Original implementation for MathUtils.RotationDirection.
    /// Included here to test equivalence with new implementation.
    /// </summary>
    private static double OriginalRotationDirection(double from, double to)
    {
        var a = to - from;
        var b = to - from + 360.0;
        var c = to - from - 360.0;
        var aAbs = Math.Abs(a);
        var bAbs = Math.Abs(b);
        var cAbs = Math.Abs(c);
        if (aAbs <= bAbs && aAbs <= cAbs)
        {
            return a >= 0.0 ? 1.0 : -1.0;
        }
        else if (bAbs <= aAbs && bAbs <= cAbs)
        {
            return b >= 0.0 ? 1.0 : -1.0;
        }
        else
        {
            return c >= 0.0 ? 1.0 : -1.0;
        }
    }

    [Fact]
    public void RotationDirection_BehavesCorrectly()
    {
        for (var from = 0.0; from < 360.0; from += 15.0)
        {
            for (var to = 7.5; to < 360.0; to += 15.0)
            {
                var expectedAnswer = OriginalRotationDirection(from, to);
                var actualAnswer = MathUtils.RotationDirection(from, to);
                
                Assert.Equal(expectedAnswer, actualAnswer);
                Assert.Equal(1.0, Math.Abs(actualAnswer));
            }
        }
    }
}
