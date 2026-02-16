using Avalonia.Media;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Avalonia;

public static class HctExtensions
{
    extension(Hct @this)
    {
        public Color ToAvaloniaColor() => @this.Argb.ToAvaloniaColor();

        public static Hct FromAvaloniaColor(Color color) => Hct.From(ArgbExtensions.FromAvaloniaColor(color));
    }
}
