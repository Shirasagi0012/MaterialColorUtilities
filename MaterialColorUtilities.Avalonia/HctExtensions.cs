using Avalonia.Media;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Avalonia;

public static class HctExtensions
{
    extension (Hct @this)
    {
        public Color ToAvaloniaColor()
        {
            var argb = @this.Argb;
            return Color.FromArgb(argb.Alpha, argb.Red, argb.Green, argb.Blue);
        }

        public static Hct FromAvaloniaColor(Color color)
        {
            var argb = new Utils.ArgbColor(color.A, color.R, color.G, color.B);
            return Hct.From(argb);
        }
    }
}
