using Avalonia.Media;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Avalonia.Helpers;

public static class HctExtensions
{
    extension(Hct @this)
    {
        public Color ToAvaloniaColor()
        {
            return @this.Argb.ToAvaloniaColor();
        }

        public static Hct FromAvaloniaColor(Color color)
        {
            return Hct.From(ArgbExtensions.FromAvaloniaColor(color));
        }
    }
}
