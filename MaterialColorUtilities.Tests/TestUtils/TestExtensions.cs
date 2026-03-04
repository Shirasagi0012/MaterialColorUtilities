using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Tests.TestUtils;

/// <summary>
/// Extension methods for tests.
/// </summary>
internal static class TestExtensions
{
    /// <summary>
    /// Asserts that a value is close to expected with specified tolerance.
    /// </summary>
    internal static void AssertCloseTo(this double actual, double expected, double tolerance)
    {
        if (Math.Abs(actual - expected) > tolerance)
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected {expected} ± {tolerance}, but got {actual}");
        }
    }

    /// <summary>
    /// Asserts that a value is close to expected with specified tolerance.
    /// </summary>
    internal static void AssertCloseTo(this double? actual, double? expected, double tolerance)
    {
        if (actual == null && expected == null)
            return;

        if (actual == null)
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected {expected} ± {tolerance}, but got null");
        }

        if (Math.Abs(actual.Value - expected.Value) > tolerance)
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected {expected} ± {tolerance}, but got {actual.Value}");
        }
    }

    /// <summary>
    /// Asserts that two colors are equal.
    /// </summary>
    internal static void AssertColorEquals(this ArgbColor actual, ArgbColor expected)
    {
        if (!ColorMatcher.IsColor(actual, expected))
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected color {ColorMatcher.HexFromArgb(expected)}, " +
                $"but got {ColorMatcher.HexFromArgb(actual)}");
        }
    }

    /// <summary>
    /// Asserts that two colors are close to each other.
    /// </summary>
    internal static void AssertColorCloseTo(this ArgbColor actual, ArgbColor expected)
    {
        if (!ColorMatcher.IsCloseToColor(actual, expected))
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected color close to {ColorMatcher.HexFromArgb(expected)}, " +
                $"but got {ColorMatcher.HexFromArgb(actual)}");
        }
    }
}
