using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace DynamoSharp.ChangeTracking;

public interface IChangeTracker
{
    ReadOnlyDictionary<int, EntityChangeTracker> Entries { get; }

    void AcceptChanges();
    (IImmutableList<EntityChangeTracker> AddedEntities, IImmutableList<EntityChangeTracker> ModifiedEntities, IImmutableList<EntityChangeTracker> DeletedEntities) FetchChanges();
    void Track(object entity, EntityState state);
    void Track(object entity, EntityState state, object parentEntity);
    void Track(EntityChangeTracker entityChangeTracker);
    void ChangeState(object entity, EntityState state);
    void Untrack(object entity);
}