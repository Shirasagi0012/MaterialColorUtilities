// Copyright 2021 Google LLC
//
//  This file is part of the material-color-utilities C# port by @Shirasagi0012
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using MaterialColorUtilities.Utils;

namespace MaterialColorUtilities.Quantize;

/// <summary>
/// Weighted sum k-means quantizer. Implements a k-means clustering algorithm that accounts
/// for pixel frequency.
/// </summary>
public sealed class QuantizerWsmeans
{
    private const bool Debug = false;

    private static void DebugLog(string log)
    {
        if (Debug)
            Console.WriteLine(log);
    }

    public static QuantizerResult Quantize(
        IEnumerable<ArgbColor> inputPixels,
        int maxColors,
        List<ArgbColor>? startingClusters = null,
        IPointProvider? pointProvider = null,
        int maxIterations = 5,
        bool returnInputPixelToClusterPixel = false
    )
    {
        startingClusters ??= [];
        pointProvider ??= new PointProviderLab();

        var pixelToCount = new Dictionary<ArgbColor, int>();
        var points = new List<Vector3D>();
        var pixels = new List<ArgbColor>();
        var pointCount = 0;

        foreach (var inputPixel in inputPixels)
            if (pixelToCount.TryGetValue(inputPixel, out var count))
            {
                pixelToCount[inputPixel] = count + 1;
            }
            else
            {
                pixelToCount[inputPixel] = 1;
                pointCount++;
                points.Add(pointProvider.FromArgb(inputPixel));
                pixels.Add(inputPixel);
            }

        var counts = new int[pointCount];
        for (var i = 0; i < pointCount; i++)
        {
            var pixel = pixels[i];
            var count = pixelToCount[pixel];
            counts[i] = count;
        }

        var clusterCount = Math.Min(maxColors, pointCount);

        var clusters = startingClusters.Select(e => pointProvider.FromArgb(e)).ToList();

        var additionalClustersNeeded = clusterCount - clusters.Count;
        if (additionalClustersNeeded > 0)
        {
            var random = new Random(0x42688);
            var indices = new HashSet<int>();

            for (var i = 0; i < additionalClustersNeeded; i++)
            {
                // Use existing points rather than generating random centroids.
                //
                // KMeans is extremely sensitive to initial clusters. This quantizer
                // is meant to be used with a Wu quantizer that provides initial
                // centroids, but Wu is very slow on unscaled images and when extracting
                // more than 256 colors.
                //
                // Here, we can safely assume that more than 256 colors were requested
                // for extraction. Generating random centroids tends to lead to many
                // "empty" centroids, as the random centroids are nowhere near any pixels
                // in the image, and the centroids from Wu are very refined and close
                // to pixels in the image.
                //
                // Rather than generate random centroids, we'll pick centroids that
                // are actual pixels in the image, and avoid duplicating centroids.

                var index = random.Next(points.Count);
                while (indices.Contains(index))
                    index = random.Next(points.Count);
                indices.Add(index);
            }

            clusters.AddRange(indices.Select(index => points[index]));
        }

        DebugLog($"have {clusters.Count} starting clusters, {points.Count} points");

        var clusterIndices = new int[pointCount];
        for (var i = 0; i < pointCount; i++)
            clusterIndices[i] = i % clusterCount;

        var indexMatrix = new int[clusterCount][];
        for (var i = 0; i < clusterCount; i++)
            indexMatrix[i] = new int[clusterCount];

        var distanceToIndexMatrix = new DistanceAndIndex[clusterCount][];
        for (var i = 0; i < clusterCount; i++)
        {
            distanceToIndexMatrix[i] = new DistanceAndIndex[clusterCount];
            for (var j = 0; j < clusterCount; j++)
                distanceToIndexMatrix[i][j] = new DistanceAndIndex(0, j);
        }

        var pixelCountSums = new int[clusterCount];

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            if (Debug)
            {
                Array.Clear(pixelCountSums, 0, clusterCount);
                for (var i = 0; i < pointCount; i++)
                {
                    var clusterIndex = clusterIndices[i];
                    var count = counts[i];
                    pixelCountSums[clusterIndex] += count;
                }

                var emptyClusters = 0;
                for (var cluster = 0; cluster < clusterCount; cluster++)
                    if (pixelCountSums[cluster] == 0)
                        emptyClusters++;

                DebugLog(
                    $"starting iteration {iteration + 1}; {emptyClusters} clusters are empty of {clusterCount}"
                );
            }

            var pointsMoved = 0;
            for (var i = 0; i < clusterCount; i++)
            {
                for (var j = i + 1; j < clusterCount; j++)
                {
                    var distance = pointProvider.Distance(clusters[i], clusters[j]);
                    distanceToIndexMatrix[j][i].Distance = distance;
                    distanceToIndexMatrix[j][i].Index = i;
                    distanceToIndexMatrix[i][j].Distance = distance;
                    distanceToIndexMatrix[i][j].Index = j;
                }

                Array.Sort(distanceToIndexMatrix[i]);
                for (var j = 0; j < clusterCount; j++)
                    indexMatrix[i][j] = distanceToIndexMatrix[i][j].Index;
            }

            for (var i = 0; i < pointCount; i++)
            {
                var point = points[i];
                var previousClusterIndex = clusterIndices[i];
                var previousCluster = clusters[previousClusterIndex];
                var previousDistance = pointProvider.Distance(point, previousCluster);
                var minimumDistance = previousDistance;
                var newClusterIndex = -1;

                for (var j = 0; j < clusterCount; j++)
                {
                    if (
                        distanceToIndexMatrix[previousClusterIndex][j].Distance
                        >= 4 * previousDistance
                    )
                        continue;

                    var distance = pointProvider.Distance(point, clusters[j]);
                    if (distance < minimumDistance)
                    {
                        minimumDistance = distance;
                        newClusterIndex = j;
                    }
                }

                if (newClusterIndex != -1)
                {
                    pointsMoved++;
                    clusterIndices[i] = newClusterIndex;
                }
            }

            if (pointsMoved == 0 && iteration > 0)
            {
                DebugLog($"terminated after {iteration} k-means iterations");
                break;
            }

            DebugLog($"iteration {iteration + 1} moved {pointsMoved}");

            var componentASums = new double[clusterCount];
            var componentBSums = new double[clusterCount];
            var componentCSums = new double[clusterCount];

            Array.Clear(pixelCountSums, 0, clusterCount);

            for (var i = 0; i < pointCount; i++)
            {
                var clusterIndex = clusterIndices[i];
                var point = points[i];
                var count = counts[i];
                pixelCountSums[clusterIndex] += count;
                componentASums[clusterIndex] += point.X * count;
                componentBSums[clusterIndex] += point.Y * count;
                componentCSums[clusterIndex] += point.Z * count;
            }

            for (var i = 0; i < clusterCount; i++)
            {
                var count = pixelCountSums[i];
                if (count == 0)
                {
                    clusters[i] = new Vector3D(0.0, 0.0, 0.0);
                    continue;
                }

                var a = componentASums[i] / count;
                var b = componentBSums[i] / count;
                var c = componentCSums[i] / count;
                clusters[i] = new Vector3D(a, b, c);
            }
        }

