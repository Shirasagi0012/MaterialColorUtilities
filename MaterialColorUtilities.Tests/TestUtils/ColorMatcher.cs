using MaterialColorUtilities.HCT;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Tests.TestUtils;

/// <summary>
/// Helper methods for asserting color equality in tests.
/// </summary>
internal static class ColorMatcher
{
    /// <summary>
    /// Checks if two ARGB colors are exactly equal.
    /// </summary>
    internal static bool IsColor(ArgbColor actual, ArgbColor expected)
    {
        return actual.Value == expected.Value;
    }

    /// <summary>
    /// Checks if two ARGB colors are close to each other (using CAM16 distance).
    /// Colors are considered close if their CAM16 distance is less than or equal to 5.
    /// </summary>
    internal static bool IsCloseToColor(ArgbColor actual, ArgbColor expected)
    {
        var actualCam = Cam16.FromArgb(actual);
        var expectedCam = Cam16.FromArgb(expected);
        return actualCam.Distance(expectedCam) <= 5.0;
    }

    /// <summary>
    /// Formats an ARGB color as a hex string for error messages.
    /// </summary>
    internal static string HexFromArgb(ArgbColor argb)
    {
        return $"0x{argb.Value:X8}";
    }
}
