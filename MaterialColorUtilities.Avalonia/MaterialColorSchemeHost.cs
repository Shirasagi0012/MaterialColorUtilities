using Avalonia;
using Avalonia.Media;
using DesignTokens;

namespace MaterialColorUtilities.Avalonia;

internal class MaterialColorSchemeHost : 
    ITokenHost<Color, SysColorTokenKey, MaterialColorSchemeHost>,
    ITokenHost<Color, RefPaletteTokenKey, MaterialColorSchemeHost>
{
    public static readonly AttachedProperty<ITokenResolver<Color, SysColorTokenKey>?> SysColorHostProperty =
        AvaloniaProperty.RegisterAttached<MaterialColorSchemeHost, AvaloniaObject, ITokenResolver<Color, SysColorTokenKey>?>(
            "SysColorHost", inherits: true);

    public static readonly AttachedProperty<ITokenResolver<Color, RefPaletteTokenKey>?> RefPaletteHostProperty =
        AvaloniaProperty.RegisterAttached<MaterialColorSchemeHost, AvaloniaObject, ITokenResolver<Color, RefPaletteTokenKey>?>(
            "RefPaletteHost", inherits: true);

    static IObservable<ITokenResolver<Color, SysColorTokenKey>?> ITokenHost<Color, SysColorTokenKey, MaterialColorSchemeHost>.
        GetTokenObservable(AvaloniaObject element)
    {
        return element.GetObservable(SysColorHostProperty);
    }

    static ITokenResolver<Color, RefPaletteTokenKey>? ITokenHost<Color, RefPaletteTokenKey, MaterialColorSchemeHost>.GetResolver(
        AvaloniaObject element
    )
    {
        return element.GetValue(RefPaletteHostProperty);
    }

    static void ITokenHost<Color, RefPaletteTokenKey, MaterialColorSchemeHost>.SetResolver(
        AvaloniaObject element,
        ITokenResolver<Color, RefPaletteTokenKey>? value
    )
    {
        element.SetValue(RefPaletteHostProperty, value);
    }

    static void ITokenHost<Color, RefPaletteTokenKey, MaterialColorSchemeHost>.ClearResolver(AvaloniaObject element)
    {
        element.ClearValue(RefPaletteHostProperty);
    }

    static IObservable<ITokenResolver<Color, RefPaletteTokenKey>?> ITokenHost<Color, RefPaletteTokenKey, MaterialColorSchemeHost>.
        GetTokenObservable(AvaloniaObject element)
    {
        return element.GetObservable(RefPaletteHostProperty);
    }

    static ITokenResolver<Color, SysColorTokenKey>? ITokenHost<Color, SysColorTokenKey, MaterialColorSchemeHost>.GetResolver(
        AvaloniaObject element
    )
    {
        return element.GetValue(SysColorHostProperty);
    }

    static void ITokenHost<Color, SysColorTokenKey, MaterialColorSchemeHost>.SetResolver(
        AvaloniaObject element,
        ITokenResolver<Color, SysColorTokenKey>? value
    )
    {
        element.SetValue(SysColorHostProperty, value);
    }

    static void ITokenHost<Color, SysColorTokenKey, MaterialColorSchemeHost>.ClearResolver(AvaloniaObject element)
    {
        element.ClearValue(SysColorHostProperty);
    }
}
