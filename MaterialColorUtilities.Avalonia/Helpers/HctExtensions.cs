using Avalonia.Media;
using MaterialColorUtilities.HCT;

namespace MaterialColorUtilities.Avalonia;

public static class HctExtensions
{
    extension(Hct @this)
    {
        public Color ToAvaloniaColor()
        {
            var argb = @this.Argb;

            return Color.FromArgb(a: argb.Alpha, r: argb.Red, g: argb.Green, b: argb.Blue);
        }

        public static Hct FromAvaloniaColor(Color color)
        {
            var argb = new Utils.ArgbColor(alpha: color.A, red: color.R, green: color.G, blue: color.B);

            return Hct.From(argb);
        }
    }
}
