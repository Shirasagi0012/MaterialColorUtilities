using MaterialColorUtilities.Quantize;
using MaterialColorUtilities.Utils;
using Xunit;

namespace MaterialColorUtilities.Tests.Quantize;

public class QuantizerSynchronousTests
{
    // Captured from e254817 before removing async. Digests include ordered colors,
    // populations and optional mappings, not just set equality.
    [Theory]
    [InlineData("empty", "Map", "CBE5CFDF7C2118A9C3D78EF1D684F3AFA089201352886449A06A6511CFEF74A7")]
    [InlineData("empty", "Wu", "CBE5CFDF7C2118A9C3D78EF1D684F3AFA089201352886449A06A6511CFEF74A7")]
    [InlineData("empty", "Celebi", "CBE5CFDF7C2118A9C3D78EF1D684F3AFA089201352886449A06A6511CFEF74A7")]
    [InlineData("empty", "CelebiMapping", "CBE5CFDF7C2118A9C3D78EF1D684F3AFA089201352886449A06A6511CFEF74A7")]
    [InlineData("one", "Map", "4D7B40D09EF0D08D5980D1AA7E2E2DB1EE1E0C9A2E261E5999C8FB12068714D1")]
    [InlineData("one", "Wu", "C113FCE14E063B7198E4EB522CF19CA1469992E168A3115BDBDC1501E676305C")]
    [InlineData("one", "Celebi", "4D7B40D09EF0D08D5980D1AA7E2E2DB1EE1E0C9A2E261E5999C8FB12068714D1")]
    [InlineData("one", "CelebiMapping", "76D7A84E82318CC638D04AC53C9558EBBEDE7C2AD40ED0A95B65CA20552BA260")]
    [InlineData("repeated", "Map", "65A8A9EA7BF9B59E1FACE2586546F3307D03ADD9CB36B093CCFC4CB4D6B543BC")]
    [InlineData("repeated", "Wu", "C113FCE14E063B7198E4EB522CF19CA1469992E168A3115BDBDC1501E676305C")]
    [InlineData("repeated", "Celebi", "65A8A9EA7BF9B59E1FACE2586546F3307D03ADD9CB36B093CCFC4CB4D6B543BC")]
    [InlineData("repeated", "CelebiMapping", "F7FC3E5CC62B48094A0F6225D4B11144E5A56242017BA9D7E7A284813B289DD0")]
    [InlineData("transparent", "Map", "CBE5CFDF7C2118A9C3D78EF1D684F3AFA089201352886449A06A6511CFEF74A7")]
    [InlineData("transparent", "Wu", "CBE5CFDF7C2118A9C3D78EF1D684F3AFA089201352886449A06A6511CFEF74A7")]
    [InlineData("transparent", "Celebi", "FDE106364FF8DF02F12CA4517675132C78A1E414F64631F6696138587E1572E8")]
    [InlineData("transparent", "CelebiMapping", "67DA2663218A1EEF4C35C1976B61749401650EB1DDA18CDC2C949A24D9352D0F")]
    [InlineData("duplicate-rgb-alpha", "Map", "480842727B22C77273E21C09B14AB4D72FDABC56EF2AD014AA9F8DD746632973")]
    [InlineData("duplicate-rgb-alpha", "Wu", "7FCB9F2CFA9C0D0077C06D14D7660EFD0FA893CF5E15E9C30FFCDBC639D4E8A6")]
    [InlineData("duplicate-rgb-alpha", "Celebi", "480842727B22C77273E21C09B14AB4D72FDABC56EF2AD014AA9F8DD746632973")]
    [InlineData("duplicate-rgb-alpha", "CelebiMapping", "4F3ED8530D33A39DB0C066EDBB8E76CB345403063CA116FC755698824FC31FFF")]
    [InlineData("mixed", "Map", "FA5C8424C20446E82E0F28A4DF7FF1AE12EB0B1CC714145ADE066C96157F1621")]
    [InlineData("mixed", "Wu", "E7B3E1C0B38190EE5686997B80C64E5AEB92A4F160C0774415DC6D1BE5B8D096")]
    [InlineData("mixed", "Celebi", "FEA83F0F44592A49F04F3D9762660210D87FAD878C577C1120CCC9B98CBB0185")]
    [InlineData("mixed", "CelebiMapping", "7717897849120ABE02BB5414137EDC938A663957233C6A8F59ED899893A4175A")]
    [InlineData("noise-1", "Map", "C95F6CAF23E04ECC9C7E9E424D6E571ADEE966189CAF9D86D5E8981AE15D8E54")]
    [InlineData("noise-1", "Wu", "E0D807458DCB5BB66CF570BE4AAB4E5F30601AB1DC873905FB6F987FCA7658EC")]
    [InlineData("noise-1", "Celebi", "4A473DCE2EDB0C77697D3C6B1401948E261C4EB5FF023B5E8E2971FC47C7C78A")]
    [InlineData("noise-1", "CelebiMapping", "5E368FAAA78718E3F89E62C1FC70A2D88CE6104D735205C811CC93836E7ECCF0")]
    [InlineData("noise-2", "Map", "C95F6CAF23E04ECC9C7E9E424D6E571ADEE966189CAF9D86D5E8981AE15D8E54")]
    [InlineData("noise-2", "Wu", "FF2D8F4A574B9ABEB0E20AFBA893A50B6319019EB24BB48687985E5F693F56DC")]
    [InlineData("noise-2", "Celebi", "EAB47716B4412EB509F7B977235AD181F7DDFB4D8F23612212ABE7D801ED324C")]
    [InlineData("noise-2", "CelebiMapping", "C79D9D49FB974282837DA9A20C61A790484F33AA3189A5CBCB5128471FD0D69C")]
    [InlineData("noise-16", "Map", "C95F6CAF23E04ECC9C7E9E424D6E571ADEE966189CAF9D86D5E8981AE15D8E54")]
    [InlineData("noise-16", "Wu", "84869D3711754CCE0668F88A48F4B2AAE015689CA6B447A262CB1FC9CBD43AB9")]
    [InlineData("noise-16", "Celebi", "7CF05D6EDEF3C90BE8E9BBDE354F2DBD699584B9D9F822B54D7B6C84DB150DDC")]
    [InlineData("noise-16", "CelebiMapping", "8DF218A981F71403EE825CACAAC0C17A475697A9E9D58FD7F11DA964EE697826")]
    [InlineData("noise-256", "Map", "C95F6CAF23E04ECC9C7E9E424D6E571ADEE966189CAF9D86D5E8981AE15D8E54")]
    [InlineData("noise-256", "Wu", "215D0A81FA8764E11E9EDB5AC0D008E43F849ADA9E86766F57555642FC5A8C08")]
    [InlineData("noise-256", "Celebi", "104243A99AD9D9FB62AD8D600B6312F1EBAAA837563F8C7F0B2087E208BBC1EB")]
    [InlineData("noise-256", "CelebiMapping", "F5BFBD2CBFE06A4E246ACB08028CBE82A5221B60192088A7A36FA0FD109F16E9")]
    [InlineData("gradient", "Map", "70B8A7E23830ADF4A274B8C52908B02F0F769FB612B387BCD5A691B82A160D2C")]
    [InlineData("gradient", "Wu", "95C3C3F5BA3732A055140A26C5F1B073702091246C8FB4BBE99D3D70A476126E")]
    [InlineData("gradient", "Celebi", "F23AB2B148A25D84C07A8046D4219BAF515B7F4644FD3EE91D087927B80CB20C")]
    [InlineData("gradient", "CelebiMapping", "4FD333B2B9EC9372FD28127E1D121A92004FCF4CC69391379AFA47C745912D31")]
    [InlineData("palette", "Map", "D3561EF6B795BA3524829BC780977EEF4134F35FFACBC0D783003B612ED4819F")]
    [InlineData("palette", "Wu", "691DA5C469158F040378359FD6F28BAFCCFB8777E11AF6910AA2394F820B7D4F")]
    [InlineData("palette", "Celebi", "D3561EF6B795BA3524829BC780977EEF4134F35FFACBC0D783003B612ED4819F")]
    [InlineData("palette", "CelebiMapping", "09D8469D7C8FC0E8578A113162DEE9D84CC8919694FAF1A5671333C2B0B797CD")]
    public void MatchesAsyncBaseline(string caseName, string algorithm, string expected)
    {
        var (_, pixels, maxColors) = QuantizationRegressionData.Cases().Single(c => c.Name == caseName);
        var original = pixels.ToArray();
        var result = algorithm == "CelebiMapping"
            ? new QuantizerCelebi().Quantize(pixels, maxColors, true)
            : Create(algorithm).Quantize(pixels, maxColors);

        Assert.Equal(expected, QuantizationRegressionData.Fingerprint(result));
        Assert.Equal(original, pixels);
    }

