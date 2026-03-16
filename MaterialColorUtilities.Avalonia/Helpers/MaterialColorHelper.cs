using System;
using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal static class MaterialColorHelper
{
    internal static readonly IBrush TransparentBrush = new ImmutableSolidColorBrush(Colors.Transparent);

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

    public static IObservable<Color> ProvideSysColorBinding(
        IAvaloniaXamlIlParentStackProvider parentStack,
        SysColorToken token,
        string? customKey = null,
        Color? fallback = null,
        ThemeVariant? themeVariant = null
    )
    {
        if (TokenHelper.IsCustom(token) && String.IsNullOrWhiteSpace(customKey))
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");

        var source = ResolveSource(parentStack);
        var fallbackColor = fallback ?? Colors.Transparent;
        var themeHost = ResolveThemeHost(parentStack);
        var normalizedKey = customKey?.Trim();

        Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> resolveColor = (scheme, theme) =>
            scheme?.ResolveSys(token, theme, normalizedKey) ?? fallbackColor;
        return new MaterialColorObservable(source, Application.Current, themeHost, fallbackColor, resolveColor, themeVariant);
    }

    public static IObservable<Color> ProvideRefColorBinding(
        IAvaloniaXamlIlParentStackProvider parentStack,
        RefPaletteToken token,
        byte tone,
        string? customKey = null,
        Color? fallback = null
    )
    {
        if (TokenHelper.IsCustom(token) && String.IsNullOrWhiteSpace(customKey))
            throw new InvalidOperationException($"Token '{token}' requires a non-empty custom key.");

        var source = ResolveSource(parentStack);
        var fallbackColor = fallback ?? Colors.Transparent;
        var themeHost = ResolveThemeHost(parentStack);
        var normalizedKey = customKey?.Trim();

        Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> resolveColor = (scheme, theme) =>
            scheme?.ResolveRef(token, tone, normalizedKey) ?? fallbackColor;
        return new MaterialColorObservable(source, Application.Current, themeHost, fallbackColor, resolveColor, null);
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

    private static AvaloniaObject? ResolveSource(IAvaloniaXamlIlParentStackProvider parentStack)
    {
        if (parentStack is not { Parents: { } parents })
            return null;

        foreach (var parent in parents)
        {
            if (parent is StyledElement styled)
                return styled;
        }

        return null;
    }

    private static IThemeVariantHost? ResolveThemeHost(IAvaloniaXamlIlParentStackProvider parentStack)
    {
        if (parentStack is not { Parents: { } parents })
            return null;

        foreach (var context in parents)
        {
            if (context is IThemeVariantHost host)
                return host;
        }

        return null;
    }

    internal static IObservable<Color> ObserveSysColor(
        AvaloniaObject? source,
        IThemeVariantHost? themeHost,
        SysColorToken token,
        string? customKey,
        Color fallbackColor,
        ThemeVariant? themeVariant = null
    )
    {
        Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> resolveColor = (scheme, theme) =>
            scheme?.ResolveSys(token, theme, customKey) ?? fallbackColor;
        return new MaterialColorObservable(source, Application.Current, themeHost, fallbackColor, resolveColor, themeVariant);
    }

    internal static IObservable<Color> ObserveRefColor(
        AvaloniaObject? source,
        IThemeVariantHost? themeHost,
        RefPaletteToken token,
        byte tone,
        string? customKey,
        Color fallbackColor
    )
    {
        Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> resolveColor = (scheme, theme) =>
            scheme?.ResolveRef(token, tone, customKey) ?? fallbackColor;
        return new MaterialColorObservable(source, Application.Current, themeHost, fallbackColor, resolveColor, null);
    }

    private sealed class MaterialColorObservable(
        AvaloniaObject? source,
        Application? application,
        IThemeVariantHost? themeHost,
        Color fallbackColor,
        Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> resolveColor,
        ThemeVariant? themeVariant
    )
        : IObservable<Color>
    {
        private readonly AvaloniaObject? _source = source;
        private readonly Application? _application = application;
        private readonly IThemeVariantHost? _themeHost = themeHost;
        private readonly Color _fallbackColor = fallbackColor;
        private readonly Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> _resolveColor = resolveColor;
        private readonly ThemeVariant? _themeVariant = themeVariant;

        public IDisposable Subscribe(IObserver<Color> observer)
        {
            return new Subscription(this, observer);
        }

        private sealed class Subscription : IDisposable
        {
            private readonly MaterialColorObservable _owner;
            private readonly IObserver<Color> _observer;
            private readonly PropertyChangedEventHandler _schemeChangedHandler;
            private readonly IDisposable? _sourceSubscription;
            private readonly IDisposable? _applicationSubscription;
            private MaterialColorScheme? _sourceSchemeHost;
            private MaterialColorScheme? _applicationSchemeHost;
            private bool _isDisposed;

            public Subscription(MaterialColorObservable owner, IObserver<Color> observer)
            {
                _owner = owner;
                _observer = observer;
                _schemeChangedHandler = (_, _) => Publish();

                if (_owner._source is not null)
                {
                    _sourceSubscription = _owner._source
                        .GetObservable(MaterialColor.SchemeHostProperty)
                        .Subscribe(new Observer<MaterialColorScheme?>(OnSourceSchemeChanged));
                }

                if (_owner._application is not null && !ReferenceEquals(_owner._source, _owner._application))
                {
                    _applicationSubscription = _owner._application
                        .GetObservable(MaterialColor.SchemeHostProperty)
                        .Subscribe(new Observer<MaterialColorScheme?>(OnApplicationSchemeChanged));
                }

                if (_owner._themeVariant is null && _owner._themeHost is not null)
                {
                    _owner._themeHost.ActualThemeVariantChanged += OnActualThemeVariantChanged;
                }

                Publish();
            }

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                if (_owner._themeVariant is null && _owner._themeHost is not null)
                {
                    _owner._themeHost.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
                }

                _sourceSubscription?.Dispose();
                _applicationSubscription?.Dispose();
                SetSchemeHost(ref _sourceSchemeHost, null);
                SetSchemeHost(ref _applicationSchemeHost, null);
            }

            private void OnSourceSchemeChanged(MaterialColorScheme? schemeHost)
            {
                SetSchemeHost(ref _sourceSchemeHost, schemeHost);
                Publish();
            }

            private void OnApplicationSchemeChanged(MaterialColorScheme? schemeHost)
            {
                SetSchemeHost(ref _applicationSchemeHost, schemeHost);
                Publish();
            }

            private void OnActualThemeVariantChanged(object? sender, EventArgs e)
            {
                Publish();
            }

            private void SetSchemeHost(ref MaterialColorScheme? current, MaterialColorScheme? next)
            {
                if (ReferenceEquals(current, next))
                    return;

                if (current is not null)
                {
                    current.Internal.PropertyChanged -= _schemeChangedHandler;
                }

                current = next;

                if (current is not null)
                {
                    current.Internal.PropertyChanged += _schemeChangedHandler;
                }
            }

            private void Publish()
            {
                if (_isDisposed)
                    return;

                var theme = _owner._themeVariant ?? _owner._themeHost?.ActualThemeVariant ?? ThemeVariant.Light;
                var scheme = _sourceSchemeHost?.Internal ?? _applicationSchemeHost?.Internal;
                var color = scheme is null ? _owner._fallbackColor : _owner._resolveColor(scheme, theme);

                _observer.OnNext(color);
            }
        }
    }

    private sealed class Observer<T>(Action<T> onNext) : IObserver<T>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            onNext(value);
        }
    }
}