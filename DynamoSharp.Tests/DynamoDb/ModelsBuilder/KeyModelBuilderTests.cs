using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.TestContexts.Models.Ecommerce;
using FluentAssertions;

namespace DynamoSharp.Tests.DynamoDb.ModelsBuilder;

public class KeyModelBuilderTests
{
    [Fact]
    public void Create_ShouldReturnKeyBuilder()
    {
        // Arrange
        var properties = new Dictionary<string, string>();
        properties.Add("Id", "ORDER");

        // Act
        var keyBuilder = new KeyModelBuilder<Order>(properties);

        // Assert
        keyBuilder.Should().NotBeNull();
        keyBuilder.Properties.Should().NotBeEmpty();
        keyBuilder.Properties.Should().ContainKey("Id");
        keyBuilder.Properties["Id"].Should().Be("ORDER");
    }

    [Fact]
    public void Include_ShouldAddProperties()
    {
        // Arrange
        var keyBuilder = new KeyModelBuilder<Order>(new Dictionary<string, string>());

        // Act
        keyBuilder.Include(o => o.Id, "ID");
        keyBuilder.Include(o => o.BuyerId, "BUYER");
        keyBuilder.Include(o => o.Date, "DATE");

        // Assert
        keyBuilder.Properties.Should().HaveCount(3);
        keyBuilder.Properties.Should().ContainKey("Id");
        keyBuilder.Properties[keyBuilder.Properties.Keys.First()].Should().Be("ID");
        keyBuilder.Properties.Should().ContainKey("BuyerId");
        keyBuilder.Properties[keyBuilder.Properties.Keys.Skip(1).First()].Should().Be("BUYER");
        keyBuilder.Properties.Should().ContainKey("Date");
        keyBuilder.Properties[keyBuilder.Properties.Keys.Last()].Should().Be("DATE");
    }
}

