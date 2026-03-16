using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal sealed class MaterialColorObservable(
    MaterialColorBindingContext context,
    Application? application,
    Color fallbackColor,
    Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> resolveColor
) : IObservable<Color>
{
    private readonly MaterialColorBindingContext _context = context;
    private readonly Application? _application = application;
    private readonly Color _fallbackColor = fallbackColor;
    private readonly Func<MaterialColorScheme.MaterialColorSchemeInternal?, ThemeVariant, Color> _resolveColor = resolveColor;

    public IDisposable Subscribe(IObserver<Color> observer)
    {
        return new Subscription(this, observer);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly MaterialColorObservable _owner;
        private readonly IObserver<Color> _observer;
        private readonly MaterialHostState _hostState;
        private bool _isDisposed;

        public Subscription(MaterialColorObservable owner, IObserver<Color> observer)
        {
            _owner = owner;
            _observer = observer;
            _hostState = new MaterialHostState(_owner._context, _owner._application);
            _hostState.Changed += OnHostStateChanged;

            Publish();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _hostState.Changed -= OnHostStateChanged;
            _hostState.Dispose();
        }

        private void OnHostStateChanged(object? sender, EventArgs e)
        {
            Publish();
        }

        private void Publish()
        {
            if (_isDisposed)
                return;

            var scheme = _hostState.Scheme;
            var theme = _hostState.ThemeVariant;
            var color = scheme is null ? _owner._fallbackColor : _owner._resolveColor(scheme, theme);

            _observer.OnNext(color);
        }
    }
}