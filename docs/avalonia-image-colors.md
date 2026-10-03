# Avalonia image colors

The Avalonia package exposes five small types: `ImageColorExtractor`,
`ImageColorOptions`, `ImageColorResult`, `ImageColorSource`, and the
`ImageSeedExtension` markup extension. Core quantization/scoring algorithms and
existing scheme, custom-color, and token APIs are unchanged.

## Synchronous service

```csharp
using MaterialColorUtilities.Avalonia;

// On the Avalonia UI thread, while the caller-owned bitmap is still alive:
ImageColorResult result = ImageColorExtractor.Extract(bitmap, new ImageColorOptions
{
    MaxSamplePixels = 16_384,
    MaxColors = 128,
    DesiredColors = 4,
    Filter = true,
    FallbackColor = null
});
// result.SeedColor is Avalonia.Media.Color?; result.Candidates is read-only.
```

`Extract` returns only after capture, Celebi quantization, and Material Score have
completed. It is a real synchronous operation; there is no `ExtractAsync`,
`QuantizeAsync`, or cancellation-token wrapper. A direct large extraction can block
the UI thread. Choose the reactive source below when UI responsiveness is needed.

Options are immutable records. Limits are 1..65,536 sample pixels, 1..256 quantized
colors, and 1..16 desired colors (also no greater than `MaxColors`). Invalid options
throw; a nonopaque fallback is rejected. Defaults are 16,384 / 128 / 4 with filtering
on. These defaults are a quality/cost starting point, not a frame-budget guarantee.

### Result contract

- `Candidates` contains only real image candidates in Material Score's selection
  order, **not population order**; it can be shorter than `DesiredColors`
- With candidates, `SeedColor == Candidates[0]` and `IsFallback == false`
- With none, `SeedColor` is null unless `FallbackColor` was explicitly set;
  `IsFallback` is true only when that configured fallback is actually used
- All result objects are immutable; candidates are defensively copied and wrapped
  read-only. A real candidate matching the fallback value is still a real candidate
- Empty/fully transparent input or filtering all colors (for example many grayscale
  images) is a successful empty result. Disable `Filter` to keep low-chroma candidates
- The core Score default blue is never exposed by this adapter. An internal
  transparent sentinel distinguishes Score's no-candidate branch and is removed
  before producing a result. Core Score's own behavior remains compatible

## XAML and shared source

