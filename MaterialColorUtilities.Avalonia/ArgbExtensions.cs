namespace MaterialColorUtilities.Avalonia;

using global::Avalonia.Media;
using Utils;

public static class ArgbExtensions
{
    extension(ArgbColor @this)
    {
        public Color ToAvaloniaColor()
        {
            return new Color(@this.Alpha, @this.Red, @this.Green, @this.Blue);
        }

        public static ArgbColor FromAvaloniaColor(Color color)
        {
            return new ArgbColor(color.A, color.R, color.G, color.B);
        }
    }
}
