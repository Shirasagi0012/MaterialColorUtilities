using MaterialColorUtilities.Score;
using MaterialColorUtilities.Utils;
using MaterialColorUtilities.Tests.TestUtils;
using Xunit;

namespace MaterialColorUtilities.Tests.Score;

public class ScoreTests
{
    [Fact]
    public void PrioritizesChroma()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff000000), 1 },
            { new ArgbColor(0xffffffff), 1 },
            { new ArgbColor(0xff0000ff), 1 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(colorsToPopulation, desired: 4);

        Assert.Single(ranked);
        ranked[0].AssertColorEquals(new ArgbColor(0xff0000ff));
    }

    [Fact]
    public void PrioritizesChromaWhenProportionsEqual()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xffff0000), 1 },
            { new ArgbColor(0xff00ff00), 1 },
            { new ArgbColor(0xff0000ff), 1 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(colorsToPopulation, desired: 4);

        Assert.Equal(3, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xffff0000));
        ranked[1].AssertColorEquals(new ArgbColor(0xff00ff00));
        ranked[2].AssertColorEquals(new ArgbColor(0xff0000ff));
    }

    [Fact]
    public void GeneratesGBlueWhenNoColorsAvailable()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff000000), 1 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(colorsToPopulation, desired: 4);

        Assert.Single(ranked);
        ranked[0].AssertColorEquals(new ArgbColor(0xff4285f4));
    }

    [Fact]
    public void DedupesNearbyHues()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff008772), 1 }, // H 180 C 42 T 50
            { new ArgbColor(0xff318477), 1 }  // H 184 C 35 T 50
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(colorsToPopulation, desired: 4);

        Assert.Single(ranked);
        ranked[0].AssertColorEquals(new ArgbColor(0xff008772));
    }

    [Fact]
    public void MaximizesHueDistance()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff008772), 1 }, // H 180 C 42 T 50
            { new ArgbColor(0xff008587), 1 }, // H 198 C 50 T 50
            { new ArgbColor(0xff007ebc), 1 }  // H 245 C 50 T 50
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(colorsToPopulation, desired: 2);
        Assert.Equal(2, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xff007ebc));
        ranked[1].AssertColorEquals(new ArgbColor(0xff008772));
    }

    [Fact]
    public void PassesGeneratedScenarioOne()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff7ea16d), 67 },
            { new ArgbColor(0xffd8ccae), 67 },
            { new ArgbColor(0xff835c0d), 49 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 3,
            fallbackColorARGB: new ArgbColor(0xff8d3819),
            filter: false);

        Assert.Equal(3, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xff7ea16d));
        ranked[1].AssertColorEquals(new ArgbColor(0xffd8ccae));
        ranked[2].AssertColorEquals(new ArgbColor(0xff835c0d));
    }

    [Fact]
    public void PassesGeneratedScenarioTwo()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xffd33881), 14 },
            { new ArgbColor(0xff3205cc), 77 },
            { new ArgbColor(0xff0b48cf), 36 },
            { new ArgbColor(0xffa08f5d), 81 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 4,
            fallbackColorARGB: new ArgbColor(0xff7d772b),
            filter: true);

        Assert.Equal(3, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xff3205cc));
        ranked[1].AssertColorEquals(new ArgbColor(0xffa08f5d));
        ranked[2].AssertColorEquals(new ArgbColor(0xffd33881));
    }

    [Fact]
    public void PassesGeneratedScenarioThree()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xffbe94a6), 23 },
            { new ArgbColor(0xffc33fd7), 42 },
            { new ArgbColor(0xff899f36), 90 },
            { new ArgbColor(0xff94c574), 82 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 3,
            fallbackColorARGB: new ArgbColor(0xffaa79a4),
            filter: true);

        Assert.Equal(3, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xff94c574));
        ranked[1].AssertColorEquals(new ArgbColor(0xffc33fd7));
        ranked[2].AssertColorEquals(new ArgbColor(0xffbe94a6));
    }

    [Fact]
    public void PassesGeneratedScenarioFour()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xffdf241c), 85 },
            { new ArgbColor(0xff685859), 44 },
            { new ArgbColor(0xffd06d5f), 34 },
            { new ArgbColor(0xff561c54), 27 },
            { new ArgbColor(0xff713090), 88 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 5,
            fallbackColorARGB: new ArgbColor(0xff58c19c),
            filter: false);

        Assert.Equal(2, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xffdf241c));
        ranked[1].AssertColorEquals(new ArgbColor(0xff561c54));
    }

    [Fact]
    public void PassesGeneratedScenarioFive()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xffbe66f8), 41 },
            { new ArgbColor(0xff4bbda9), 88 },
            { new ArgbColor(0xff80f6f9), 44 },
            { new ArgbColor(0xffab8017), 43 },
            { new ArgbColor(0xffe89307), 65 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 3,
            fallbackColorARGB: new ArgbColor(0xff916691),
            filter: false);

        Assert.Equal(3, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xffab8017));
        ranked[1].AssertColorEquals(new ArgbColor(0xff4bbda9));
        ranked[2].AssertColorEquals(new ArgbColor(0xffbe66f8));
    }

    [Fact]
    public void PassesGeneratedScenarioSix()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff18ea8f), 93 },
            { new ArgbColor(0xff327593), 18 },
            { new ArgbColor(0xff066a18), 53 },
            { new ArgbColor(0xfffa8a23), 74 },
            { new ArgbColor(0xff04ca1f), 62 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 2,
            fallbackColorARGB: new ArgbColor(0xff4c377a),
            filter: false);

        Assert.Equal(2, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xff18ea8f));
        ranked[1].AssertColorEquals(new ArgbColor(0xfffa8a23));
    }

    [Fact]
    public void PassesGeneratedScenarioSeven()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff2e05ed), 23 },
            { new ArgbColor(0xff153e55), 90 },
            { new ArgbColor(0xff9ab220), 23 },
            { new ArgbColor(0xff153379), 66 },
            { new ArgbColor(0xff68bcc3), 81 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 2,
            fallbackColorARGB: new ArgbColor(0xfff588dc),
            filter: true);

        Assert.Equal(2, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xff2e05ed));
        ranked[1].AssertColorEquals(new ArgbColor(0xff9ab220));
    }

    [Fact]
    public void PassesGeneratedScenarioEight()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff816ec5), 24 },
            { new ArgbColor(0xff6dcb94), 19 },
            { new ArgbColor(0xff3cae91), 98 },
            { new ArgbColor(0xff5b542f), 25 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 1,
            fallbackColorARGB: new ArgbColor(0xff84b0fd),
            filter: false);

        Assert.Single(ranked);
        ranked[0].AssertColorEquals(new ArgbColor(0xff3cae91));
    }

    [Fact]
    public void PassesGeneratedScenarioNine()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff206f86), 52 },
            { new ArgbColor(0xff4a620d), 96 },
            { new ArgbColor(0xfff51401), 85 },
            { new ArgbColor(0xff2b8ebf), 3 },
            { new ArgbColor(0xff277766), 59 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 3,
            fallbackColorARGB: new ArgbColor(0xff02b415),
            filter: true);

        Assert.Equal(3, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xfff51401));
        ranked[1].AssertColorEquals(new ArgbColor(0xff4a620d));
        ranked[2].AssertColorEquals(new ArgbColor(0xff2b8ebf));
    }

    [Fact]
    public void PassesGeneratedScenarioTen()
    {
        var colorsToPopulation = new Dictionary<ArgbColor, int>
        {
            { new ArgbColor(0xff8b1d99), 54 },
            { new ArgbColor(0xff27effe), 43 },
            { new ArgbColor(0xff6f558d), 2 },
            { new ArgbColor(0xff77fdf2), 78 }
        };

        var ranked = MaterialColorUtilities.Score.Score.CalculateScore(
            colorsToPopulation,
            desired: 4,
            fallbackColorARGB: new ArgbColor(0xff5e7a10),
            filter: true);

        Assert.Equal(3, ranked.Count);
        ranked[0].AssertColorEquals(new ArgbColor(0xff27effe));
        ranked[1].AssertColorEquals(new ArgbColor(0xff8b1d99));
        ranked[2].AssertColorEquals(new ArgbColor(0xff6f558d));
    }
}