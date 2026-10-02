using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Avalonia;
using MaterialColorUtilities.Avalonia.Helpers;
using MaterialColorUtilities.Avalonia.Tokens;
using MaterialColorUtilities.DynamicColors;
using Xunit;

namespace MaterialColorUtilities.Tests.Avalonia.TestUtils;

internal static class MaterialColorTestHelper
{
    internal static Color Primary(ColorScheme scheme, ThemeVariant theme) =>
        new MaterialDynamicColors().Primary.GetArgb(scheme.CreateScheme(theme)).ToAvaloniaColor();

    internal static Color PrimaryContainer(ColorScheme scheme, ThemeVariant theme) =>
        new MaterialDynamicColors().PrimaryContainer.GetArgb(scheme.CreateScheme(theme)).ToAvaloniaColor();

    internal static Color ReadColor(IResourceHost host, object key, ThemeVariant? theme = null)
    {
        Assert.True(host.TryFindResource(key, theme, out var value));
        return Assert.IsType<Color>(value);
    }

    internal static Color BrushColor(IBrush? brush) => Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;
}
