using System.Collections.Concurrent;

namespace DynamoSharp.DynamoDb.ModelsBuilder;

public class ModelBuilder : IModelBuilder
{
    public ConcurrentDictionary<Type, IEntityTypeBuilder> Entities { get; private set; } = new();

    public EntityTypeBuilder<TEntity> Entity<TEntity>() where TEntity : class
    {
        var entityTypeBuilder = new EntityTypeBuilder<TEntity>();
        var isAdded = Entities.TryAdd(typeof(TEntity), entityTypeBuilder);
        return isAdded ? entityTypeBuilder : (EntityTypeBuilder<TEntity>)Entities[typeof(TEntity)];
    }
}