        var clusterArgbs = new List<ArgbColor>();
        var clusterPopulations = new List<int>();

        for (var i = 0; i < clusterCount; i++)
        {
            var count = pixelCountSums[i];
            if (count == 0)
                continue;

            var possibleNewCluster = pointProvider.ToArgb(clusters[i]);
            if (clusterArgbs.Contains(possibleNewCluster))
                continue;

            clusterArgbs.Add(possibleNewCluster);
            clusterPopulations.Add(count);
        }

        DebugLog(
            $"kmeans finished and generated {clusterArgbs.Count} clusters; {clusterCount} were requested"
        );

        var inputPixelToClusterPixel = new Dictionary<ArgbColor, ArgbColor>();
        if (returnInputPixelToClusterPixel)
            for (var i = 0; i < pixels.Count; i++)
            {
                var inputPixel = pixels[i];
                var clusterIndex = clusterIndices[i];
                var cluster = clusters[clusterIndex];
                var clusterPixel = pointProvider.ToArgb(cluster);
                inputPixelToClusterPixel[inputPixel] = clusterPixel;
            }

        var colorToCount = new Dictionary<ArgbColor, int>();
        for (var i = 0; i < clusterArgbs.Count; i++)
            colorToCount[clusterArgbs[i]] = clusterPopulations[i];

        return new QuantizerResult(colorToCount, inputPixelToClusterPixel);
    }
}

/// <summary>
/// Helper class for tracking distances to cluster centers.
/// </summary>
internal sealed class DistanceAndIndex : IComparable<DistanceAndIndex>
{
    public double Distance { get; set; }
    public int Index { get; set; }

    public DistanceAndIndex(double distance, int index)
    {
        Distance = distance;
        Index = index;
    }

    public int CompareTo(DistanceAndIndex? other)
    {
        if (other == null)
            return 1;
        if (Distance < other.Distance)
            return -1;
        if (Distance > other.Distance)
            return 1;
        return 0;
    }
}