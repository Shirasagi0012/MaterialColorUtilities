# Image-color verification

Verified on 2026-10-03 against the synchronous-quantizer baseline `d8a64c1`.
Production changes are confined to the Avalonia adapter, its Gallery demonstration,
and documentation. Core quantization/scoring algorithms are unchanged.

## Automated coverage

The existing test application now uses real Skia drawing with the bundled Inter
font under Linux Headless; the old dummy renderer cannot validate bitmap extraction.
All AXAML fixtures are compiled at build time.

- Synchronous extractor: sample-size cross-product including `int.MaxValue` aspect
  ratios; actual Skia BGRA/RGBA Premul/Unpremul inputs; explicit source/destination
  stride and opaque metadata; transparent/semitransparent images; a 100000×1 real
  bitmap; caller ownership; disposed/null/off-thread/mutable/render-target failures
- Scoring/result: filtered grayscale, unfiltered recovery, no hidden core fallback,
  true candidate matching fallback, immutable defensive copies, nullable seed,
  explicit fallback and candidate bounds
- Shared source: init/dispatcher-turn coalescing; semaphore-controlled A→B→C;
  stale success/error suppression after real capture; clear and AutoUpdate disable;
  Refresh; failure and invalid-option recovery; stage-cache identity assertions;
  reentrant Result notification; weak lifetime before admission, after capture,
  in queued completion, and when releasing a containing view
- Binding/resource integration: compiled nested StaticResource constructor;
  native best-seed binding; rank 1→2→1→empty→recovery; per-consumer and per-source
  fallback; nullable scheme/custom seeds removing resources and then restoring them;
  invalid source/index/alpha rejected; same result with 1/10/100 consumers and theme
  switches does not trigger image work
- Gallery: shared source, real raster candidates, switch/clear, disposal,
  detach/reattach, theme reuse, and visual inspection of the rendered demo

Final local checks passed:

- Release solution build for core/Avalonia net8.0 + net10.0 and Gallery/tests net10.0
- Full test suite: **632 passed, 1 pre-existing skip, 0 failed**
- Release `dotnet pack`: both core and Avalonia `.nupkg` and `.snupkg` outputs
- Independent review of extraction, queue/lifetime, markup, and documentation
- Original README introduction/video preserved; no core algorithm changes

The only existing skipped test is
`HctRoundTripTests.HctPreservesOriginalColor`; it was already skipped in the baseline.
The opt-in performance tests are not timed during ordinary CI runs.

## Reproducible performance

See [benchmark instructions and complete CSV](../benchmarks/ImageColors/README.md).
Measurements used a shared Linux x64 container, .NET 10.0.12 / SDK 10.0.401,
Avalonia/Skia 12.1.3, Release, 3 warmups and 20 measured iterations per case.
They are observations on this environment, not portable performance promises.

At the default 16,384 samples / 128 colors:

| Input | UI capture median / p95, ms | Background quantize+score median / p95, ms | Stage managed allocation median, MiB |
| --- | ---: | ---: | ---: |
| NASA photo, 4096×4096 | 13.574 / 18.117 | 21.275 / 23.357 | 3.49 |
| Mostly transparent PNG, 3840×2160 | 7.667 / 8.490 | 9.456 / 12.817 | 2.83 |
| Noise, 3840×2160 | 8.507 / 9.806 | 13.424 / 14.834 | 2.60 |
| Wide color bands, 131072×8 | 4.330 / 5.965 | 1.306 / 2.261 | 1.11 |
| Grayscale, 3840×2160 | 7.635 / 9.441 | 0.806 / 1.445 | 1.12 |

These managed allocations include stage workspaces, not merely the 64 KiB sample.
Capture allocation for a 3840×2160 image at the default is 131,144 bytes before
transparent compaction; quantizer/score workspaces dominate many cases. Native
thumbnail and platform allocations are additional.

For the photograph at 128 colors, 4096 / 16384 / 65536 sample limits produced
capture medians 10.176 / 13.574 / 25.605 ms and background medians
13.110 / 21.275 / 51.710 ms. Managed totals were about 2.98 / 3.66 / 7.91 MB
(decimal). This supports an explicit low-cost 4096 setting where appropriate;
it does not establish perceptual equivalence or a universally optimal default.
The approved 16,384 / 128 / 4 defaults are retained as a bounded starting point,
with one background worker to avoid concurrent quantizer allocation bursts.
The benchmark is not a reason to raise the default to 65,536 or to promise a
frame budget. Applications should profile their own content and hardware.

