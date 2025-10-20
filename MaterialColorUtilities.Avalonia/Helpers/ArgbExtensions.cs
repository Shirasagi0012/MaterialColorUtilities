namespace MaterialColorUtilities.Avalonia;

using global::Avalonia.Media;
using Utils;

public static class ArgbExtensions
{
    extension(ArgbColor @this)
    {
        public Color ToAvaloniaColor() => new(a: @this.Alpha, r: @this.Red, g: @this.Green, b: @this.Blue);

        public static ArgbColor FromAvaloniaColor(Color color) => new(alpha: color.A, red: color.R, green: color.G, blue: color.B);
    }
}
