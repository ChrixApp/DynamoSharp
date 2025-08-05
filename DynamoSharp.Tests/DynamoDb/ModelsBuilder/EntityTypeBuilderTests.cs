using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models;
using DynamoSharp.Tests.Contexts.Models.Affiliation;
using FluentAssertions;
using Xunit;

namespace DynamoSharp.Tests.DynamoDb.ModelsBuilder;

public class EntityTypeBuilderTests
{
    [Fact]
    public void Create_ShouldReturnEntityTypeBuilder()
    {
        // Act
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Assert
        entityTypeBuilder.Should().NotBeNull();
        entityTypeBuilder.PartitionKey.Should().NotBeNull();
        entityTypeBuilder.PartitionKey.Should().BeEmpty();
        entityTypeBuilder.SortKey.Should().NotBeNull();
        entityTypeBuilder.SortKey.Should().BeEmpty();
        entityTypeBuilder.OneToMany.Should().NotBeNull();
        entityTypeBuilder.OneToMany.Should().BeEmpty();
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.Should().NotBeNull();
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.Should().BeEmpty();
        entityTypeBuilder.GlobalSecondaryIndexSortKey.Should().NotBeNull();
        entityTypeBuilder.GlobalSecondaryIndexSortKey.Should().BeEmpty();
    }

    [Fact]
    public void HasId_ShouldSetIdNameToPropertyName()
    {
        // Arrange
        var builder = new EntityTypeBuilder<Affiliation>();

        // Act
        builder.HasId(e => e.TerminalId);

        // Assert
        Assert.Equal("TerminalId", builder.IdName);
    }

    [Fact]
    public void HasPartitionKey_ShouldSetPartitionKey()
    {
        // Arrange
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Act
        entityTypeBuilder.HasPartitionKey(o => o.Id, "ORDER");

        // Assert
        entityTypeBuilder.PartitionKey.Should().HaveCount(1);
        entityTypeBuilder.PartitionKey.First().Key.Should().Be("Id");
        entityTypeBuilder.PartitionKey.First().Value.Should().Be("ORDER");
    }

    [Fact]
    public void HasSortKey_ShouldSetSortKey()
    {
        // Arrange
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Act
        entityTypeBuilder.HasSortKey(o => o.Id, "ORDER");

        // Assert
        entityTypeBuilder.SortKey.Should().HaveCount(1);
        entityTypeBuilder.SortKey.First().Key.Should().Be("Id");
        entityTypeBuilder.SortKey.First().Value.Should().Be("ORDER");
    }

    [Fact]
    public void HasGlobalSecondaryIndexPartitionKey_WithExpression_ShouldSetGsiPartitionKey()
    {
        // Arrange
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Act
        entityTypeBuilder.HasGlobalSecondaryIndexPartitionKey("GSI1PK", o => o.BuyerId, "BUYER");

        // Assert
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.Should().HaveCount(1);
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Key.Should().Be("GSI1PK");
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value.Should().HaveCount(1);
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value.First().Path.Should().Be("BuyerId");
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value.First().Prefix.Should().Be("BUYER");
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value.First().Value.Should().Be(string.Empty);
    }

    [Fact]
    public void HasGlobalSecondaryIndexPartitionKey_ShouldSetGsiPartitionKey()
    {
        // Arrange
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Act
        entityTypeBuilder.HasGlobalSecondaryIndexPartitionKey("GSI1PK", "BUYER");

        // Assert
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.Should().HaveCount(1);
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Key.Should().Be("GSI1PK");
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value.Should().HaveCount(1);
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value.First().Path.Should().Be(string.Empty);
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value.First().Prefix.Should().Be(string.Empty);
        entityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value.First().Value.Should().Be("BUYER");
    }

    [Fact]
    public void HasGlobalSecondaryIndexSortKey_WithExpression_ShouldSetGsiSortKey()
    {
        // Arrange
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Act
        entityTypeBuilder.HasGlobalSecondaryIndexSortKey("GSI1SK", o => o.BuyerId, "BUYER");

        // Assert
        entityTypeBuilder.GlobalSecondaryIndexSortKey.Should().HaveCount(1);
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Key.Should().Be("GSI1SK");
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value.Should().HaveCount(1);
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value.First().Path.Should().Be("BuyerId");
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value.First().Prefix.Should().Be("BUYER");
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value.First().Value.Should().Be(string.Empty);
    }

    [Fact]
    public void HasGlobalSecondaryIndexSortKey_ShouldSetGsiSortKey()
    {
        // Arrange
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Act
        entityTypeBuilder.HasGlobalSecondaryIndexSortKey("GSI1SK", "BUYER");

        // Assert
        entityTypeBuilder.GlobalSecondaryIndexSortKey.Should().HaveCount(1);
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Key.Should().Be("GSI1SK");
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value.Should().HaveCount(1);
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value.First().Path.Should().Be(string.Empty);
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value.First().Prefix.Should().Be(string.Empty);
        entityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value.First().Value.Should().Be("BUYER");
    }

    [Fact]
    public void HasOneToMany_ShouldSetOneToMany()
    {
        // Arrange
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Act
        entityTypeBuilder.HasOneToMany(o => o.Items);

        // Assert
        entityTypeBuilder.OneToMany.Values.AsEnumerable().Should().HaveCount(1);
        entityTypeBuilder.OneToMany.Values.AsEnumerable().First().Should().Be(typeof(Item));
    }

    [Fact]
    public void HasManyToMany_ShouldSetManyToMany()
    {
        // Arrange
        var entityTypeBuilder = new EntityTypeBuilder<Order>();

        // Act
        entityTypeBuilder.HasManyToMany(o => o.Items);

        // Assert
        entityTypeBuilder.ManyToMany.Values.AsEnumerable().Should().HaveCount(1);
        entityTypeBuilder.ManyToMany.Values.AsEnumerable().First().Should().Be(typeof(Item));
    }
}
