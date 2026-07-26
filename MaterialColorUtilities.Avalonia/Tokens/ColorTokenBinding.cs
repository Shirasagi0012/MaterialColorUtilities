using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Tokens;

/// <summary>
/// Builds the bindings behind the color markup extensions.
/// </summary>
/// <remarks>
/// Every binding here is rooted at <c>$self</c>, so it resolves against whatever object it is
/// ultimately attached to rather than against something captured while the XAML was parsed. That
/// is what makes a single <c>Setter</c> in a <c>ControlTheme</c> produce
/// per-control values: <c>Setter</c> instantiates its binding once per target, and a
/// <c>$self</c>-rooted compiled binding takes the target as its source.
/// <para>
/// Objects outside any control tree (a brush declared in a <c>ResourceDictionary</c>, say) cannot
/// inherit anything, so each binding also carries an application-level child as a fallback.
/// </para>
/// </remarks>
internal static class ColorTokenBinding
{
    private static readonly IImmutableBrush TransparentBrush = new ImmutableSolidColorBrush(Colors.Transparent);

    public static BindingBase SysColor(SysColorToken token, string? customKey, ThemeVariant? pinnedVariant, bool asBrush)
    {
        var layout = new SourceLayout(pinnedVariant);
        object fallback = asBrush ? TransparentBrush : Colors.Transparent;

        return new MultiBinding
        {
            Mode = BindingMode.OneWay,
            Bindings = layout.Bindings,
            FallbackValue = fallback,
            Converter = new FuncMultiValueConverter<object?, object?>(values =>
            {
                if (layout.ResolveScheme(values) is not { } scheme)
                    return fallback;

                var isDark = ColorScheme.IsDark(pinnedVariant ?? layout.ResolveVariant(values));

                if (asBrush)
                    return scheme.TryGetBrush(token, isDark, customKey, out var brush) ? brush : fallback;

                return scheme.TryGetColor(token, isDark, customKey, out var color) ? color : fallback;
            })
        };
    }

    public static BindingBase RefPalette(RefPaletteToken palette, byte tone, string? customKey, bool asBrush)
    {
        // Reference palettes are generated from the seed color alone, so unlike the system roles
        // they do not vary with the theme variant and need no variant input.
        var layout = new SourceLayout(pinnedVariant: ThemeVariant.Light);
        object fallback = asBrush ? TransparentBrush : Colors.Transparent;

        return new MultiBinding
        {
            Mode = BindingMode.OneWay,
            Bindings = layout.Bindings,
            FallbackValue = fallback,
            Converter = new FuncMultiValueConverter<object?, object?>(values =>
            {
                if (layout.ResolveScheme(values) is not { } scheme)
                    return fallback;

                if (!scheme.TryGetPaletteColor(palette, tone, customKey, out var color))
                    return fallback;

                return asBrush ? new ImmutableSolidColorBrush(color) : color;
            })
        };
    }

    /// <summary>
    /// Assembles the child bindings and remembers which slot each input landed in. The variant
    /// children are omitted when the caller pinned a variant, and the application children when
    /// there is no <see cref="Application.Current" /> (unit tests constructing bindings by hand).
    /// </summary>
    private sealed class SourceLayout
    {
        private readonly int _selfScheme;
        private readonly int _selfVariant;
        private readonly int _appScheme;
        private readonly int _appVariant;

        public SourceLayout(ThemeVariant? pinnedVariant)
        {
            var app = Application.Current;
            var next = 0;

            Bindings = new List<BindingBase> { Self(MaterialColor.ResolvedSchemeProperty) };
            _selfScheme = next++;

            if (pinnedVariant is null)
            {
                Bindings.Add(Self(ThemeVariantScope.ActualThemeVariantProperty));
                _selfVariant = next++;
            }
            else
            {
                _selfVariant = -1;
            }

            if (app is { })
            {
                Bindings.Add(FixedSource(app, MaterialColor.ResolvedSchemeProperty));
                _appScheme = next++;

                if (pinnedVariant is null)
                {
                    Bindings.Add(FixedSource(app, ThemeVariantScope.ActualThemeVariantProperty));
                    _appVariant = next;
                }
                else
                {
                    _appVariant = -1;
                }
            }
            else
            {
                _appScheme = -1;
                _appVariant = -1;
            }
        }

        public List<BindingBase> Bindings { get; }

        public ResolvedColorScheme? ResolveScheme(IReadOnlyList<object?> values)
        {
            return At<ResolvedColorScheme>(values, _selfScheme) ?? At<ResolvedColorScheme>(values, _appScheme);
        }

        public ThemeVariant ResolveVariant(IReadOnlyList<object?> values)
        {
            if (At<ThemeVariant>(values, _selfVariant) is { } self && self != ThemeVariant.Default)
                return self;

            return At<ThemeVariant>(values, _appVariant) ?? ThemeVariant.Default;
        }

        private static T? At<T>(IReadOnlyList<object?> values, int index) where T : class
        {
            return index >= 0 && index < values.Count ? values[index] as T : null;
        }

        /// <summary>
        /// A binding rooted at the target object itself. There is no expression-tree equivalent:
        /// <see cref="CompiledBinding.Create{TIn,TOut}" /> builds an unrooted path, which resolves
        /// against the target's <c>DataContext</c> unless a source is supplied.
        /// </summary>
        private static BindingBase Self(AvaloniaProperty property)
        {
            return new CompiledBinding(
                new CompiledBindingPathBuilder()
                    .Self()
                    .Property(property, PropertyInfoAccessorFactory.CreateAvaloniaPropertyAccessor)
                    .Build())
            {
                Mode = BindingMode.OneWay
            };
        }

        private static BindingBase FixedSource(AvaloniaObject source, AvaloniaProperty property)
        {
            return CompiledBinding.Create<AvaloniaObject, object?>(
                o => o[property],
                source,
                mode: BindingMode.OneWay);
        }
    }
}
