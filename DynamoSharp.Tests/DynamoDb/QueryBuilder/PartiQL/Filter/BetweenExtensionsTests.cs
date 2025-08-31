using DynamoSharp.DynamoDb.QueryBuilder.PartiQL.Filter;

namespace DynamoSharp.Tests.DynamoDb.QueryBuilder.PartiQL.Filter;

public class BetweenExtensionsTests
{
    [Fact]
    public void NumericOverloads_ReturnTrueForTypicalRanges()
    {
        Assert.True(((short)5).Between((short)1, (short)10));
        Assert.True(((ushort)5).Between((ushort)1, (ushort)10));
        Assert.True(5.Between(1, 10));
        Assert.True(((uint)5).Between((uint)1, (uint)10));
        Assert.True(5L.Between(1L, 10L));
        Assert.True(((ulong)5).Between((ulong)1, (ulong)10));
        Assert.True(5.5f.Between(1.1f, 10.9f));
        Assert.True(5.5d.Between(1.1d, 10.9d));
        Assert.True(5.5m.Between(1.1m, 10.9m));
    }

    [Fact]
    public void NumericOverloads_HandleEdgeValues()
    {
        Assert.True(short.MinValue.Between(short.MinValue, short.MaxValue));
        Assert.True(ushort.MinValue.Between(ushort.MinValue, ushort.MaxValue));
        Assert.True(int.MinValue.Between(int.MinValue, int.MaxValue));
        Assert.True(uint.MinValue.Between(uint.MinValue, uint.MaxValue));
        Assert.True(long.MinValue.Between(long.MinValue, long.MaxValue));
        Assert.True(ulong.MinValue.Between(ulong.MinValue, ulong.MaxValue));
        Assert.True(float.MinValue.Between(float.MinValue, float.MaxValue));
        Assert.True(double.MinValue.Between(double.MinValue, double.MaxValue));
        Assert.True(decimal.MinValue.Between(decimal.MinValue, decimal.MaxValue));
    }

    [Fact]
    public void FloatingPoint_NaNAndInfinity_AreHandled()
    {
        // Current implementation returns true unconditionally; ensure calling these values does not throw.
        Assert.True(float.NaN.Between(float.MinValue, float.MaxValue));
        Assert.True(float.PositiveInfinity.Between(float.MinValue, float.MaxValue));
        Assert.True(float.NegativeInfinity.Between(float.MinValue, float.MaxValue));

        Assert.True(double.NaN.Between(double.MinValue, double.MaxValue));
        Assert.True(double.PositiveInfinity.Between(double.MinValue, double.MaxValue));
        Assert.True(double.NegativeInfinity.Between(double.MinValue, double.MaxValue));
    }

    [Fact]
    public void StringOverload_ReturnsTrueForLexicographicRanges()
    {
        Assert.True("m".Between("a", "z"));
        Assert.True("apple".Between("ant", "azure"));
        Assert.True("zzz".Between("a", "zzz"));
    }

    [Fact]
    public void When_BoundsReversed_MethodStillReturnsTrue()
    {
        // Current implementation is a no-op for runtime evaluation; verify it remains callable in reversed-bounds cases.
        Assert.True(5.Between(10, 1));
        Assert.True("m".Between("z", "a"));
    }
}