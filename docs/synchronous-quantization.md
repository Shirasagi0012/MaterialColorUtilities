# Synchronous core quantization

`IQuantizer`, `QuantizerMap`, `QuantizerWu`, and `QuantizerCelebi` expose
`QuantizerResult Quantize(List<ArgbColor> pixels, int maxColors)`.
The concrete Wu method retains the parameter name `colorCount` for named arguments.
Celebi also retains the overload with `bool returnInputPixelToClusterPixel`.
`QuantizerWsmeans.Quantize` and `Score.CalculateScore` were already synchronous.

This is a breaking API change: replace calls to `await quantizer.QuantizeAsync(...)`
with `quantizer.Quantize(...)`. Custom `IQuantizer` implementations must update their
method signature too. No asynchronous compatibility wrapper or cancellation API is
provided. Exceptions now propagate directly at the call site instead of through a task.

## Select an image seed

```csharp
using MaterialColorUtilities.Quantize;
using MaterialColorUtilities.Score;
using MaterialColorUtilities.Utils;

// Decode, sample and apply your transparency policy before calling the core.
List<ArgbColor> pixels = GetDecodedPixels();
var result = new QuantizerCelebi().Quantize(pixels, maxColors: 128);
var seeds = Score.CalculateScore(
    result.ColorToCount.ToDictionary(pair => pair.Key, pair => pair.Value),
    desired: 4);
ArgbColor seed = seeds[0];
```

The core does CPU work on its caller's thread. A UI/image adapter should schedule
its complete decode/sample/quantize/score pipeline off the UI thread and marshal
only the final result back. The core neither dispatches nor captures a
`SynchronizationContext`. Choose the sample size and color budget explicitly;
a synchronous API does not make large image processing inexpensive.

Do not mutate the input while quantizing. A `QuantizerWu` instance holds mutable
histogram state: sequential reuse is supported, concurrent reuse is not. Use a
separate instance per concurrent operation. Celebi creates a Wu instance per call.

## Deliberately unchanged behavior

This refactor changes execution ownership, not the color algorithms:

- Map counts opaque pixels in insertion order and ignores the color budget
- Wu uses opaque input and its representative counts remain zero
- Celebi passes the original input to Wsmeans, including non-opaque pixels
- Mapping generation remains opt-in; ordering, populations, rounding and
  deterministic clustering are unchanged, including existing alpha-mixed quirks
- Input validation and invalid-budget behavior have not been redesigned

Image decoding, downsampling, transparency normalization, and an Avalonia
image-extraction API remain separate work. The regression suite records baseline
outputs rather than silently introducing such changes here.

## Verification and performance

48 ordered-output fingerprints were captured from commit `e254817` using the
same fixtures before the refactor, covering all three quantizers and Celebi's
mapping overload. Tests also cover input preservation, synchronous interface use,
opaque Map counts, sequential Wu reuse and a synchronization context that rejects
continuation posting.

See [the reproducible benchmark and measurements](../benchmarks/Quantization/README.md).
