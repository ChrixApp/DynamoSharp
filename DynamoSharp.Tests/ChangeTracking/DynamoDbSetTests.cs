using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.ModelsBuilder;
using Moq;
using System.Collections.Concurrent;

namespace DynamoSharp.Tests.ChangeTracking;

public class DynamoDbSetTests
{
    private readonly Mock<IDynamoSharpContext> _mockEskaContext;
    private readonly Mock<IChangeTracker> _mockChangeTracker;
    private readonly Mock<IModelBuilder> _mockModelBuilder;
    private readonly DynamoDbSet<TestEntity> _dynamoDbSet;

    public DynamoDbSetTests()
    {
        _mockEskaContext = new Mock<IDynamoSharpContext>();
        _mockChangeTracker = new Mock<IChangeTracker>();
        _mockModelBuilder = new Mock<IModelBuilder>();
        _mockModelBuilder.Setup(x => x.Entities).Returns(new ConcurrentDictionary<Type, IEntityTypeBuilder>());

        _mockEskaContext.SetupGet(x => x.ChangeTracker).Returns(_mockChangeTracker.Object);
        _mockEskaContext.SetupGet(x => x.ModelBuilder).Returns(_mockModelBuilder.Object);

        _dynamoDbSet = new DynamoDbSet<TestEntity>(_mockEskaContext.Object);
    }

    [Fact]
    public void Add_ShouldTrackEntityAsAdded()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        _dynamoDbSet.Add(entity);

        // Assert
        _mockChangeTracker.Verify(x => x.Track(entity, EntityState.Added), Times.Once);
    }

    [Fact]
    public void AddRange_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dynamoDbSet.AddRange(null!));
    }

    [Fact]
    public void AddRange_ShouldTrackEntityAsAdded()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();
        var entities = new List<TestEntity> { entity1, entity2 };

        // Act
        _dynamoDbSet.AddRange(entities);

        // Assert
        _mockChangeTracker.Verify(x => x.Track(entity1, EntityState.Added), Times.Once);
        _mockChangeTracker.Verify(x => x.Track(entity2, EntityState.Added), Times.Once);
    }

    [Fact]
    public void Add_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dynamoDbSet.Add(null!));
    }

    [Fact]
    public void Remove_ShouldChangeEntityStateToDeleted()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();
        var entities = new List<TestEntity> { entity1, entity2 };

        // Act
        _dynamoDbSet.RemoveRange(entities);

        // Assert
        _mockChangeTracker.Verify(x => x.ChangeState(entity1, EntityState.Deleted), Times.Once);
        _mockChangeTracker.Verify(x => x.ChangeState(entity2, EntityState.Deleted), Times.Once);
    }

    [Fact]
    public void Remove_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dynamoDbSet.Remove(null));
    }

    [Fact]
    public void RemoveRange_ShouldChangeEntityStateToDeleted()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        _dynamoDbSet.Remove(entity);

        // Assert
        _mockChangeTracker.Verify(x => x.ChangeState(entity, EntityState.Deleted), Times.Once);
    }

    [Fact]
    public void RemoveRange_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dynamoDbSet.RemoveRange(null!));
    }

    [Fact]
    public void GetGenericType_ShouldThrowNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => _dynamoDbSet.GetGenericType());
    }

    // Additional tests for private methods can be added here if necessary

    private class TestEntity
    {
        public string Id { get; set; } = string.Empty;
    }
}
