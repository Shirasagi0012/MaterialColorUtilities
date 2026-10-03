# Image extraction measurements

Opt-in diagnostics live in `MaterialColorUtilities.Tests/ImageColorPerformanceTests.cs`
and the image-source lifecycle tests. Ordinary test runs do not execute the long
measurements. They are diagnostic observations, not portable timing gates.

```sh
MCU_IMAGE_BENCHMARK=1 \
MCU_BENCHMARK_PHOTO=/absolute/path/to/photo.jpg \
MCU_IMAGE_BENCHMARK_OUTPUT=/tmp/image-stages.csv \
dotnet test MaterialColorUtilities.Tests/MaterialColorUtilities.Tests.csproj \
  -c Release --filter FullyQualifiedName~ImageColorPerformanceTests \
  --logger 'console;verbosity=detailed'
```

The optional photograph is supplied explicitly; the test never downloads files.
Without it, the photographic workload is omitted and that omission is reported.
Fixtures are created/decoded before stage measurements. The checked-in run used
NASA's [PIA18033 Earth composite](https://science.nasa.gov/photojournal/earth/),
[original JPEG](https://assets.science.nasa.gov/content/dam/science/psd/photojournal/pia/pia18/pia18033/PIA18033.jpg)
(8000×8000), resized once with Pillow Lanczos to 4096×4096 and JPEG quality 95.
The photographic file is not bundled. Credit: NASA.

## Method and scope

- Linux x64 container; reported Intel Xeon Platinum 8573C, 9 available logical CPUs;
  shared-host scheduling and frequency are not controlled
- .NET SDK 10.0.401/runtime 10.0.12, Release, Avalonia/Skia 12.1.3, headless actual
  Skia drawing (not Avalonia's dummy bitmap renderer)
- Five workloads: photograph, mostly transparent PNG with a translucent strip and
  opaque island, deterministic noise, 131072×8 color bands, and grayscale
- Matrix: 4096 / 16384 / 65536 sampled-pixel budgets × 64 / 128 / 256 colors
- Default-options size sweeps: 64², 512², 3840×2160, 7680×4320, smooth and noise
- Each of 53 cases: 3 warmups + 20 measured iterations. Median is the middle-pair
  average; p95 is nearest rank. No assertions on elapsed time
- Capture runs on the UI thread. Only independent managed samples and immutable
  options cross to worker quantization/scoring. All three stages use a monotonic
  stopwatch; managed allocations use `GC.GetAllocatedBytesForCurrentThread` on
  each executing thread, summed across stages. Scheduling/test overhead is excluded
- RSS/working-set snapshots and process-lifetime high-water include **both managed
  and native memory**, test runtime, original images, and prior cases. They are not
  per-case native peaks or extraction-only allocation counts
- Linux/glibc `mallinfo2` is sampled approximately every 1 ms during each case.
  `uordblks + hblkhd` provides an allocator subset, with baseline, observed maximum,
  and delta. This includes unrelated allocator activity, misses shorter transients,
  excludes direct runtime mappings/other allocators, and is **not an exact total
  native-memory peak**. Unsupported platforms emit -1 fields
- Original decoding, image creation, source/consumer notification, and resource
  rebuilding are outside the three extraction stage timings. Fanout is measured
  separately; neither benchmark promises a complete application's frame latency

The full data is in [linux-x64-stages.csv](linux-x64-stages.csv). See the
[verification report](../../docs/avalonia-image-colors-verification.md) for selected
results, interpretation, and important UI/native-memory boundaries.

## Interpolation and binding publication

Run separately from the extraction matrix and other CPU-heavy jobs:

```sh
MCU_IMAGE_RESIZE_COMPARE=1 MCU_BENCHMARK_PHOTO=/absolute/path/to/photo.jpg \
MCU_IMAGE_RESIZE_OUTPUT=/tmp/resize-comparison.csv \
dotnet test MaterialColorUtilities.Tests/MaterialColorUtilities.Tests.csproj \
  -c Release --filter FullyQualifiedName~OptInCompareResizeInterpolation \
  --logger 'console;verbosity=detailed'

MCU_IMAGE_BENCHMARK=1 MCU_IMAGE_FANOUT_REPORT=/tmp/image-fanout.txt \
dotnet test MaterialColorUtilities.Tests/MaterialColorUtilities.Tests.csproj \
  -c Release --filter FullyQualifiedName~ReportSharedResultPublicationFanout \
  --logger 'console;verbosity=detailed'
```

The resize comparison only times native thumbnail creation/disposal; extraction of
candidate colors from those thumbnails is outside that timed section. Production
continues to use MediumQuality. The fanout file appends one result per consumer-count
case; start with a new report path for each run. It uses one provider with one scheme
seed and N−1 custom seeds, not N independent extraction jobs. Neither experiment
changes production scheduling or provider notification semantics.
