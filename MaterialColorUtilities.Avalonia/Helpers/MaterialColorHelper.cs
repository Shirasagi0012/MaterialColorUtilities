using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using MaterialColorUtilities.DynamicColors;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal static class MaterialColorHelper
{
    private static readonly IBrush TransparentBrush = new ImmutableSolidColorBrush(Colors.Transparent);
    private static readonly Dictionary<uint, IBrush> BrushCache = [];

    private static readonly ClrPropertyInfo ThemeVariantPropertyInfo =
        new(
            nameof(IThemeVariantHost.ActualThemeVariant),
            target => target is IThemeVariantHost host ? host.ActualThemeVariant : ThemeVariant.Light,
            (_, _) => { },
            typeof(ThemeVariant)
        );

    public static (IProvideValueTarget, IAvaloniaXamlIlParentStackProvider) GetContextServices(
        IServiceProvider services
    )
    {
        if (services.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget target)
            throw new InvalidOperationException($"Service '{nameof(IProvideValueTarget)}' not found.");

        if (services.GetService(typeof(IAvaloniaXamlIlParentStackProvider)) is not IAvaloniaXamlIlParentStackProvider
            parentStack)
            throw new InvalidOperationException($"Service '{nameof(IAvaloniaXamlIlParentStackProvider)}' not found.");

        return (target, parentStack);
    }

    public static IBinding ProvideSysColorBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        Color? fallback = null
    )
    {
        if (TokenHelper.IsCustom(token) && String.IsNullOrWhiteSpace(customKey))
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");

        var fallbackColor = fallback ?? Colors.Transparent;
        var (scheme, themeHost) = ResolveSchemeAndThemeHost(target, parentStack);
        if (scheme is null)
            return CreateConstantBinding(fallbackColor);

        var normalizedKey = customKey?.Trim();
        return CreateThemeAwareBinding(
            themeHost,
            scheme,
            fallbackColor,
            theme => ResolveSysColor(scheme, token, normalizedKey, theme, fallbackColor)
        );
    }

    public static IBinding ProvideRefColorBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        RefPaletteToken token,
        byte tone,
        string? customKey = null,
        Color? fallback = null
    )
    {
        if (TokenHelper.IsCustom(token) && String.IsNullOrWhiteSpace(customKey))
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");

        var fallbackColor = fallback ?? Colors.Transparent;
        var (scheme, themeHost) = ResolveSchemeAndThemeHost(target, parentStack);
        if (scheme is null)
            return CreateConstantBinding(fallbackColor);

        var normalizedKey = customKey?.Trim();
        return CreateThemeAwareBinding(
            themeHost,
            scheme,
            fallbackColor,
            theme => ResolveRefColor(scheme, token, tone, normalizedKey, theme, fallbackColor)
        );
    }

    public static IBinding ProvideSysBrushBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        IBrush? fallback = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideSysColorBinding(target, parentStack, token, customKey);
        return CreateBrushBinding(colorBinding, fallbackBrush);
    }

    public static IBinding ProvideRefBrushBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        RefPaletteToken token,
        byte tone,
        string? customKey = null,
        IBrush? fallback = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideRefColorBinding(target, parentStack, token, tone, customKey);
        return CreateBrushBinding(colorBinding, fallbackBrush);
    }

    private static IBinding CreateBrushBinding(IBinding colorBinding, IBrush fallbackBrush)
    {
        return new MultiBinding
        {
            Priority = BindingPriority.Style,
            FallbackValue = fallbackBrush,
            Bindings = { colorBinding },
            Converter = new FuncMultiValueConverter<object?, IBrush>(values =>
                (values as IList<object?> ?? values.ToList()) switch
                {
                    [Color color, ..] => GetCachedBrush(color),
                    [IBrush brush, ..] => brush,
                    _ => fallbackBrush
                })
        };
    }

    private static IBrush GetCachedBrush(Color color)
    {
        var key =
            ((uint)color.A << 24)
            | ((uint)color.R << 16)
            | ((uint)color.G << 8)
            | color.B;

        if (BrushCache.TryGetValue(key, out var brush))
            return brush;

        if (BrushCache.Count > 512)
            BrushCache.Clear();

        brush = new ImmutableSolidColorBrush(color);
        BrushCache[key] = brush;
        return brush;
    }

    private static IBinding CreateThemeAwareBinding(
        IThemeVariantHost? themeHost,
        MaterialColorScheme scheme,
        Color fallbackColor,
        Func<ThemeVariant, Color> resolveColor
    )
    {
        var defaultTheme = themeHost?.ActualThemeVariant ?? ThemeVariant.Light;

        var converter =
            new FuncValueConverter<ThemeVariant, Color>(variant =>
                variant switch
                {
                    { } => resolveColor(variant),
                    _ => resolveColor(defaultTheme)
                });

        var themeBinding = themeHost is { }
            ? CreateCompiledBinding(themeHost, ThemeVariantPropertyInfo, fallbackColor, converter)
            : CreateConstantBinding(defaultTheme);

        return themeBinding;
    }

    private static IBinding CreateCompiledBinding(
        object source,
        ClrPropertyInfo propertyInfo,
        object fallbackValue,
        IValueConverter? converter = null
    )
    {
        return new CompiledBindingExtension
        {
            Mode = BindingMode.OneWay,
            Priority = BindingPriority.Style,
            Source = source,
            FallbackValue = fallbackValue,
            Path = new CompiledBindingPathBuilder()
                .Property(propertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build(),
            Converter = converter
        };
    }

    private static IBinding CreateConstantBinding(object value)
    {
        return new Binding
        {
            Mode = BindingMode.OneTime,
            Priority = BindingPriority.Style,
            Source = value
        };
    }

    private static IEnumerable<object> EnumerateContextObjects(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack
    )
    {
        if (target is { TargetObject: { } targetObject })
            yield return targetObject;

        if (parentStack is { Parents: { } parents })
            foreach (var parent in parents)
                yield return parent;
    }

    public static bool ShouldProvideBrush(IProvideValueTarget provideValueTarget)
    {
        if (provideValueTarget.TargetProperty switch
            {
                AvaloniaProperty avaloniaProperty => avaloniaProperty.PropertyType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => null
            } is not { } type)
            return true;

        return type != typeof(Color) && type != typeof(Color?);
    }

    private static (MaterialColorScheme?, IThemeVariantHost?) ResolveSchemeAndThemeHost(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack
    )
    {
        MaterialColorScheme? scheme = null;
        IThemeVariantHost? themeHost = null;
        foreach (var context in EnumerateContextObjects(target, parentStack))
        {
            if (context is AvaloniaObject avaloniaObject && scheme is null)
                scheme = MaterialColor.GetScheme(avaloniaObject);

            if (context is IThemeVariantHost host && themeHost is null)
                themeHost = host;

            if (scheme is { } && themeHost is { })
                break;
        }

        return (scheme, themeHost);
    }


    private static Color ResolveSysColor(
        MaterialColorScheme scheme,
        SysColorToken token,
        string? customKey,
        ThemeVariant themeVariant,
        Color fallbackColor
    )
    {
        try
        {
            if (TokenHelper.IsCustom(token))
                return scheme.Resolve(customKey!, token, themeVariant) ?? fallbackColor;

            return scheme.Resolve(token, themeVariant) ?? fallbackColor;
        }
        catch
        {
            return fallbackColor;
        }
    }

    private static Color ResolveRefColor(
        MaterialColorScheme scheme,
        RefPaletteToken token,
        byte tone,
        string? customKey,
        ThemeVariant themeVariant,
        Color fallbackColor
    )
    {
        try
        {
            if (TokenHelper.IsCustom(token))
                return scheme.Resolve(customKey!, token, tone, themeVariant) ?? fallbackColor;

            return scheme.Resolve(token, tone, themeVariant) ?? fallbackColor;
        }
        catch
        {
            return fallbackColor;
        }
    }
}
