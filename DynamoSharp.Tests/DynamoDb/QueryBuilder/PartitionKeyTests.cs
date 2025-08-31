using DynamoSharp.DynamoDb.QueryBuilder;

namespace DynamoSharp.Tests.DynamoDb.QueryBuilder;

public class PartitionKeyTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        var pk = new PartitionKey("id", "123");

        Assert.Equal("id", pk.AttributeName);
        Assert.Equal("123", pk.AttributeValue);
    }

    [Fact]
    public void Create_ShouldReturnNewInstanceWithSameValues()
    {
        var pk = PartitionKey.Create("user", "alice");

        Assert.IsType<PartitionKey>(pk);
        Assert.Equal("user", pk.AttributeName);
        Assert.Equal("alice", pk.AttributeValue);
    }

    [Fact]
    public void ToString_ShouldReturnAttributeNameEqualsAttributeValue()
    {
        var pk = new PartitionKey("pkName", "pkValue");

        var text = pk.ToString();

        Assert.Equal("pkName = pkValue", text);
    }

    [Fact]
    public void HandlesEmptyStrings_CorrectlyFormatsToString()
    {
        var pk = new PartitionKey(string.Empty, string.Empty);

        Assert.Equal(string.Empty, pk.AttributeName);
        Assert.Equal(string.Empty, pk.AttributeValue);
        Assert.Equal(" = ", pk.ToString());
    }
}
