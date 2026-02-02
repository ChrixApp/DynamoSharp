using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.TestContexts.Models.Ecommerce;
using FluentAssertions;

namespace DynamoSharp.Tests.DynamoDb.ModelsBuilder;

public class ModelBuilderTests
{
    [Fact]
    public void Create_ShouldReturnModelBuilder()
    {
        // Act
        var modelBuilder = new ModelBuilder();

        // Assert
        modelBuilder.Should().NotBeNull();
    }

    [Fact]
    public void Entity_ShouldReturnEntityTypeBuilder()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();

        // Act
        var entityTypeBuilder = modelBuilder.Entity<Order>();

        // Assert
        entityTypeBuilder.Should().NotBeNull();
        entityTypeBuilder.Should().BeOfType<EntityTypeBuilder<Order>>();
    }

    [Fact]
    public void Entity_ShouldAddEntityTypeBuilderToEntities()
    {
        // Arrange
        var modelBuilder = new ModelBuilder();

        // Act
        modelBuilder.Entity<Order>();
        modelBuilder.Entity<Item>();

        // Assert
        modelBuilder.Entities.Should().NotBeEmpty();
        modelBuilder.Entities.Count.Should().Be(2);
    }
}