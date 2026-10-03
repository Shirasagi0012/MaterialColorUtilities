using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Avalonia;

/// <summary>
/// A shared, reactive image palette. Captures bounded pixels on the UI thread and runs synchronous
/// quantization on a bounded background queue. The caller owns and disposes <see cref="Image"/>.
/// </summary>
/// <remarks>
/// This resource has no control lifetime or inherited DataContext. Use an explicit binding source.
/// Pending work holds only a weak reference to this object. Changes coalesce, and obsolete work
/// cannot overwrite newer results. Bind several consumers to one instance to share extraction.
/// </remarks>
public sealed class ImageColorSource : AvaloniaObject, ISupportInitialize
{
    public static readonly StyledProperty<Bitmap?> ImageProperty =
        AvaloniaProperty.Register<ImageColorSource, Bitmap?>(nameof(Image));
    public static readonly StyledProperty<int> MaxSamplePixelsProperty =
        AvaloniaProperty.Register<ImageColorSource, int>(nameof(MaxSamplePixels), 16_384,
            validate: value => value is >= 1 and <= 65_536);
    public static readonly StyledProperty<int> MaxColorsProperty =
        AvaloniaProperty.Register<ImageColorSource, int>(nameof(MaxColors), 128,
            validate: value => value is >= 1 and <= 256);
    public static readonly StyledProperty<int> DesiredColorsProperty =
        AvaloniaProperty.Register<ImageColorSource, int>(nameof(DesiredColors), 4,
            validate: value => value is >= 1 and <= 16);
    public static readonly StyledProperty<bool> FilterProperty =
        AvaloniaProperty.Register<ImageColorSource, bool>(nameof(Filter), true);
    public static readonly StyledProperty<Color?> FallbackColorProperty =
        AvaloniaProperty.Register<ImageColorSource, Color?>(nameof(FallbackColor),
            validate: value => value is null || value.Value.A == 255);
    public static readonly StyledProperty<bool> AutoUpdateProperty =
        AvaloniaProperty.Register<ImageColorSource, bool>(nameof(AutoUpdate), true);
    public static readonly DirectProperty<ImageColorSource, ImageColorResult> ResultProperty =
        AvaloniaProperty.RegisterDirect<ImageColorSource, ImageColorResult>(nameof(Result), source => source.Result);
    public static readonly DirectProperty<ImageColorSource, bool> IsBusyProperty =
        AvaloniaProperty.RegisterDirect<ImageColorSource, bool>(nameof(IsBusy), source => source.IsBusy);
    public static readonly DirectProperty<ImageColorSource, Exception?> ErrorProperty =
        AvaloniaProperty.RegisterDirect<ImageColorSource, Exception?>(nameof(Error), source => source.Error);

    // All source state is confined to the UI thread. Workers receive only independent managed data.
    private static readonly SemaphoreSlim WorkerGate = new(1, 1);
    private ImageColorResult _result = ImageColorResult.CreateEmpty(null);
    private bool _isBusy;
    private Exception? _error;
    private long _generation;
    private bool _pending;
    private bool _running;
    private bool _posted;
    private int _initializationDepth;
    private ArgbColor[]? _samples;
    private IReadOnlyDictionary<ArgbColor, int>? _histogram;
    private ImageColorResult? _scoredResult;

    public Bitmap? Image { get => GetValue(ImageProperty); set => SetValue(ImageProperty, value); }
    public int MaxSamplePixels { get => GetValue(MaxSamplePixelsProperty); set => SetValue(MaxSamplePixelsProperty, value); }
    public int MaxColors { get => GetValue(MaxColorsProperty); set => SetValue(MaxColorsProperty, value); }
    public int DesiredColors { get => GetValue(DesiredColorsProperty); set => SetValue(DesiredColorsProperty, value); }
    public bool Filter { get => GetValue(FilterProperty); set => SetValue(FilterProperty, value); }
    public Color? FallbackColor { get => GetValue(FallbackColorProperty); set => SetValue(FallbackColorProperty, value); }
    public bool AutoUpdate { get => GetValue(AutoUpdateProperty); set => SetValue(AutoUpdateProperty, value); }
    public ImageColorResult Result { get => _result; private set => SetAndRaise(ResultProperty, ref _result, value); }
    public bool IsBusy { get => _isBusy; private set => SetAndRaise(IsBusyProperty, ref _isBusy, value); }
    public Exception? Error { get => _error; private set => SetAndRaise(ErrorProperty, ref _error, value); }

    /// <summary>Invalidates all cached stages and requests one extraction, even when AutoUpdate is false.</summary>
    public void Refresh()
    {
        VerifyAccess();
        ClearCache();
        Request(update: true);
    }

