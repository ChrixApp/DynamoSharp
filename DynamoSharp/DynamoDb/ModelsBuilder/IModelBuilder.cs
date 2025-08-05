using System.Collections.Concurrent;

namespace DynamoSharp.DynamoDb.ModelsBuilder;

public interface IModelBuilder
{
    ConcurrentDictionary<Type, IEntityTypeBuilder> Entities { get; }

    EntityTypeBuilder<TEntity> Entity<TEntity>() where TEntity : class;
}