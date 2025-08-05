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

    public abstract Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