    public void BeginInit() { VerifyAccess(); _initializationDepth++; }
    public void EndInit()
    {
        VerifyAccess();
        if (_initializationDepth == 0)
            throw new InvalidOperationException("EndInit requires a matching BeginInit.");
        if (--_initializationDepth == 0)
            Schedule();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ImageProperty || change.Property == MaxSamplePixelsProperty)
            ClearCache();
        else if (change.Property == MaxColorsProperty)
        {
            _histogram = null;
            _scoredResult = null;
        }
        else if (change.Property == DesiredColorsProperty || change.Property == FilterProperty)
            _scoredResult = null;
        else if (change.Property != FallbackColorProperty && change.Property != AutoUpdateProperty)
            return;

        Request(AutoUpdate);
    }

    private void ClearCache()
    {
        _samples = null;
        _histogram = null;
        _scoredResult = null;
    }

    private void Request(bool update)
    {
        _generation++;
        _pending = update && Image is not null;
        if (Image is null)
        {
            ClearCache();
            Result = ImageColorResult.CreateEmpty(FallbackColor);
            Error = null;
        }
        // Before the first success, only an explicitly supplied fallback may create a seed.
        else if (_samples is null && Result.Candidates.Count == 0 && Error is null)
            Result = Result.WithFallback(FallbackColor);
        IsBusy = _pending;
        Schedule();
    }

    private void Schedule()
    {
        if (!_pending || _running || _posted || _initializationDepth > 0)
            return;
        _posted = true;
        var weak = new WeakReference<ImageColorSource>(this);
        Dispatcher.UIThread.Post(() => Start(weak), DispatcherPriority.Background);
    }

    private static void Start(WeakReference<ImageColorSource> weak)
    {
        if (!weak.TryGetTarget(out var source))
            return;
        source._posted = false;
        if (!source._pending || source._running || source._initializationDepth > 0)
            return;
        source._running = true;
        source._pending = false;
        var generation = source._generation;
        // Do not capture source (or its Bitmap) in the worker closure.
        _ = Task.Run(() => Execute(weak, generation));
    }

    private static async Task Execute(WeakReference<ImageColorSource> weak, long generation)
    {
        Work? work = null;
        ImageColorResult? result = null;
        Exception? error = null;
        try
        {
            await WorkerGate.WaitAsync().ConfigureAwait(false);
            try
            {
                // Generation is checked after admission and before any bitmap access or allocation.
                work = await Dispatcher.UIThread.InvokeAsync(() => Capture(weak, generation));
                if (work is not null)
                {
                    work.Histogram ??= ImageColorExtractor.Quantize(work.Samples, work.Options.MaxColors);
                    result = work.ScoredResult?.WithFallback(work.Options.FallbackColor)
                        ?? ImageColorExtractor.Score(work.Histogram, work.Options);
                }
            }
            finally { WorkerGate.Release(); }
        }
        catch (Exception exception) { error = exception; }

        // The posted callback also holds only a weak source reference; closing a view can collect it.
        Dispatcher.UIThread.Post(() => Complete(weak, generation, work, result, error));
    }

    private static Work? Capture(WeakReference<ImageColorSource> weak, long generation)
    {
        if (!weak.TryGetTarget(out var source) || source._generation != generation || source.Image is not { } image)
            return null;
        var options = new ImageColorOptions
        {
            MaxSamplePixels = source.MaxSamplePixels,
            MaxColors = source.MaxColors,
            DesiredColors = source.DesiredColors,
            Filter = source.Filter,
            FallbackColor = source.FallbackColor
        };
        options.Validate();
        var samples = source._samples ?? ImageColorExtractor.Capture(image, options);
        return new Work(samples, source._histogram, source._scoredResult, options);
    }

    private static void Complete(WeakReference<ImageColorSource> weak, long generation, Work? work,
        ImageColorResult? result, Exception? error)
    {
        if (!weak.TryGetTarget(out var source))
            return;
        source._running = false;
        if (source._generation == generation)
        {
            if (error is not null)
                source.Error = error; // Keep the previous result; failure is not fallback success.
            else if (work is not null && result is not null)
            {
                source._samples = work.Samples;
                source._histogram = work.Histogram;
                source._scoredResult = result;
                source.Result = result;
                if (source._generation == generation)
                    source.Error = null;
            }
            if (source._generation == generation)
                source.IsBusy = false;
        }
        source.Schedule();
    }

    private sealed class Work(ArgbColor[] samples, IReadOnlyDictionary<ArgbColor, int>? histogram,
        ImageColorResult? scoredResult, ImageColorOptions options)
    {
        public ArgbColor[] Samples { get; } = samples;
        public IReadOnlyDictionary<ArgbColor, int>? Histogram { get; set; } = histogram;
        public ImageColorResult? ScoredResult { get; } = scoredResult;
        public ImageColorOptions Options { get; } = options;
    }
}
