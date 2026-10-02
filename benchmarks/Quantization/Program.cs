using System.Diagnostics;
using MaterialColorUtilities.Quantize;
using MaterialColorUtilities.Tests.Quantize;
using MaterialColorUtilities.Utils;

static QuantizerResult Run(string name, List<ArgbColor> pixels, int maxColors, bool mapping = false)
{
#if LEGACY
    return name switch
    {
        "Map" => new QuantizerMap().QuantizeAsync(pixels, maxColors).GetAwaiter().GetResult(),
        "Wu" => new QuantizerWu().QuantizeAsync(pixels, maxColors).GetAwaiter().GetResult(),
        _ => new QuantizerCelebi().QuantizeAsync(pixels, maxColors, mapping).GetAwaiter().GetResult()
    };
#else
    return name switch
    {
        "Map" => new QuantizerMap().Quantize(pixels, maxColors),
        "Wu" => new QuantizerWu().Quantize(pixels, maxColors),
        _ => new QuantizerCelebi().Quantize(pixels, maxColors, mapping)
    };
#endif
}

if (args.Contains("--invalid-budgets"))
{
    foreach (var algorithm in new[] { "Map", "Wu", "Celebi" })
        foreach (var budget in new[] { 0, -1 })
        {
            try
            {
                Console.WriteLine($"{algorithm},{budget},{Run(algorithm, [new(0xffff0000)], budget).ColorToCount.Count}");
            }
            catch (Exception error)
            {
                Console.WriteLine($"{algorithm},{budget},{error.GetType().Name}");
            }
        }
    return;
}

if (args.Contains("--snapshots"))
{
    foreach (var (name, pixels, maxColors) in QuantizationRegressionData.Cases())
        foreach (var algorithm in new[] { "Map", "Wu", "Celebi", "CelebiMapping" })
        {
            var result = Run(algorithm, pixels, maxColors, algorithm == "CelebiMapping");
            Console.WriteLine($"{name},{algorithm},{QuantizationRegressionData.Fingerprint(result)}");
        }
    return;
}

// Serial end-to-end calls. Allocation includes all threads, including legacy Task.Run.
// Keep this process otherwise idle; do not compare with thread-local allocation counters.
Console.WriteLine($"runtime={Environment.Version}; os={Environment.OSVersion}; processors={Environment.ProcessorCount}");
Console.WriteLine("dataset,algorithm,pixels,maxColors,iterations,median_ms,min_ms,max_ms,median_bytes");
foreach (var (name, pixels) in new[]
{
    ("palette-4096", QuantizationRegressionData.Palette(4096)),
    ("gradient-4096", QuantizationRegressionData.Gradient(64)),
    ("noise-65536", QuantizationRegressionData.Noise(65536))
})
foreach (var algorithm in new[] { "Map", "Wu", "Celebi" })
{
    const int iterations = 20;
    const int batches = 7;
    for (var i = 0; i < 20; i++) Run(algorithm, pixels, 16);
    var times = new List<double>();
    var allocations = new List<double>();
    for (var batch = 0; batch < batches; batch++)
    {
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var before = GC.GetTotalAllocatedBytes(true);
        var start = Stopwatch.GetTimestamp();
        for (var i = 0; i < iterations; i++) GC.KeepAlive(Run(algorithm, pixels, 16));
        times.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds / iterations);
        allocations.Add((GC.GetTotalAllocatedBytes(true) - before) / (double)iterations);
    }
    times.Sort(); allocations.Sort();
    Console.WriteLine(FormattableString.Invariant($"{name},{algorithm},{pixels.Count},16,{iterations},{times[batches / 2]:F4},{times[0]:F4},{times[^1]:F4},{allocations[batches / 2]:F0}"));
}
