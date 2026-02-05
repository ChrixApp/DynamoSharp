using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.DynamoDb.QueryBuilder;
using DynamoSharp.Tests.Contexts.Models.Movies;
using DynamoSharp.Tests.DynamoDb.QueryBuilder;
using DynamoSharp.Tests.TestContexts;
using DynamoSharp.Tests.TestContexts.Models.Ecommerce;
using EfficientDynamoDb;
using EfficientDynamoDb.Operations.ExecuteStatement;
using FluentAssertions;
using Moq;
using System.Collections.Concurrent;
using static DynamoSharp.Tests.Converters.Jsons.JsonSerializerBuilderTests;

namespace DynamoSharp.Tests.ChangeTracking;

public class ChangeTrackerTests
{
    [Fact]
    public void CreateChangeTracker_ShouldCreateSuccessfully()
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = new ModelBuilder();

        // act
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        // assert
        changeTracker.Should().NotBeNull();
        changeTracker.Entries.Count.Should().Be(0);
        var changes = changeTracker.FetchChanges();
        changes.AddedEntities.Count.Should().Be(0);
        changes.ModifiedEntities.Count.Should().Be(0);
        changes.DeletedEntities.Count.Should().Be(0);
    }

    [Fact]
    public void Track_AddsNewEntity_WhenEntityIsNotTracked()
    {
        // Arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForAffiliation();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var affiliation = ChangeTrackerTestDataFactory.CreateAffiliation();
        var state = EntityState.Added;
        changeTracker.Track(affiliation, EntityState.Added);

        // Act
        changeTracker.Track(affiliation, state);

        // Assert
        Assert.Single(changeTracker.Entries);
        Assert.Equal(state, changeTracker.Entries[affiliation.GetHashCode()].State);
    }

    [Fact]
    public void Track_UpdatesState_WhenEntityIsAlreadyTracked()
    {
        // Arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForAffiliation();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var affiliation = ChangeTrackerTestDataFactory.CreateAffiliation();
        var initialState = EntityState.Unchanged;
        var newState = EntityState.Modified;
        changeTracker.Track(affiliation, initialState);

        // Act
        changeTracker.Track(affiliation, newState);

        // Assert
        Assert.Single(changeTracker.Entries);
        Assert.Equal(newState, changeTracker.Entries[affiliation.GetHashCode()].State);
    }

    [Fact]
    public void Track_AddsNewEntityWithParent_WhenEntityIsNotTracked()
    {
        // Arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var parentEntity = ChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), productCount: 1);
        var entity = parentEntity.Items[0];
        var state = EntityState.Added;

        // Act
        changeTracker.Track(entity, state, parentEntity);

        // Assert
        Assert.Single(changeTracker.Entries);
        var trackedEntity = changeTracker.Entries[entity.GetHashCode()];
        Assert.Equal(state, trackedEntity.State);
        Assert.Equal(parentEntity, trackedEntity.ParentEntity);
    }

    [Fact]
    public void Track_UpdatesStateWithParent_WhenEntityIsAlreadyTracked()
    {
        // Arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var parentEntity = ChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), productCount: 1);
        var entity = parentEntity.Items[0];
        var initialState = EntityState.Unchanged;
        var newState = EntityState.Modified;

        changeTracker.Track(entity, initialState, parentEntity);

        // Act
        changeTracker.Track(entity, newState, parentEntity);

        // Assert
        Assert.Single(changeTracker.Entries);
        var trackedEntity = changeTracker.Entries[entity.GetHashCode()];
        Assert.Equal(newState, trackedEntity.State);
        Assert.Equal(parentEntity, trackedEntity.ParentEntity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void AddEntities_ShouldAddEntities(int addedEntities)
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForAffiliation();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        for (int i = 0; i < addedEntities; i++)
        {
            var affiliation = ChangeTrackerTestDataFactory.CreateAffiliation();
            changeTracker.Track(affiliation, EntityState.Added);
        }

        // act
        var changes = changeTracker.FetchChanges();

        // assert
        changes.AddedEntities.Count.Should().Be(addedEntities);
    }

    [Fact]
    public void Untrack_RemovesEntity_WhenEntityIsTracked()
    {
        // Arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForAffiliation();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var affiliation = ChangeTrackerTestDataFactory.CreateAffiliation();
        changeTracker.Track(affiliation, EntityState.Added);

        // Act
        changeTracker.Untrack(affiliation);

        // Assert
        Assert.Empty(changeTracker.Entries);
    }

    [Fact]
    public void Untrack_DoesNothing_WhenEntityIsNotTracked()
    {
        // Arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForAffiliation();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var affiliation = ChangeTrackerTestDataFactory.CreateAffiliation();

        // Act
        changeTracker.Untrack(affiliation);

        // Assert
        Assert.Empty(changeTracker.Entries);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 2)]
    [InlineData(5, 3)]
    public void RemoveRootEntities_ShouldRemoveEntities(int addedEntities, int removeEntities)
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var orders = new List<Order>();

        for (int i = 0; i < addedEntities; i++)
        {
            var order = ChangeTrackerTestDataFactory.CreateOrder();
            changeTracker.Track(order, EntityState.Added);
            orders.Add(order);
        }

        // act
        for (int i = 0; i < removeEntities; i++)
        {
            changeTracker.ChangeState(orders[i], EntityState.Deleted);
        }

        // assert
        changeTracker.Entries.Count(x => x.Value.State == EntityState.Deleted).Should().Be(removeEntities);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 6)]
    [InlineData(5, 10)]
    public void GetAddedEntities_ShouldReturnAddedEntities(int totalOrders, int totalEntities)
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        for (int i = 0; i < totalOrders; i++)
        {
            var order = ChangeTrackerTestDataFactory.CreateOrder();
            order.AddProduct(Guid.NewGuid(), $"Product 1", 10);
            changeTracker.Track(order, EntityState.Added);
        }

        // act
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // assert
        addedEntities.Count.Should().Be(totalEntities);
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(3, 2, 1, 3)]
    [InlineData(5, 3, 3, 12)]
    public void GetDeletedEntities_ShouldReturnDeletedEntities(int totalOrders, int totalProducts, int deletedOrders, int totalEntities)
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var orders = new List<Order>();

        for (int i = 0; i < totalOrders; i++)
        {
            var order = ChangeTrackerTestDataFactory.CreateOrder();
            for (int j = 0; j < totalProducts; j++)
            {
                order.AddProduct(Guid.NewGuid(), $"Product {j + 1}", 10);
            }
            changeTracker.Track(order, EntityState.Added);
            orders.Add(order);
        }

        for (int i = 0; i < deletedOrders; i++)
        {
            changeTracker.ChangeState(orders[i], EntityState.Deleted);
        }

        // act
        var (_, _, deletedEntities) = changeTracker.FetchChanges();

        // assert
        deletedEntities.Count.Should().Be(totalEntities);
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(3, 1, 1, 0)]
    [InlineData(5, 3, 1, 1)]
    [InlineData(6, 6, 3, 2)]
    public void GetModifiedEntities_WithSeveralPropertiesModified_ShouldReturnModifiedEntities(int totalOrders, int totalProducts, int totalModifiedRootEntities, int totalModifiedEntities)
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var mockDynamoSharpContext = new Mock<IDynamoSharpContext>();
        mockDynamoSharpContext.SetupGet(x => x.ChangeTracker).Returns(changeTracker);
        mockDynamoSharpContext.SetupGet(x => x.ModelBuilder).Returns(modelBuilder);
        var dynamoDbSet = new DynamoDbSet<Order>(mockDynamoSharpContext.Object);
        var orders = new List<Order>();

        for (int i = 0; i < totalOrders; i++)
        {
            var order = ChangeTrackerTestDataFactory.CreateOrder();
            for (int j = 0; j < totalProducts; j++)
            {
                order.AddProduct(Guid.NewGuid(), $"Product {j + 1}", 10);
            }
            changeTracker.Track(order, EntityState.Added);
            orders.Add(order);
        }

        for (int i = 0; i < totalModifiedRootEntities; i++)
        {
            for (int j = 0; j < totalModifiedEntities; j++)
            {
                var product = orders[i].Items[j];
                orders[i].AddProduct(product.Id, product.ProductName, 20, 0);
            }
        }

        // act
        var (_, modifiedEntities, _) = changeTracker.FetchChanges();

        // assert
        modifiedEntities.Count.Should().Be(totalModifiedRootEntities * totalModifiedEntities);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 3)]
    public void GetModifiedEntities_WithOnePropertyModified_ShouldReturnModifiedEntities(
        int totalOrders, int totalModifiedRootEntities)
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var orders = new List<Order>();

        for (int i = 0; i < totalOrders; i++)
        {
            var order = ChangeTrackerTestDataFactory.CreateOrder();
            for (int j = 0; j < 2; j++)
            {
                order.AddProduct(Guid.NewGuid(), $"Product {j + 1}", 10);
            }
            changeTracker.Track(order, EntityState.Added);
            orders.Add(order);
        }

        for (int i = 0; i < totalModifiedRootEntities; i++)
        {
            orders[i].UpdateAddress("Street 2", "City 2", "State 2", "ZipCode 2");
        }

        // act
        var (_, modifiedEntities, _) = changeTracker.FetchChanges();

        // assert
        modifiedEntities.Count.Should().Be(totalModifiedRootEntities);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(3, 2, 1, 2, 3)]
    [InlineData(5, 4, 2, 5, 0)]
    [InlineData(8, 7, 5, 6, 16)]
    public void AcceptChanges_ShouldAcceptAllChanges(int totalOrders, int productsToAdd, int rootEntitiesToModify, int removeEntities, int totalEntities)
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var mockDynamoSharpContext = new Mock<IDynamoSharpContext>();
        mockDynamoSharpContext.SetupGet(x => x.ChangeTracker).Returns(changeTracker);
        mockDynamoSharpContext.SetupGet(x => x.ModelBuilder).Returns(modelBuilder);
        var dynamoDbSet = new DynamoDbSet<Order>(mockDynamoSharpContext.Object);
        var orders = new List<Order>();

        for (int i = 0; i < totalOrders; i++)
        {
            var order = ChangeTrackerTestDataFactory.CreateOrder();
            changeTracker.Track(order, EntityState.Added);
            orders.Add(order);
        }

        for (int i = 0; i < orders.Count; i++)
        {
            for (int j = 0; j < productsToAdd; j++)
            {
                orders[i].AddProduct(Guid.NewGuid(), $"Product {j + 1}", 10);
            }
        }

        for (int i = 0; i < rootEntitiesToModify; i++)
        {
            orders[i].UpdateAddress("Street 2", "City 2", "State 2", "ZipCode 2");
        }

        for (int i = 0; i < removeEntities; i++)
        {
            changeTracker.ChangeState(orders[i], EntityState.Deleted);
        }

        // act
        changeTracker.AcceptChanges();

        // assert
        var changes = changeTracker.FetchChanges();
        changeTracker.Entries.Count.Should().Be(totalEntities);
        changes.AddedEntities.Count.Should().Be(0);
        changes.DeletedEntities.Count.Should().Be(0);
        changes.ModifiedEntities.Count.Should().Be(0);
    }

    [Fact]
    public void FetchChanges_ShouldTracksEntityWithParent_AfterChangesAccepted()
    {
        // Arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = ChangeTrackerTestDataFactory.CreateModelBuilderForOrder();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);
        var mockDynamoSharpContext = new Mock<IDynamoSharpContext>();
        mockDynamoSharpContext.SetupGet(x => x.ChangeTracker).Returns(changeTracker);
        mockDynamoSharpContext.SetupGet(x => x.ModelBuilder).Returns(modelBuilder);
        var dynamoDbSet = new DynamoDbSet<Order>(mockDynamoSharpContext.Object);
        var entity = ChangeTrackerTestDataFactory.CreateOrder(Guid.NewGuid(), productCount: 1);
        changeTracker.Track(entity, EntityState.Added);
        changeTracker.AcceptChanges();
        entity.AddProduct(Guid.NewGuid(), "Product 2", 10, 20);
        entity.AddProduct(Guid.NewGuid(), "Product 3", 20, 30);

        // Act
        var changes = changeTracker.FetchChanges();

        // Assert
        changeTracker.Entries.Count.Should().Be(4);
        changes.AddedEntities.Count.Should().Be(2);
        changes.DeletedEntities.Count.Should().Be(0);
        changes.ModifiedEntities.Count.Should().Be(0);
    }

    [Fact]
    public async Task FetchChanges_AfterQuerying_OneToMany_ShouldTrackAddedChildEntity_AndModifiedRootEntity()
    {
        // Arrange
        var orders = ChangeTrackerTestDataFactory.CreateOrderDocuments();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var dynamoDbLowLevelPartiQLContext = new Mock<IDynamoDbLowLevelPartiQLContext>();
        dynamoDbLowLevelPartiQLContext
            .Setup(x => x.ExecuteStatementAsync(It.IsAny<ExecuteStatementRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecuteStatementResponse
            {
                Items = orders
            });
        var dynamoDbLowLevelContext = new Mock<IDynamoDbLowLevelContext>();
        dynamoDbLowLevelContext
            .Setup(x => x.PartiQL)
            .Returns(dynamoDbLowLevelPartiQLContext.Object);
        var dynamoDbContext = new Mock<IDynamoDbContext>();
        dynamoDbContext
            .Setup(x => x.LowLevel)
            .Returns(dynamoDbLowLevelContext.Object);

        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext.Object);
        var dynamoChangeTrackerContext = new NewEcommerceDynamoChangeTrackerContext(dynamoDbContextAdapter, tableSchema);
        dynamoChangeTrackerContext.OnModelCreating(dynamoChangeTrackerContext.ModelBuilder);
        dynamoChangeTrackerContext.Registration();
        var queryBuilder = (IQueryBuilder<NewOrder>)new Query<NewOrder>.Builder(dynamoChangeTrackerContext, tableSchema);
        var order = await queryBuilder
            .PartitionKey("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af")
            .ToEntityAsync();
        var item = new Item.Builder()
            .WithId(Guid.NewGuid())
            .WithProductName("Product X")
            .WithUnitPrice(10)
            .WithUnits(1)
            .Build();
        order?.AddItem(item);
        order?.ChangeBuyer(Guid.NewGuid());

        // Act
        var changes = dynamoChangeTrackerContext.ChangeTracker.FetchChanges();

        // Assert
        changes.AddedEntities.Count.Should().Be(1);
        changes.ModifiedEntities.Count.Should().Be(1);
        changes.DeletedEntities.Count.Should().Be(0);
    }

    [Fact]
    public async Task FetchChanges_AfterQuerying_ManyToMany_ShouldTrackAddedRelatedEntities_AndModifiedRootEntity()
    {
        // Arrange
        var actorDocument = ChangeTrackerTestDataFactory.CreateActorDocuments();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("movies")
            .Build();
        var dynamoDbLowLevelPartiQLContext = new Mock<IDynamoDbLowLevelPartiQLContext>();
        dynamoDbLowLevelPartiQLContext
            .Setup(x => x.ExecuteStatementAsync(It.IsAny<ExecuteStatementRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecuteStatementResponse
            {
                Items = actorDocument
            });
        var dynamoDbLowLevelContext = new Mock<IDynamoDbLowLevelContext>();
        dynamoDbLowLevelContext
            .Setup(x => x.PartiQL)
            .Returns(dynamoDbLowLevelPartiQLContext.Object);
        var dynamoDbContext = new Mock<IDynamoDbContext>();
        dynamoDbContext
            .Setup(x => x.LowLevel)
            .Returns(dynamoDbLowLevelContext.Object);
        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext.Object);
        var dynamoChangeTrackerContext = new MovieContext(dynamoDbContextAdapter, tableSchema);
        dynamoChangeTrackerContext.OnModelCreating(dynamoChangeTrackerContext.ModelBuilder);
        dynamoChangeTrackerContext.Registration();
        var queryBuilder = (IQueryBuilder<Actor>)new Query<Actor>.Builder(dynamoChangeTrackerContext, tableSchema);
        var actorEntity = await queryBuilder
            .PartitionKey("ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c")
            .ToEntityAsync();
        var theMatrixReloaded = new Movie("The Matrix Reloaded", 2003, "Science Fiction", 7.2f);
        actorEntity?.AddMovie(theMatrixReloaded, "Neo");
        var theMatrixRevolutions = new Movie("The Matrix Revolutions", 2003, "Science Fiction", 6.7f);
        actorEntity?.AddMovie(theMatrixRevolutions, "Neo");
        actorEntity?.Rename("Keanu Reeves");

        // Act
        var changes = dynamoChangeTrackerContext.ChangeTracker.FetchChanges();

        // Assert
        changes.AddedEntities.Count.Should().Be(2);
        changes.ModifiedEntities.Count.Should().Be(1);
        changes.DeletedEntities.Count.Should().Be(0);
    }
}
