using Avalonia;
using Avalonia.Media;
using DesignTokens;

namespace MaterialColorUtilities.Avalonia;

public class MaterialColor : AvaloniaObject
{
    static MaterialColor()
    {
        SchemeProperty.Changed.AddClassHandler<AvaloniaObject>(OnSchemeChanged);
    }

    public static readonly AttachedProperty<ColorScheme?> SchemeProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, ColorScheme?>("Scheme");

    public static ColorScheme? GetScheme(AvaloniaObject element)
    {
        return element.GetValue(SchemeProperty);
    }

    public static void SetScheme(AvaloniaObject element, ColorScheme? value)
    {
        element.SetValue(SchemeProperty, value);
    }

    private static void OnSchemeChanged(AvaloniaObject element, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not ColorScheme nextScheme)
        {
            ClearResolvers(element);
            return;
        }

        SetResolvers(element, new MaterialColorScheme(nextScheme));
    }

    private static void ClearResolvers(AvaloniaObject element)
    {
        element.ClearValue(MaterialColorSchemeHost.SysColorHostProperty);
        element.ClearValue(MaterialColorSchemeHost.RefPaletteHostProperty);
    }

    private static void SetResolvers(AvaloniaObject element, MaterialColorScheme resolver)
    {
        element.SetValue(MaterialColorSchemeHost.SysColorHostProperty, resolver);
        element.SetValue(MaterialColorSchemeHost.RefPaletteHostProperty, resolver);
    }
}
