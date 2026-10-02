# Quantization comparison

This dependency-free Release console harness measures serial, end-to-end caller
latency and process-wide allocated bytes for Map, Wu, and Celebi. It also emits
the ordered-output fingerprints used by the regression tests.

## Reproduce

From the repository root, with .NET 10 SDK installed (the core also builds for
.NET 8):

```sh
# Use an absolute path for the baseline checkout override.
git worktree add --detach /tmp/mcu-async-baseline e254817f1a807276c1729fd9c2ee81d642404a3c

dotnet build benchmarks/Quantization -c Release -m:1 \
  -p:QuantizationProject=/tmp/mcu-async-baseline/MaterialColorUtilities/MaterialColorUtilities.csproj \
  -p:DefineConstants=LEGACY -o /tmp/mcu-quantization-legacy

dotnet build benchmarks/Quantization -c Release -m:1 -o /tmp/mcu-quantization-sync

DOTNET_TieredCompilation=0 dotnet /tmp/mcu-quantization-legacy/Quantization.dll
DOTNET_TieredCompilation=0 dotnet /tmp/mcu-quantization-sync/Quantization.dll

# Compare identical fixtures, populations, order and optional mappings.
dotnet /tmp/mcu-quantization-legacy/Quantization.dll --snapshots > /tmp/legacy.txt
dotnet /tmp/mcu-quantization-sync/Quantization.dll --snapshots > /tmp/sync.txt
diff -u /tmp/legacy.txt /tmp/sync.txt

# Inspect preserved invalid-budget behavior separately.
dotnet /tmp/mcu-quantization-legacy/Quantization.dll --invalid-budgets
dotnet /tmp/mcu-quantization-sync/Quantization.dll --invalid-budgets
```

The `LEGACY` compile branch exists only in this comparison program to call the
old API. There is no async compatibility wrapper in the library. Each variant
must be built into a separate output directory; do not copy one variant's core
DLL over the other.

## Method

- Same Linux x64 environment, SDK 10.0.401 / runtime 10.0.12, Intel Xeon Platinum
  8370C @ 2.80 GHz, 9 logical processors visible to the runtime
- Release builds, tiered compilation disabled for both timed processes
- Inputs built before timing: repeating five-color palette (4,096 pixels),
  synthetic 64×64 RGB gradient, fixed LCG noise (65,536 opaque pixels)
- `maxColors = 16`, mapping disabled, fresh quantizer instance per operation
- 20 warm-up calls, then 7 batches of 20 sequential calls per dataset/algorithm;
  full collections before each batch, outside the timed interval
- Legacy uses `GetAwaiter().GetResult()` from a context-free console caller. This
  intentionally includes its thread-pool scheduling and wait cost. It does not
  measure parallel throughput or a UI dispatcher workload
- `GC.GetTotalAllocatedBytes(true)` includes legacy worker-thread allocations;
  process-wide runtime bookkeeping introduces small allocation noise
- Three separate process pairs, in legacy→sync, sync→legacy, legacy→sync order;
  no concurrent build/test workloads during measurements

## Measurements (2026-10-02)

Values below are the median of three per-process medians. Milliseconds and
allocated bytes are per complete call. [All process summaries](measurements.csv)
include each run's batch minimum/maximum to expose noise.

| Input | Quantizer | Legacy ms | Sync ms | Legacy bytes | Sync bytes |
|---|---|---:|---:|---:|---:|
| palette-4096 | Map | 0.1578 | 0.0677 | 951 | 609 |
| palette-4096 | Wu | 0.8289 | 0.7080 | 896,187 | 895,661 |
| palette-4096 | Celebi | 0.9394 | 0.7610 | 900,307 | 899,669 |
| gradient-4096 | Map | 0.1798 | 0.1288 | 323,199 | 322,857 |
| gradient-4096 | Wu | 0.8813 | 0.7336 | 1,220,392 | 1,219,865 |
| gradient-4096 | Celebi | 3.2577 | 3.0109 | 1,823,530 | 1,823,146 |
| noise-65536 | Map | 3.2117 | 3.0891 | 2,909,806 | 2,909,277 |
| noise-65536 | Wu | 5.9484 | 5.7186 | 3,806,236 | 3,805,508 |
| noise-65536 | Celebi | 47.8657 | 47.7445 | 10,927,456 | 10,926,586 |

Removing scheduling generally helps the small serial calls here and eliminates
several hundred bytes of async bookkeeping per call. Algorithm allocations still
dominate Wu and Celebi. Large-image timing is noisy: this is not evidence of a
universal throughput improvement, nor a statistically controlled benchmark.
The change primarily lets the caller own scheduling. It does not optimize the
quantization algorithms, decoding, or sample selection.

The first 65,536-pixel Celebi run was 48.81 ms legacy versus 66.62 ms synchronous;
the reversed-order round was 47.87 ms versus 47.74 ms. Both are retained in the
raw summaries rather than selecting only favorable results. Benchmark your real
image pipeline and sampling policy before choosing UI latency limits.
