using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb.ModelsBuilder;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DynamoSharp.Tests.ChangeTracking;

public class EntityChangeTrackerTests
{
    [Fact]
    public void CreateEntityChangeTracker_ShouldCreateSuccessfully()
    {
        // arrange
        var order = EntityChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), "Street 1", "City 1", "State 1", "ZipCode 1");
        var modelBuilder = EntityChangeTrackerTestDataFactory.CreateModelBuilder();

        // act
        var entityChangeTracker = new EntityChangeTracker(modelBuilder, order, EntityState.Added);

        // assert
        entityChangeTracker.Should().NotBeNull();
        entityChangeTracker.IsParentEntity.Should().BeTrue();
        entityChangeTracker.ParentEntity.Should().BeNull();
        entityChangeTracker.Entity.Should().Be(order);
        entityChangeTracker.State.Should().Be(EntityState.Added);
        entityChangeTracker.OriginalEntity.Count.Should().Be(EntityChangeTrackerTestDataFactory.CountPropertiesExcludingLists(order.GetType()));
        entityChangeTracker.ModifiedProperties.Count.Should().Be(0);
    }

    [Fact]
    public void CreateEntityChangeTracker_WithParentEntity_ShouldCreateSuccessfully()
    {
        // arrange
        var order = EntityChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), "Street 1", "City 1", "State 1", "ZipCode 1", 1);
        var item = order.Items[0];
        var modelBuilder = EntityChangeTrackerTestDataFactory.CreateModelBuilder();

        // act
        var entityChangeTracker = new EntityChangeTracker(modelBuilder, item, EntityState.Added, order);

        // assert
        entityChangeTracker.Should().NotBeNull();
        entityChangeTracker.IsParentEntity.Should().BeFalse();
        entityChangeTracker.ParentEntity.Should().NotBeNull();
        entityChangeTracker.Entity.Should().Be(item);
        entityChangeTracker.State.Should().Be(EntityState.Added);
        entityChangeTracker.OriginalEntity.Count.Should().Be(EntityChangeTrackerTestDataFactory.CountPropertiesExcludingLists(item.GetType()));
        entityChangeTracker.ModifiedProperties.Count.Should().Be(0);
    }

    [Fact]
    public void TakeSnapshot_ShouldCaptureOriginalEntityState()
    {
        // Arrange
        var order = EntityChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), "Street 1", "City 1", "State 1", "ZipCode 1", 1);
        var modelBuilder = EntityChangeTrackerTestDataFactory.CreateModelBuilder();
        var jsonSerializer = EntityChangeTrackerTestDataFactory.GetJsonSerializer(modelBuilder.Entities[order.GetType()]);
        var entityChangeTracker = new EntityChangeTracker(modelBuilder, order, EntityState.Added);

        // Act
        entityChangeTracker.TakeSnapshot();

        // Assert
        var expectedOriginalEntity = JObject.FromObject(order, jsonSerializer);
        Assert.True(JToken.DeepEquals(expectedOriginalEntity, entityChangeTracker.OriginalEntity));
    }

    [Fact]
    public void TakeNavegationSnapshots_ShouldCaptureOriginalAndCurrentCollections()
    {
        // Arrange
        var order = EntityChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), "Street 1", "City 1", "State 1", "ZipCode 1", 1);
        var modelBuilder = EntityChangeTrackerTestDataFactory.CreateModelBuilder();
        var entityChangeTracker = new EntityChangeTracker(modelBuilder, order, EntityState.Added);

        // Act
        entityChangeTracker.TakeNavigationSnapshots();

        // Assert
        entityChangeTracker.OneToManyOriginalCollections.Count.Should().Be(1);
        entityChangeTracker.OneToManyCurrentCollections.Count.Should().Be(1);
    }

    [Fact]
    public void HasChanged_ShouldReturnFalse_WhenEntityHasNotChanged()
    {
        var order = EntityChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), "Street 1", "City 1", "State 1", "ZipCode 1");
        var modelBuilder = EntityChangeTrackerTestDataFactory.CreateModelBuilder();
        var entityChangeTracker = new EntityChangeTracker(modelBuilder, order, EntityState.Added);
        entityChangeTracker.TakeSnapshot();
        entityChangeTracker.TakeNavigationSnapshots();

        // Act
        var hasChanged = entityChangeTracker.HasChanged();

        // Assert
        hasChanged.Should().BeFalse();
        entityChangeTracker.ModifiedProperties.Count.Should().Be(0);
    }

    [Fact]
    public void HasChanged_ShouldReturnTrue_WhenEntityHasChanged()
    {
        // Arrange
        var order = EntityChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), "Street 1", "City 1", "State 1", "ZipCode 1");
        var modelBuilder = EntityChangeTrackerTestDataFactory.CreateModelBuilder();
        var entityChangeTracker = new EntityChangeTracker(modelBuilder, order, EntityState.Added);
        entityChangeTracker.TakeSnapshot();
        entityChangeTracker.TakeNavigationSnapshots();
        order.UpdateAddress("Street 2", "City 2", "State 2", "ZipCode 2");

        // Act
        var hasChanged = entityChangeTracker.HasChanged();

        // Assert
        hasChanged.Should().BeTrue();
        entityChangeTracker.ModifiedProperties.Count.Should().Be(1);
        entityChangeTracker.ModifiedProperties.Properties().First().Path.Should().Be("Address");
    }
}
