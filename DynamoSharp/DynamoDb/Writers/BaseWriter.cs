using System.Collections.Immutable;
using DynamoSharp.ChangeTracking;

namespace DynamoSharp.DynamoDb.Writers;

public abstract class BaseWriter : IWriter
{
    protected static bool AnyEntity(
        IImmutableList<EntityChangeTracker> addedEntities,
        IImmutableList<EntityChangeTracker> modifiedEntities,
        IImmutableList<EntityChangeTracker> deletedEntities)
    {
        return addedEntities.Any() || modifiedEntities.Any() || deletedEntities.Any();
    }

    [Obsolete("This method is deprecated. Use SaveChangesAsync method instead.")]
    public abstract Task WriteAsync(CancellationToken cancellationToken = default);
    public abstract Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
