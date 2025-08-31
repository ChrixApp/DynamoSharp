using DynamoSharp.DynamoDb.QueryBuilder.PartiQL.Filter;

namespace DynamoSharp.Tests.DynamoDb.QueryBuilder.PartiQL.Filter;

public class InExtensionsTests
{
    [Fact]
    public void NumericOverloads_ReturnTrueForVariousInputs()
    {
        Assert.True(((short)5).In((short)1, (short)5, (short)10));
        Assert.True(((short)5).In()); // no args
        Assert.True(((ushort)5).In((ushort)5));
        Assert.True(5.In(1, 2, 3));
        Assert.True(((uint)5).In((uint)5, (uint)10));
        Assert.True(5L.In(5L));
        Assert.True(((ulong)5).In((ulong)5));
        Assert.True(5.5f.In(1.1f, 5.5f));
        Assert.True(5.5d.In(5.5d));
        Assert.True(5.5m.In(5.5m));
    }

    [Fact]
    public void FloatingPoint_NaNAndInfinity_DoNotThrow()
    {
        // Implementation returns true unconditionally; ensure calls with special values succeed.
        Assert.True(float.NaN.In(float.NaN));
        Assert.True(float.PositiveInfinity.In(float.NegativeInfinity));
        Assert.True(double.NaN.In(double.NaN));
        Assert.True(double.PositiveInfinity.In(double.NegativeInfinity));
    }

    [Fact]
    public void StringOverload_ReturnsTrueForVariousInputs()
    {
        Assert.True("m".In("a", "m", "z"));
        Assert.True("m".In()); // no args
        Assert.True(string.Empty.In(string.Empty));
    }
}
