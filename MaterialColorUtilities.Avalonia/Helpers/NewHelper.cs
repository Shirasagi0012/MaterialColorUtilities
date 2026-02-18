using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Avalonia;
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

internal static class NewHelper
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
        IProvideValueTarget target,
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

        //var scheme = ResolveAttachedProperty(target);
        //if (scheme is null)
        //    return CreateConstantBinding(fallbackColor);

        var themeHost = ResolveThemeHost(target, parentStack);

        var normalizedKey = customKey?.Trim();
        return CreateBinding1(
            parentStack,
            themeHost,
            fallbackColor,
            (scheme, theme) => scheme?.ResolveSys(token, theme, normalizedKey) ?? fallbackColor,
            themeVariant
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

        //var scheme = ResolveAttachedProperty(target);
        var themeHost = ResolveThemeHost(target, parentStack);

        //if (scheme is null)
        //    return CreateConstantBinding(fallbackColor);

        var normalizedKey = customKey?.Trim();
        return CreateBinding1(
            parentStack,
            themeHost,
            fallbackColor,
            (scheme, theme) => scheme?.ResolveRef(token, tone, normalizedKey) ?? fallbackColor
        );
    }

    private static IThemeVariantHost? ResolveThemeHost(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack
    )
    {
        // TODO: debug with ToList()
        var list = Enumerate().ToList();
        foreach (var context in list)
        {
            if (context is IThemeVariantHost host)
            {
                return host;
            }
        }

        return null;

        IEnumerable<object> Enumerate()
        {
            if (target is { TargetObject: { } targetObject })
                yield return targetObject;

            if (parentStack is { Parents: { } parents })
                foreach (var parent in parents)
                    yield return parent;
        }
    }

    private static IBinding CreateConstantBinding(Color fallbackColor)
    {
        return new CompiledBindingExtension
        {
            Mode = BindingMode.OneTime,
            Priority = BindingPriority.Style,
            Source = fallbackColor
        };
    }

    private static IBinding CreateBinding1(
        IAvaloniaXamlIlParentStackProvider parentStack,
        IThemeVariantHost? themeHost,
        Color fallbackColor,
        Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> resolveColor,
        ThemeVariant? themeVariant = null
    )
    {
        var defaultTheme = themeHost?.ActualThemeVariant ?? ThemeVariant.Light;

        var schemeBinding = new CompiledBindingExtension
        {
            Mode = BindingMode.OneWay,
            Priority = BindingPriority.Style,
            Source = (object?)GetFirstStyledElement(parentStack) ?? Application.Current,
            Path = new CompiledBindingPathBuilder()
                .Property(MaterialColor.SchemeProperty, PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor)
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
                .Property(MaterialColor.SchemeProperty, PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor)
                .Property(SchemePropertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Property(SchemeInternalPropertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build(),
        };

        //var debug = MaterialColor.GetScheme(target.TargetObject as AvaloniaObject);

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


            return new MultiBinding
            {
                Mode = BindingMode.OneWay,
                Priority = BindingPriority.Style,
                FallbackValue = fallbackColor, TargetNullValue = fallbackColor,
                Bindings = [schemeBinding, appSchemeBinding, themeBinding],
                Converter = new FuncMultiValueConverter<object?, Color>(values =>
                    (values as IList<object?> ?? values.ToList()) switch
                    {
                        [var scheme, var appScheme, ThemeVariant theme, ..] => resolveColor(
                            scheme as MaterialColorScheme.MaterialColorSchemeInternal ??
                            appScheme as MaterialColorScheme.MaterialColorSchemeInternal,
                            theme),
                        _ => fallbackColor
                    }),
            };
        }
        else // if explicit theme is provided
        {
            return new MultiBinding
            {
                Mode = BindingMode.OneWay,
                Priority = BindingPriority.Style,
                FallbackValue = fallbackColor, TargetNullValue = fallbackColor,
                Bindings = [schemeBinding, appSchemeBinding],
                Converter = new FuncMultiValueConverter<object?, Color>(values =>
                    (values as IList<object?> ?? values.ToList()) switch
                    {
                        [var scheme, var appScheme, ..] => resolveColor(
                            scheme as MaterialColorScheme.MaterialColorSchemeInternal ??
                            appScheme as MaterialColorScheme.MaterialColorSchemeInternal,
                            themeVariant),
                        _ => fallbackColor
                    }),
            };
        }

        static StyledElement? GetFirstStyledElement(
            IAvaloniaXamlIlParentStackProvider parentStack
        )
        {
            if (parentStack is { Parents: { } parents })
                foreach (var parent in parents)
                    if (parent is StyledElement styled)
                        return styled;
            return null;
        }

        static IEnumerable<StyledElement> EnumerateScopeChain(StyledElement start)
        {
            var visited = new HashSet<StyledElement>();

            for (var current = start; current is { } && visited.Add(current);)
            {
                yield return current;
                current = current.Parent ?? current.TemplatedParent as StyledElement;
            }
        }
    }

    public static IBinding ProvideSysBrushBinding(
        IProvideValueTarget target,
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        IBrush? fallback = null,
        ThemeVariant? themeVariant = null
    )
    {
        var fallbackBrush = fallback ?? TransparentBrush;
        var colorBinding = ProvideSysColorBinding(target, parentStack, token, customKey, themeVariant: themeVariant);
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
