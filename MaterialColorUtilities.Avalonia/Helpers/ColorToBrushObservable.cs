using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace MaterialColorUtilities.Avalonia.Helpers;

internal class ColorToBrushObservable(IObservable<Color> source) : IObservable<object>
{
    public IDisposable Subscribe(IObserver<object> observer)
    {
        return source.Subscribe(new ColorToBrushObserver(observer));
    }

    private class ColorToBrushObserver(IObserver<object> observer) : IObserver<Color>
    {
        public void OnCompleted() => observer.OnCompleted();

        public void OnError(Exception error) => observer.OnError(error);

        public void OnNext(Color value)
        {
            var brush = new ImmutableSolidColorBrush(value);
            observer.OnNext(brush);
        }
    }
}