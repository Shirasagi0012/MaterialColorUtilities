using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
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

namespace MaterialColorUtilities.Avalonia.Helpers;

internal static class MaterialColorHelper
{
    private static readonly IBrush TransparentBrush = new ImmutableSolidColorBrush(Colors.Transparent);

    private static readonly ClrPropertyInfo ThemeVariantPropertyInfo =
        new(
            nameof(IThemeVariantHost.ActualThemeVariant),
            target => target is IThemeVariantHost host ? host.ActualThemeVariant : null,
            null,
            typeof(ThemeVariant)
        );

    private static readonly ClrPropertyInfo SchemePropertyInfo =
        new(
            "Internal",
            target => (target as MaterialColorScheme)?.Internal,
            null,
            typeof(MaterialColorScheme)
        );

    private static readonly ClrPropertyInfo SchemeInternalPropertyInfo =
        new(
            "",
            target => target,
            null,
            typeof(MaterialColorScheme.MaterialColorSchemeInternal)
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
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        Color? fallback = null,
        ThemeVariant? themeVariant = null
    )
    {
        if (TokenHelper.IsCustom(token) && String.IsNullOrWhiteSpace(customKey))
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");

        var fallbackColor = fallback ?? Colors.Transparent;

        var themeHost = ResolveThemeHost(parentStack);

        var normalizedKey = customKey?.Trim();
        return CreateBinding(
            parentStack,
            themeHost,
            fallbackColor,
            (scheme, theme) => scheme?.ResolveSys(token, theme, normalizedKey) ?? fallbackColor,
            themeVariant
        );
    }

    public static IBinding ProvideRefColorBinding(
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

        var themeHost = ResolveThemeHost(parentStack);

        var normalizedKey = customKey?.Trim();
        return CreateBinding(
            parentStack,
            themeHost,
            fallbackColor,
            (scheme, theme) => scheme?.ResolveRef(token, tone, normalizedKey) ?? fallbackColor
        );
    }

    private static IThemeVariantHost? ResolveThemeHost(IAvaloniaXamlIlParentStackProvider parentStack)
    {
        if (parentStack is not { Parents: { } parents }) return null;
        foreach (var context in parents)
        {
            if (context is IThemeVariantHost host)
                return host;
        }

        return null;
    }

    private static IBinding CreateBinding(
        IAvaloniaXamlIlParentStackProvider parentStack,
        IThemeVariantHost? themeHost,
        Color fallbackColor,
        Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> resolveColor,
        ThemeVariant? themeVariant = null
    )
    {
        var defaultTheme = themeHost?.ActualThemeVariant ?? ThemeVariant.Light;

        object? source = null;
        if (parentStack is { Parents: { } parents })
            foreach (var parent in parents)
                if (parent is StyledElement styled)
                {
                    source = styled;
                    break;
                }

        var schemeBinding = new CompiledBindingExtension
        {
            Mode = BindingMode.OneWay,
            Priority = BindingPriority.Style,
            Source = source ?? Application.Current,
            Path = new CompiledBindingPathBuilder()
                .Property(MaterialColor.SchemeHostProperty, PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor)
                .Property(SchemePropertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Property(SchemeInternalPropertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build(),
        };

        // We must keep app binding because attached property set on application will not inherit down to elements in 
        // logical tree (e.g. all controls in window).
        var appSchemeBinding = new CompiledBindingExtension
        {
            Mode = BindingMode.OneWay,
            Priority = BindingPriority.Style,
            Source = Application.Current,
            Path = new CompiledBindingPathBuilder()
                .Property(MaterialColor.SchemeHostProperty, PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor)
                .Property(SchemePropertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Property(SchemeInternalPropertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build(),
        };

        var binding = new MultiBinding
        {
            Mode = BindingMode.OneWay,
            Priority = BindingPriority.Style,
            FallbackValue = fallbackColor, TargetNullValue = fallbackColor
        };

        // need theme binding?
        if (themeVariant is null)
        {
            var themeBinding = new CompiledBindingExtension
            {
                Mode = BindingMode.OneWay,
                Priority = BindingPriority.Style,
                FallbackValue = defaultTheme,
                Source = themeHost,
                Path = new CompiledBindingPathBuilder()
                    .Property(ThemeVariantPropertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                    .Build(),
            };

            binding.Bindings = [schemeBinding, appSchemeBinding, themeBinding];
            binding.Converter = new FuncMultiValueConverter<object?, Color>(values =>
                (values as IList<object?> ?? values.ToList()) switch
                {
                    [var scheme, var appScheme, ThemeVariant theme, ..] => resolveColor(
                        scheme as MaterialColorScheme.MaterialColorSchemeInternal ??
                        appScheme as MaterialColorScheme.MaterialColorSchemeInternal,
                        theme),
                    _ => fallbackColor
                });
        }
        else // if explicit theme is provided
        {
            binding.Bindings = [schemeBinding, appSchemeBinding];
            binding.Converter = new FuncMultiValueConverter<object?, Color>(values =>
                (values as IList<object?> ?? values.ToList()) switch
                {
                    [var scheme, var appScheme, ..] => resolveColor(
                        scheme as MaterialColorScheme.MaterialColorSchemeInternal ??
                        appScheme as MaterialColorScheme.MaterialColorSchemeInternal,
                        themeVariant),
                    _ => fallbackColor
                });
        }

        return binding;
    }

    public static IBinding ProvideSysBrushBinding(
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        IBrush? fallback = null,
        ThemeVariant? themeVariant = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideSysColorBinding(parentStack, token, customKey, themeVariant: themeVariant);
        return CreateBrushBinding(colorBinding, fallbackBrush);
    }

    public static IBinding ProvideRefBrushBinding(
        IAvaloniaXamlIlParentStackProvider parentStack,
        RefPaletteToken token,
        byte tone,
        string? customKey = null,
        IBrush? fallback = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideRefColorBinding(parentStack, token, tone, customKey);
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
                    // TODO: Brush cache
                    [Color color, ..] => new SolidColorBrush(color),
                    [IBrush brush, ..] => brush,
                    _ => fallbackBrush
                })
        };
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
}
