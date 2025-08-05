using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Exceptions;
using GlobalSecondaryIndex = DynamoSharp.DynamoDb.ModelsBuilder.GlobalSecondaryIndex;
using ModelsBuilder_GlobalSecondaryIndex = DynamoSharp.DynamoDb.ModelsBuilder.GlobalSecondaryIndex;

namespace DynamoSharp.ChangeTracking;

public class DynamoDbSet<TEntity> : IDynamoDbSet<TEntity>
{
    public IDynamoSharpContext DynamoSharpContext { get; }
    private readonly IChangeTracker _changeTracker;

    public DynamoDbSet(IDynamoSharpContext dynamoSharpContext)
    {
        DynamoSharpContext = dynamoSharpContext;
        _changeTracker = dynamoSharpContext.ChangeTracker;
        GetEntityTypeBuilder(dynamoSharpContext.ModelBuilder, typeof(TEntity));
    }

    public void Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _changeTracker.Track(entity, EntityState.Added);
    }

    public void AddRange(List<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        entities.ForEach(entity => Add(entity));
    }

    public Type GetGenericType()
    {
        throw new NotImplementedException();
    }

    public void Remove(TEntity? entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _changeTracker.ChangeState(entity, EntityState.Deleted);
    }

    public void RemoveRange(List<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        entities.ForEach(entity => Remove(entity));
    }

    private static void GetEntityTypeBuilder(IModelBuilder modelBuilder, Type entityType)
    {
        if (!modelBuilder.Entities.TryGetValue(entityType, out var entityTypeBuilder))
        {
            var defaultEntityTypeBuilder = CreatePrimaryKeyForEntity(typeof(TEntity));
            modelBuilder.Entities.TryAdd(entityType, defaultEntityTypeBuilder);
            return;
        }

        if (!entityTypeBuilder.PartitionKey.Any() &&
            !entityTypeBuilder.SortKey.Any() &&
            !entityTypeBuilder.OneToMany.Any() &&
            !entityTypeBuilder.ManyToMany.Any())
        {
            CreatePrimaryKeyForEntity(typeof(TEntity), entityTypeBuilder);
            return;
        }

        if (!entityTypeBuilder.PartitionKey.Any() &&
            !entityTypeBuilder.SortKey.Any() &&
            entityTypeBuilder.OneToMany.Any())
        {
            CreatePrimaryKeyForOneToMany(entityTypeBuilder, entityType, modelBuilder);
        }

        if (!entityTypeBuilder.PartitionKey.Any() &&
            !entityTypeBuilder.SortKey.Any() &&
            entityTypeBuilder.ManyToMany.Any())
        {
            CreatePrimaryKeyForManyToMany(entityTypeBuilder, entityType, modelBuilder);
        }
    }

    private static IEntityTypeBuilder CreatePrimaryKeyForEntity(Type entityType)
    {
        var properties = DynamoDb.DynamoSharpContext.EntityPropertiesCache.GetOrAdd(entityType, type => type.GetProperties());
        var idPropertyInfo = properties.FirstOrDefault(p => p.Name is "Id");

        if (idPropertyInfo is null) throw new IdPropertyNotFoundException($"{entityType.Name} type not contains Id");

        var defaultEntityTypeBuilder = new DefaultEntityTypeBuilder();
        defaultEntityTypeBuilder.SetPartitionKey(idPropertyInfo.Name, entityType.Name.ToUpper());
        defaultEntityTypeBuilder.SetSortKey(idPropertyInfo.Name, entityType.Name.ToUpper());
        return defaultEntityTypeBuilder;
    }

    private static void CreatePrimaryKeyForEntity(Type entityType, IEntityTypeBuilder entityTypeBuilder)
    {
        var properties = DynamoDb.DynamoSharpContext.EntityPropertiesCache.GetOrAdd(entityType, type => type.GetProperties());
        var idPropertyInfo = properties.FirstOrDefault(p => p.Name is "Id");

        if (idPropertyInfo is null) throw new IdPropertyNotFoundException($"{entityType.Name} type not contains Id");

        entityTypeBuilder.PartitionKey.Add(idPropertyInfo.Name, entityType.Name.ToUpper());
        entityTypeBuilder.SortKey.Add(idPropertyInfo.Name, entityType.Name.ToUpper());
    }

    private static void CreatePrimaryKeyForOneToMany(IEntityTypeBuilder entityTypeBuilder, Type entityType, IModelBuilder modelBuilder)
    {
        var properties = DynamoDb.DynamoSharpContext.EntityPropertiesCache.GetOrAdd(entityType, type => type.GetProperties());
        var idPropertyInfo = properties.FirstOrDefault(p => p.Name is "Id");

        if (idPropertyInfo is null) throw new IdPropertyNotFoundException($"{entityType.Name} type not contains Id");

        entityTypeBuilder.PartitionKey.Add(idPropertyInfo.Name, entityType.Name.ToUpper());
        entityTypeBuilder.SortKey.Add(idPropertyInfo.Name, entityType.Name.ToUpper());

        foreach (var oneToMany in entityTypeBuilder.OneToMany.Values.ToList())
        {
            var oneToManyProperties = DynamoDb.DynamoSharpContext.EntityPropertiesCache.GetOrAdd(oneToMany, type => type.GetProperties());
            var oneToManyIdPropertyInfo = oneToManyProperties.FirstOrDefault(p => p.Name is "Id");

            if (oneToManyIdPropertyInfo is null) throw new IdPropertyNotFoundException($"{oneToMany.Name} type not contains Id");

            modelBuilder.Entities.TryGetValue(oneToMany, out var entityTypeBuilderForOneToMany);
            var defaultEntityTypeBuilder = entityTypeBuilderForOneToMany ?? new DefaultEntityTypeBuilder();
            defaultEntityTypeBuilder.SortKey.Add(oneToManyIdPropertyInfo.Name, oneToMany.Name.ToUpper());
            modelBuilder.Entities.TryAdd(oneToMany, defaultEntityTypeBuilder);

        }
    }

    private static void CreatePrimaryKeyForManyToMany(IEntityTypeBuilder entityTypeBuilder, Type entityType, IModelBuilder modelBuilder)
    {
        var properties = DynamoDb.DynamoSharpContext.EntityPropertiesCache.GetOrAdd(entityType, type => type.GetProperties());
        var idPropertyInfo = properties.FirstOrDefault(p => p.Name is "Id");

        if (idPropertyInfo is null) throw new IdPropertyNotFoundException($"{entityType.Name} type not contains Id");

        entityTypeBuilder.PartitionKey.Add(idPropertyInfo.Name, entityType.Name.ToUpper());
        entityTypeBuilder.SortKey.Add(idPropertyInfo.Name, entityType.Name.ToUpper());

        foreach (var manyToMany in entityTypeBuilder.ManyToMany.Values.ToList())
        {
            if (!modelBuilder.Entities.TryGetValue(manyToMany, out var manyToManyEntityTypeBuilder))
            {
                manyToManyEntityTypeBuilder = new DefaultEntityTypeBuilder();
            }

            var manyToManyProperties = DynamoDb.DynamoSharpContext.EntityPropertiesCache.GetOrAdd(manyToMany, type => type.GetProperties());
            var manyToManyIdPropertyInfo = manyToManyProperties.FirstOrDefault(p => p.Name == $"{entityType.Name}Id");

            if (manyToManyIdPropertyInfo is null) throw new IdPropertyNotFoundException($"{manyToMany.Name} type not contains {entityType.Name}Id");

            if (!manyToManyEntityTypeBuilder.PartitionKey.Any())
            {
                manyToManyEntityTypeBuilder.PartitionKey.Add(manyToManyIdPropertyInfo.Name, entityType.Name.ToUpper());
                manyToManyEntityTypeBuilder.GlobalSecondaryIndexSortKey.Add("GSI1SK", new List<ModelsBuilder_GlobalSecondaryIndex> { new ModelsBuilder_GlobalSecondaryIndex($"{entityType.Name}Id", entityType.Name.ToUpper()) });
                modelBuilder.Entities.TryAdd(manyToMany, manyToManyEntityTypeBuilder);
            }
            else
            {
                manyToManyEntityTypeBuilder.SortKey.Add(manyToManyIdPropertyInfo.Name, entityType.Name.ToUpper());
                manyToManyEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.Add("GSI1PK", new List<ModelsBuilder_GlobalSecondaryIndex> { new ModelsBuilder_GlobalSecondaryIndex($"{entityType.Name}Id", entityType.Name.ToUpper()) });
                entityTypeBuilder.GlobalSecondaryIndexPartitionKey.Add("GSI1PK", new List<ModelsBuilder_GlobalSecondaryIndex> { new ModelsBuilder_GlobalSecondaryIndex("Id", entityType.Name.ToUpper()) });
                entityTypeBuilder.GlobalSecondaryIndexSortKey.Add("GSI1SK", new List<ModelsBuilder_GlobalSecondaryIndex> { new ModelsBuilder_GlobalSecondaryIndex("Id", entityType.Name.ToUpper()) });
            }
        }
    }
}