See the complete resource composition in the [README](../README.md#use-an-image-as-a-seed)
and [Gallery demo](../MaterialColorUtilities.Gallery/Views/ImageSeedDemoView.axaml).
Declare a source in an earlier merged dictionary/outer scope than its consumers.
One resource instance can feed any number of schemes or custom colors:

```xml
Color="{ReflectionBinding Result.SeedColor, Source={StaticResource CoverColors}}"
Color="{mcu:ImageSeed {StaticResource CoverColors}, Index=1, FallbackColor=#006C4C}"
```

The first binding selects the best nullable seed; the second safely selects rank 1
(zero-based). `ImageSeed` binds **the whole Result** through a OneWay native
`ReflectionBinding` and a small immutable converter. `ProvideValue` does not decode,
extract, schedule, or cache images. Its object constructor validates the source at
load time, allowing XamlX's object-typed nested `StaticResource` constructor argument.
Negative indices and nonopaque fallback colors are rejected at load time.

When a rank is absent, selection uses the extension's fallback, then the result's
nullable seed. Do not replace this with `Result.Candidates[1]` plus `FallbackValue`:
Avalonia's index-binding behavior when lists shrink is different. For a custom color,
`Harmonize=True` still harmonizes toward its owning scheme; use `False` to retain
candidate color semantics.

The resource is an `AvaloniaObject`, not a control. It does not inherit a consumer's
DataContext or gain attach/detach callbacks. Use an explicit source when binding its
`Image`, for example `Image="{ReflectionBinding CoverBitmap, Source={StaticResource GalleryState}}"`.
Replace `source.Image`, not the whole StaticResource entry, to update existing bindings.

### Scheduling, cache, and lifecycle

Input/options changes in a dispatcher turn coalesce. `ISupportInitialize` also
defers work until XAML initialization ends. Each source has at most one admitted or
waiting job and one latest pending request; a shared internal gate admits one worker
at a time. Waiting work checks generation **after admission, before capture**;
publication checks it again. A→B→C therefore cannot publish obsolete A or B results.

UI capture creates and disposes a bounded native thumbnail and copies independent
managed samples. Background work receives only those samples, a histogram, and an
immutable options snapshot; it does not read Avalonia objects, keep original bitmap
locks, or draw. Each quantization owns its quantizer. Running synchronous work cannot
be interrupted, but stale completion is discarded. The static queue and completion
callbacks hold weak source references, so queued work cannot root a discarded view.

Each source retains at most its current small samples, histogram, and immutable
result. There is no global URI/hash cache or per-consumer quantizer:

| Change | Work required |
| --- | --- |
| Image or MaxSamplePixels | Capture, quantize, score |
| MaxColors | Reuse samples; quantize and score |
| DesiredColors or Filter | Reuse histogram; score |
| FallbackColor | Reuse candidates; update only fallback when needed |
| Theme or another binding to the same source | No image work |
| Refresh() | Invalidate every cached stage and request one extraction |

`AutoUpdate=False` invalidates automatic work and stops later automatic requests.
`Refresh()` explicitly requests one extraction even then; it is not a synchronous UI
mode. Inputs changed again invalidate that request. Cross-option validation occurs
on the coalesced snapshot, so setting `MaxColors` and `DesiredColors` together need
not validate a transient intermediate combination. Invalid combinations become
`Error` while preserving the previous result.

`Result` is always nonnull. While busy, the last successful result stays visible.
Failures preserve that result and set `Error` (an exception); they are not reported
as fallback success. The next success clears `Error`. `Image=null` immediately
invalidates pending work, clears caches/error, and publishes an empty result (or the
explicit source fallback). Without a seed, existing `MaterialColorResources` supplies
no scheme resources and null custom colors are skipped. Configure explicit fallback
colors in applications that require uninterrupted colors.

## Supported pixels and ownership

This version verifies ordinary static raster `Bitmap` on Avalonia 12.1.3's Skia
backend. `WriteableBitmap`, `RenderTargetBitmap`, and custom Bitmap subclasses are
explicitly rejected. Other backend implementations are not a supported compatibility
promise: they must honor bounded `CreateScaledBitmap`, readable BGRA8888/RGBA8888
formats, and the non-rendering `CopyPixels(PixelRect, ...)` path or extraction fails.
The adapter does not use reflection/private backend fields or silently render an
unreadable bitmap at its original dimensions. Supply an explicitly prepared,
immutable raster snapshot when working with other image types.

Capture never enlarges either dimension. It preserves aspect ratio to integer-pixel
precision with a minimum 1×1 dimension, then reduces the longer axis if needed for
extreme aspect ratios. Checked dimensions guarantee final width × height does not
exceed the sample limit. Medium-quality scaling happens before alpha filtering.
Actual source stride is handled by Avalonia's non-rendering copy into an explicit,
bounded destination stride; channel order is read explicitly, not reinterpreted as
machine-endian ARGB. BGRA/RGBA and Opaque/Premul/Unpremul metadata are handled.

Only alpha 255 contributes. With Opaque metadata the alpha byte is unused; otherwise
all partially transparent pixels are skipped. No unpremultiplication is needed for
alpha 255, where premultiplied RGB is unchanged. There is no implicit background,
alpha threshold, theme-dependent composite, or alpha weighting. For colors as displayed
on white/black, the application should explicitly composite a bounded static snapshot.

The caller owns the original bitmap and keeps it alive through UI capture. The
library never disposes it. Replace/clear the source on the UI thread before disposing
an old image. The Gallery follows this sequence and reloads its owned image on
reattach; `ImageColorSource` itself does not invent a control lifetime.

**The sample budget is not a total-memory budget.** The original bitmap is already
fully decoded by the caller. A 16,384-pixel 32-bit sample is 64 KiB, but the thumbnail,
copy buffers, platform resize workspaces, list/dictionary workspaces, Wu moments, and
Wsmeans allocate additional memory. In particular, the public Skia scaling path can
allocate image-size-dependent native scratch space even though the returned bitmap
and every managed pixel loop are sample-bounded. The measured 8K inputs added about
42 MiB to the sampled native allocator high-water and took roughly 25–26 ms in UI
capture; sample size alone does not provide a UI frame-time guarantee. Even `DecodeToWidth/Height` is not a hard codec-memory limit. For very large
files, prepare a loading-layer thumbnail separately from the full display image.

## Verification and performance

See [verification and measured results](avalonia-image-colors-verification.md).
Tests use real Skia rendering under Linux Headless rather than its dummy bitmap
implementation; AXAML fixtures are compiled at build time. Timing measurements are
diagnostic and do not impose machine-dependent pass/fail thresholds.
