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

    private static readonly AttachedProperty<SchemeHost?> SchemeHostProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, SchemeHost?>(
            "SchemeHost",
            inherits: true);

    public static readonly AttachedProperty<ColorScheme?> SchemeProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, ColorScheme?>(
            "Scheme");

    public static ColorScheme? GetScheme(AvaloniaObject element) => element.GetValue(SchemeProperty);

    public static void SetScheme(AvaloniaObject element, ColorScheme? value) => element.SetValue(SchemeProperty, value);

    private static void OnSchemeChanged(AvaloniaObject element, AvaloniaPropertyChangedEventArgs args)
    {
        var nextScheme = args.NewValue as ColorScheme;
        var host = element.GetValue(SchemeHostProperty);

        if (nextScheme is null)
        {
            host?.Dispose();
            element.ClearValue(SchemeHostProperty);
            ClearResolvers(element);
            return;
        }

        host ??= new SchemeHost(element);
        element.SetValue(SchemeHostProperty, host);
        host.UpdateScheme(nextScheme);
    }

    private static void ClearResolvers(AvaloniaObject element)
    {
        element.ClearValue(TokenHost<Color, RefPaletteTokenKey>.ResolverProperty);
        element.ClearValue(TokenHost<Color, SysColorTokenKey>.ResolverProperty);
    }

    private static void SetResolvers(AvaloniaObject element, MaterialColorScheme resolver)
    {
        element.SetValue(TokenHost<Color, RefPaletteTokenKey>.ResolverProperty, resolver);
        element.SetValue(TokenHost<Color, SysColorTokenKey>.ResolverProperty, resolver);
    }

    private sealed class SchemeHost : IDisposable
    {
        private readonly AvaloniaObject _owner;
        private ColorScheme? _scheme;

        public SchemeHost(AvaloniaObject owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            Scheme = null;
        }

        public void UpdateScheme(ColorScheme scheme)
        {
            Scheme = scheme;
            Publish();
        }

        private ColorScheme? Scheme
        {
            set
            {
                if (ReferenceEquals(_scheme, value))
                    return;

                if (_scheme is { })
                    _scheme.SchemeChanged -= OnSchemeChanged;

                _scheme = value;

                if (_scheme is { })
                    _scheme.SchemeChanged += OnSchemeChanged;
            }
        }

        private void OnSchemeChanged(object? sender, EventArgs e)
        {
            Publish();
        }

        private void Publish()
        {
            if (_scheme is not { Color: { } })
            {
                ClearResolvers(_owner);
                return;
            }

            SetResolvers(_owner, new MaterialColorScheme(_scheme));
        }
    }
}
