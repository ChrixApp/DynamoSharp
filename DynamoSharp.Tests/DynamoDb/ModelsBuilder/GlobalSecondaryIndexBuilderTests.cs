using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models;
using FluentAssertions;
using Xunit;

namespace DynamoSharp.Tests.DynamoDb.ModelsBuilder;

public class GlobalSecondaryIndexBuilderTests
{
    [Fact]
    public void Create_ShouldReturnGlobalSecondaryIndexBuilder()
    {
        // Arrange
        var gsi = new List<GlobalSecondaryIndex>();
        gsi.Add(new GlobalSecondaryIndex("BuyerId", "BUYER"));

        // Act
        var globalSecondaryIndexBuilder = new GlobalSecondaryIndexBuilder<Order>(gsi);

        // Assert
        globalSecondaryIndexBuilder.Should().NotBeNull();
        globalSecondaryIndexBuilder.GSI.Should().NotBeNull();
        globalSecondaryIndexBuilder.GSI.Should().HaveCount(1);
        globalSecondaryIndexBuilder.GSI.First().Path.Should().Be("BuyerId");
        globalSecondaryIndexBuilder.GSI.First().Prefix.Should().Be("BUYER");
    }

    [Fact]
    public void Include_ShouldAddGlobalSecondaryIndex()
    {
        // Arrange
        var gsi = new List<GlobalSecondaryIndex>();
        gsi.Add(new GlobalSecondaryIndex("BuyerId", "BUYER"));

        var globalSecondaryIndexBuilder = new GlobalSecondaryIndexBuilder<Order>(gsi);

        // Act
        globalSecondaryIndexBuilder.Include(o => o.Date, "DATE");

        // Assert
        globalSecondaryIndexBuilder.Should().NotBeNull();
        globalSecondaryIndexBuilder.GSI.Should().NotBeNull();
        globalSecondaryIndexBuilder.GSI.Should().HaveCount(2);
        globalSecondaryIndexBuilder.GSI.First().Path.Should().Be("BuyerId");
        globalSecondaryIndexBuilder.GSI.First().Prefix.Should().Be("BUYER");
        globalSecondaryIndexBuilder.GSI.Last().Path.Should().Be("Date");
        globalSecondaryIndexBuilder.GSI.Last().Prefix.Should().Be("DATE");
    }
}

