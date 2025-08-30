using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb.ModelsBuilder;
using System.Collections.Concurrent;

namespace DynamoSharp.Tests.ChangeTracking;

public class EntityEqualityComparerTests
{
    [Fact]
    public void Equals_ThrowsIfXIsNull()
    {
        var comparer = new EntityEqualityComparer(new FakeModelBuilder());
        Assert.Throws<ArgumentNullException>(() => comparer.Equals(null, new object()));
    }

    [Fact]
    public void Equals_ThrowsIfYIsNull()
    {
        var comparer = new EntityEqualityComparer(new FakeModelBuilder());
        Assert.Throws<ArgumentNullException>(() => comparer.Equals(new object(), null));
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenTypesDiffer()
    {
        var comparer = new EntityEqualityComparer(new FakeModelBuilder());
        var obj1 = new TestEntity { Id = 1 };
        var obj2 = new AnotherEntity { Id = 1 };
        Assert.False(comparer.Equals(obj1, obj2));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenIdsAreEqual()
    {
        var comparer = new EntityEqualityComparer(new FakeModelBuilder());
        var obj1 = new TestEntity { Id = 1 };
        var obj2 = new TestEntity { Id = 1 };
        Assert.True(comparer.Equals(obj1, obj2));
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenIdsDiffer()
    {
        var comparer = new EntityEqualityComparer(new FakeModelBuilder());
        var obj1 = new TestEntity { Id = 1 };
        var obj2 = new TestEntity { Id = 2 };
        Assert.False(comparer.Equals(obj1, obj2));
    }

    [Fact]
    public void GetHashCode_UsesEntityId()
    {
        var comparer = new EntityEqualityComparer(new FakeModelBuilder());
        var obj = new TestEntity { Id = 42 };
        var expected = obj.Id.GetHashCode();
        Assert.Equal(expected, comparer.GetHashCode(obj));
    }

    [Fact]
    public void GetHashCode_FallsBackToObjectHash_WhenIdIsNull()
    {
        var comparer = new EntityEqualityComparer(new FakeModelBuilder());
        var obj = new EntityWithoutId();
        var expected = obj.GetHashCode();
        Assert.Equal(expected, comparer.GetHashCode(obj));
    }

    private class FakeModelBuilder : IModelBuilder
    {
        public ConcurrentDictionary<Type, IEntityTypeBuilder> Entities { get; }

        public FakeModelBuilder()
        {
            Entities = new ConcurrentDictionary<Type, IEntityTypeBuilder>();
            Entities[typeof(TestEntity)] = new FakeEntityTypeBuilder("Id");
            Entities[typeof(AnotherEntity)] = new FakeEntityTypeBuilder("Id");
            Entities[typeof(EntityWithoutId)] = new FakeEntityTypeBuilder(null);
        }

        public EntityTypeBuilder<TEntity> Entity<TEntity>() where TEntity : class
        {
            throw new NotImplementedException();
        }
    }

    private class FakeEntityTypeBuilder : IEntityTypeBuilder
    {
        public string IdName { get; }
        public IDictionary<string, string> PartitionKey { get; }
        public IDictionary<string, string> SortKey { get; }
        public IDictionary<string, IList<GlobalSecondaryIndex>> GlobalSecondaryIndexPartitionKey { get; }
        public IDictionary<string, IList<GlobalSecondaryIndex>> GlobalSecondaryIndexSortKey { get; }
        public IDictionary<string, Type> OneToMany { get; }
        public IDictionary<string, Type> ManyToMany { get; }
        public bool Versioning { get; } = false;

        public FakeEntityTypeBuilder(string? idName)
        {
            IdName = idName ?? "";
            PartitionKey = new Dictionary<string, string>();
            SortKey = new Dictionary<string, string>();
            GlobalSecondaryIndexPartitionKey = new Dictionary<string, IList<GlobalSecondaryIndex>>();
            GlobalSecondaryIndexSortKey = new Dictionary<string, IList<GlobalSecondaryIndex>>();
            OneToMany = new Dictionary<string, Type>();
            ManyToMany = new Dictionary<string, Type>();
        }
    }

    private class TestEntity
    {
        public int Id { get; set; }
    }

    private class AnotherEntity
    {
        public int Id { get; set; }
    }

    private class EntityWithoutId
    {
    }
}
