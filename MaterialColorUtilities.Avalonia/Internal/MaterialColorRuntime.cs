using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Internal;

internal delegate Color? MaterialColorResolver(MaterialColorScheme scheme, ThemeVariant themeVariant);

internal static class MaterialColorRuntime
{
    private sealed class MaterialColorRuntimeHost : AvaloniaObject;

    private static readonly AttachedProperty<Dictionary<AvaloniaProperty, IDisposable>?> RuntimeSubscriptionsProperty =
        AvaloniaProperty.RegisterAttached<MaterialColorRuntimeHost, AvaloniaObject, Dictionary<AvaloniaProperty, IDisposable>?>(
            "RuntimeSubscriptions"
        );

    public static AvaloniaObject? ResolveAnchor(IServiceProvider serviceProvider)
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

    public static object ProvideColor(IServiceProvider serviceProvider, SysColorToken token, Color? fallback = null)
    {
        var fallbackColor = fallback ?? Colors.Transparent;
        var anchor = ResolveAnchor(serviceProvider);
        var value = ResolveColor(anchor, token, fallbackColor);

        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget
            {
                TargetObject: AvaloniaObject target,
                TargetProperty: AvaloniaProperty property
            })
        {
            if (property.PropertyType != typeof(Color) && property.PropertyType != typeof(Color?))
                return value;

            AttachSubscription(target, property, anchor, token, fallbackColor);
        }

        return value;
    }

    public static object ProvideBrush(IServiceProvider serviceProvider, SysColorToken token, Color? fallback = null)
    {
        var fallbackColor = fallback ?? Colors.Transparent;
        var anchor = ResolveAnchor(serviceProvider);
        var brush = new SolidColorBrush(ResolveColor(anchor, token, fallbackColor));
        AttachSubscription(brush, SolidColorBrush.ColorProperty, anchor, token, fallbackColor);
        return brush;
    }

    private static Color ResolveColor(AvaloniaObject? anchor, SysColorToken token, Color fallback)
    {
        if (anchor is null)
            return fallback;

        var scheme = MaterialColor.GetScheme(anchor);
        if (scheme is null)
            return fallback;

        try
        {
            var theme = GetThemeVariant(anchor);
            return scheme.Resolve(token, theme) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static void AttachSubscription(
        AvaloniaObject target,
        AvaloniaProperty property,
        AvaloniaObject? anchor,
        SysColorToken token,
        Color fallback
    )
    {
        if (anchor is null)
            return;

        var subscriptions = target.GetValue(RuntimeSubscriptionsProperty);
        if (subscriptions is null)
        {
            subscriptions = new Dictionary<AvaloniaProperty, IDisposable>();
            target.SetValue(RuntimeSubscriptionsProperty, subscriptions);
        }

        if (subscriptions.TryGetValue(property, out var current))
            current.Dispose();

        var runtimeSubscription = new RuntimeSubscription(target, property, anchor, resolver, fallback);
        subscriptions[property] = runtimeSubscription;
    }

    private sealed class RuntimeSubscription : IDisposable
    {
        private readonly WeakReference<AvaloniaObject> _targetReference;
        private readonly AvaloniaProperty _targetProperty;
        private readonly AvaloniaObject _anchor;
        private readonly MaterialColorResolver _resolver;
        private readonly Color _fallback;

        private IDisposable? _schemeReferenceSubscription;
        private IThemeVariantHost? _themeHost;
        private MaterialColorScheme? _scheme;
        private bool _disposed;

        public RuntimeSubscription(
            AvaloniaObject target,
            AvaloniaProperty targetProperty,
            AvaloniaObject anchor,
            MaterialColorResolver resolver,
            Color fallback
        )
        {
            _targetReference = new WeakReference<AvaloniaObject>(target);
            _targetProperty = targetProperty;
            _anchor = anchor;
            _resolver = resolver;
            _fallback = fallback;

            _schemeReferenceSubscription = _anchor.GetObservable(MaterialColor.SchemeProperty)
                .Subscribe(new ActionObserver<MaterialColorScheme?>(_ => OnSchemeReferenceChanged()));

            _themeHost = _anchor as IThemeVariantHost;
            if (_themeHost is not null)
                _themeHost.ActualThemeVariantChanged += OnThemeVariantChanged;

            OnSchemeReferenceChanged();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _schemeReferenceSubscription?.Dispose();
            _schemeReferenceSubscription = null;

            if (_themeHost is not null)
            {
                _themeHost.ActualThemeVariantChanged -= OnThemeVariantChanged;
                _themeHost = null;
            }

            if (_scheme is not null)
            {
                _scheme.Changed -= OnSchemeChanged;
                _scheme = null;
            }
        }

        private void OnSchemeReferenceChanged()
        {
            if (_disposed)
                return;

            var next = MaterialColor.GetScheme(_anchor);
            if (ReferenceEquals(_scheme, next))
            {
                Apply();
                return;
            }

            if (_scheme is not null)
                _scheme.Changed -= OnSchemeChanged;

            _scheme = next;

            if (_scheme is not null)
                _scheme.Changed += OnSchemeChanged;

            Apply();
        }

        private void OnSchemeChanged(object? sender, EventArgs e) => Apply();

        private void OnThemeVariantChanged(object? sender, EventArgs e) => Apply();

        private void Apply()
        {
            if (_disposed)
                return;

            if (!_targetReference.TryGetTarget(out var target))
            {
                Dispose();
                return;
            }

            var color = ResolveColor();
            target.SetValue(_targetProperty, color);
        }

        private Color ResolveColor()
        {
            if (_scheme is null)
                return _fallback;

            try
            {
                var theme = GetThemeVariant(_anchor);
                return _resolver(_scheme, theme) ?? _fallback;
            }
            catch
            {
                return _fallback;
            }
        }
    }

    private sealed class ActionObserver<T>(Action<T> onNext) : IObserver<T>
    {
        public void OnNext(T value) => onNext(value);

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }
    }

    private static ThemeVariant GetThemeVariant(AvaloniaObject anchor)
    {
        if (anchor is IThemeVariantHost host)
            return host.ActualThemeVariant;

        return ThemeVariant.Light;
    }
}