### UI capture and native-memory boundary

The adapter's managed pixel loops and returned thumbnail are sample-bounded.
**The platform's scaling cost and native scratch space can still depend on the
original dimensions.** At 7680×4320, capture medians were 24.597–26.327 ms,
with p95 up to 33.446 ms. The Linux/glibc sampled allocator high-water delta was
about 42.27 MiB for these inputs, versus roughly 0.08 MiB for typical 4K cases.
Therefore the source is not an entirely nonblocking UI pipeline, and a pixel cap
is not a cap on total native memory. Large inputs should be supplied as loading-layer
thumbnails separate from the full display image.

The glibc numbers are a 1 ms sampled allocator subset, not an exact native-memory
peak; short transients and direct mappings/other allocators may be missed. Whole
process peak working set also includes decoded fixtures, .NET/test infrastructure,
and previous cases. Neither measurement isolates all extraction-only native bytes.
These limits are stated alongside the raw data rather than substituting RSS for
native allocation.

### Why medium-quality interpolation stays

A small independent resizing-only comparison (same sample cap, 3 warmups / 20 runs)
measured LowQuality versus the approved MediumQuality:

| Input | Low median / p95, ms | Medium median / p95, ms |
| --- | ---: | ---: |
| Photo 4096² → 128² | 0.454 / 0.563 | 11.453 / 15.221 |
| Noise 7680×4320 → 170×96 | 0.578 / 0.716 | 35.704 / 41.521 |

Low quality is substantially cheaper, but it changes which colors are sampled.
For the noise fixture, the low-quality candidates were saturated
`#5ee62b #b62be6 #b4561e #6796b6`; medium-quality candidates were near-neutral
`#7c817b #7b8081 #7c7a82 #817980`. The photographic candidates also differed.
The first version therefore retains medium interpolation rather than silently
changing the approved averaging/sampling behavior. Applications can explicitly
prepare a smaller immutable image with their chosen interpolation before passing
it to the extractor. This tradeoff is demonstrated, not presented as proof that
one interpolation is universally perceptually better. The separate run's figures
should not be subtracted from stage timings collected at another time.

[Complete comparison](../benchmarks/ImageColors/linux-x64-resize-comparison.csv)
includes candidates and sampled allocator figures.

### Source sharing versus resource notification

A shared source performs one extraction, but multiple scalar seed bindings still
use the existing provider's synchronous invalidation semantics. The separate
fanout diagnostic alternates an already-computed immutable Result with one scheme
seed plus 0/9/99 custom seeds in **one provider**, with one brush target per seed.
It includes resource resolution/brush updates and excludes extraction/decoding.
It reports the number of SchemeChanged/provider invalidations as well as median/p95
time and allocation. This is intentionally a stress shape, not a claim that merely
100 controls using one token incur 100 extractions.

Final isolated fanout run:

| Seed bindings | UI publication median / p95, ms | Managed bytes / publication | Provider invalidations |
| --- | ---: | ---: | ---: |
| 1 | 0.752 / 0.875 | 18,824 | 1 |
| 10 | 10.465 / 21.086 | 241,776 | 10 |
| 100 | 72.929 / 76.896 | 8,001,872 | 100 |

[Full fanout report](../benchmarks/ImageColors/linux-x64-publication-fanout.txt).
This exposes an existing synchronous resource-notification cost despite shared
extraction. Avoid very large numbers of independently changing custom seeds in
one provider on latency-sensitive paths. Provider notification semantics were not
redesigned in this image-adapter change; the cost is disclosed rather than hidden.

## Remaining compatibility limits

- Tested here: Linux x64 with Skia 12.1.3. Windows/macOS, mobile devices, and other
  backends have not been performance-certified by this run
- Avalonia's stable public API does not expose bitmap-backend identity. The adapter
  rejects custom subclasses and unsupported formats/scaling/copy contracts, and
  verifies the returned size. It does not use private reflection to classify every
  custom backend; Skia is the verified compatibility target
- Original decoding and file/URL loading remain caller responsibilities
- CI configuration runs on main/PR-to-main, not this feature-branch push. Local
  Release build, test, and package checks must not be represented as a feature-branch
  GitHub Actions run
