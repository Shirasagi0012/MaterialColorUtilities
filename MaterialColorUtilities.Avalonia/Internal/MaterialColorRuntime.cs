using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Internal;

internal delegate Color? MaterialColorResolver(MaterialColorScheme scheme, ThemeVariant themeVariant);

internal static class MaterialColorRuntime
{
    private static AvaloniaObject? ResolveAnchor(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget
            {
                TargetObject: AvaloniaObject targetObject
            })
        {
            return targetObject;
        }

        if (serviceProvider.GetService(typeof(IAvaloniaXamlIlParentStackProvider)) is
            IAvaloniaXamlIlEagerParentStackProvider { Parents: { } parents })
        {
            foreach (var parent in parents)
            {
                if (parent is AvaloniaObject obj)
                    return obj;
            }
        }

        return null;
    }

    private static MaterialColorScheme? ResolveScheme(AvaloniaObject? anchor)
    {
        return MaterialColor.GetScheme(anchor);
    }

    public static Color ProvideColor(IServiceProvider serviceProvider, SysColorToken token, Color? fallback = null)
    {
        var fallbackColor = fallback ?? Colors.Transparent;
        var anchor = ResolveAnchor(serviceProvider);
        var scheme = MaterialColor.GetScheme(anchor);
        var theme = GetThemeVariant(anchor);
        var value = ResolveColor(scheme, token, theme, fallbackColor);
        return value;
    }

    public static IBinding ProvideColorBinding(
        IServiceProvider serviceProvider,
        SysColorToken token,
        Color? fallback = null
    )
    {
        var fallbackColor = fallback ?? Colors.Transparent;
        var anchor = ResolveAnchor(serviceProvider);
        var scheme = MaterialColor.GetScheme(anchor);
        var theme = GetThemeVariant(anchor);
        
    }
    
    public static Color ProvideColor(IServiceProvider serviceProvider, RefPaletteToken token, byte tone, Color? fallback = null)
    {
        var fallbackColor = fallback ?? Colors.Transparent;
        var anchor = ResolveAnchor(serviceProvider);
        var scheme = MaterialColor.GetScheme(anchor);
        var value = ResolveColor(scheme, token, tone, fallbackColor);
        return value;
    }

    private static Color ResolveColor(MaterialColorScheme? scheme, SysColorToken token, ThemeVariant theme, Color fallback)
    {
        try
        {
            return scheme?.Resolve(token, theme) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static IBinding ResolveColorBinding(
        MaterialColorScheme scheme,
        SysColorToken token,
        ThemeVariant theme,
        Color fallback
    )
    {
        var color = scheme?.Resolve(token, theme) ?? fallback;
        return new CompiledBindingExtension()
        {
            Path = "",
            Priority = BindingPriority.Style,
            Source = scheme,
            FallbackValue = fallback,
        };
    }

    private static Color ResolveColor(MaterialColorScheme? scheme, RefPaletteToken token, byte tone, Color fallback)
    {
        try
        {
            return scheme?.Resolve(token, tone) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
    private static ThemeVariant GetThemeVariant(AvaloniaObject? anchor)
    {
        if (anchor is IThemeVariantHost host)
            return host.ActualThemeVariant;

        return ThemeVariant.Light;
    }
}
