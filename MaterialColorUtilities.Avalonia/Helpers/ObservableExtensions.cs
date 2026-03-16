namespace MaterialColorUtilities.Avalonia.Helpers;

internal static class ObservableExtensions
{
    public static IObservable<TResult> Select<TSource, TResult>(
        this IObservable<TSource> source,
        Func<TSource, TResult> selector
    )
    {
        return new SelectObservable<TSource, TResult>(source, selector);
    }

    private sealed class SelectObservable<TSource, TResult>(
        IObservable<TSource> source,
        Func<TSource, TResult> selector
    ) : IObservable<TResult>
    {
        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            return source.Subscribe(new AnonymousObserver<TSource>(value => { observer.OnNext(selector(value)); }));
        }
    }

    private sealed class AnonymousObserver<T>(Action<T> onNext) : IObserver<T>
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

internal sealed class Observer<T>(Action<T> onNext) : IObserver<T>
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