    [Fact]
    public void MapRetainsOpaqueCountsAndInsertionOrderRegardlessOfBudget()
    {
        var red = new ArgbColor(0xffff0000);
        var green = new ArgbColor(0xff00ff00);
        var result = new QuantizerMap().Quantize([red, new(0x00ff0000), green, red, new(0xfe00ff00)], 1);
        Assert.Equal(new[] { red, green }, result.ColorToCount.Keys);
        Assert.Equal(new[] { 2, 1 }, result.ColorToCount.Values);
        Assert.Empty(result.InputPixelToClusterPixel);
    }

    [Fact]
    public void WuCanBeReusedSequentially()
    {
        var wu = new QuantizerWu();
        foreach (var (_, pixels, maxColors) in QuantizationRegressionData.Cases())
        {
            var expected = new QuantizerWu().Quantize(pixels, maxColors);
            var actual = wu.Quantize(pixels, maxColors);
            Assert.Equal(QuantizationRegressionData.Fingerprint(expected), QuantizationRegressionData.Fingerprint(actual));
            Assert.All(actual.ColorToCount.Values, count => Assert.Equal(0, count));
        }
    }

    [Theory]
    [InlineData("Map")]
    [InlineData("Wu")]
    [InlineData("Celebi")]
    public void CompletesWithoutPostingToSynchronizationContext(string algorithm)
    {
        var previous = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(new RejectPostingContext());
            var result = Create(algorithm).Quantize(QuantizationRegressionData.Palette(64), 16);
            Assert.NotEmpty(result.ColorToCount);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }
    }

    [Theory]
    [InlineData("Map", 0)]
    [InlineData("Map", -1)]
    [InlineData("Wu", 0)]
    [InlineData("Wu", -1)]
    [InlineData("Celebi", 0)]
    [InlineData("Celebi", -1)]
    public void PreservesExistingInvalidBudgetBehavior(string algorithm, int budget)
    {
        // Baseline e254817: Map ignores budgets; Wu/Celebi do not validate them.
        var quantizer = Create(algorithm);
        List<ArgbColor> pixels = [new(0xffff0000)];
        if (algorithm == "Map")
            Assert.Single(quantizer.Quantize(pixels, budget).ColorToCount);
        else if (budget == 0)
            Assert.Throws<IndexOutOfRangeException>(() => quantizer.Quantize(pixels, budget));
        else
            Assert.Throws<OverflowException>(() => quantizer.Quantize(pixels, budget));
    }

    private static IQuantizer Create(string algorithm) => algorithm switch
    {
        "Map" => new QuantizerMap(),
        "Wu" => new QuantizerWu(),
        "Celebi" => new QuantizerCelebi(),
        _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
    };

    private sealed class RejectPostingContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state) =>
            throw new InvalidOperationException("Synchronous quantizers must not post continuations.");
    }
}
