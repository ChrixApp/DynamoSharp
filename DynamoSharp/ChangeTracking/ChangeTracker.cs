using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;

namespace DynamoSharp.ChangeTracking;

public class ChangeTracker : IChangeTracker
{
    private readonly ConcurrentDictionary<int, EntityChangeTracker> _trackedEntities = new();
    private readonly EntityEqualityComparer _entityComparer;
    private readonly IModelBuilder _modelBuilder;

    public ReadOnlyDictionary<int, EntityChangeTracker> Entries => new(_trackedEntities);

    public ChangeTracker(TableSchema tableSchema, IModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder;
        _entityComparer = new EntityEqualityComparer(modelBuilder);
    }

    public void Track(object entity, EntityState state)
    {
        var entry = _trackedEntities.FirstOrDefault(e => _entityComparer.Equals(e.Value.Entity, entity)).Value;
        if (entry == null)
        {
            entry = new EntityChangeTracker(_modelBuilder, entity, state);
            if (state == EntityState.Unchanged)
            {
                entry.TakeSnapshot();
            }
            _trackedEntities.TryAdd(entity.GetHashCode(), entry);

            UpdateRelationships(entry, state);
        }
        else
        {
            entry.State = state;
        }
    }

    public void Track(object entity, EntityState state, object parentEntity)
    {
        var entry = _trackedEntities.FirstOrDefault(e => _entityComparer.Equals(e.Value.Entity, entity)).Value;
        if (entry == null)
        {
            entry = new EntityChangeTracker(_modelBuilder, entity, state, parentEntity);
            if (state == EntityState.Unchanged)
            {
                entry.TakeSnapshot();
            }
            _trackedEntities.TryAdd(entity.GetHashCode(), entry);
        }
        else
        {
            entry.State = state;
        }
    }

    public void Track(EntityChangeTracker entityChangeTracker)
    {
        var entry = _trackedEntities.FirstOrDefault(e => _entityComparer.Equals(e.Value.Entity, entityChangeTracker.Entity)).Value;
        if (entry == null)
        {
            _trackedEntities.TryAdd(entityChangeTracker.GetHashCode(), entityChangeTracker);
        }
        else
        {
            entry.State = entityChangeTracker.State;
        }

        if (entityChangeTracker.IsParentEntity) return;

        var parentEntry = _trackedEntities.FirstOrDefault(e => _entityComparer.Equals(e.Value.Entity, entityChangeTracker.ParentEntity)).Value;
        if (parentEntry is null) return;
        parentEntry.TakeNavigationSnapshots();
    }

    public void ChangeState(object entity, EntityState state)
    {
        var entry = _trackedEntities.FirstOrDefault(e => _entityComparer.Equals(e.Value.Entity, entity)).Value;
        if (entry != null)
        {
            entry.State = state;
        }
    }

    public void Untrack(object entity)
    {
        var entry = _trackedEntities.FirstOrDefault(e => _entityComparer.Equals(e.Value.Entity, entity)).Value;
        if (entry is null) return;
        _trackedEntities.Remove(entity.GetHashCode(), out _);
        if (!entry.IsParentEntity) return;
        foreach (var child in entry.OneToManyCurrentCollections.Values.SelectMany(collection => collection))
        {
            Untrack(child);
        }
    }

    public
        (IImmutableList<EntityChangeTracker> AddedEntities,
        IImmutableList<EntityChangeTracker> ModifiedEntities,
        IImmutableList<EntityChangeTracker> DeletedEntities)
    FetchChanges()
    {
        ConcurrentDictionary<int, EntityChangeTracker> addedEntities = new();
        ConcurrentDictionary<int, EntityChangeTracker> modifiedEntities = new();
        ConcurrentDictionary<int, EntityChangeTracker> deletedEntities = new();
        DetectChanges();

        foreach (var entry in _trackedEntities.Values)
        {
            if (entry.State == EntityState.Added)
            {
                addedEntities.TryAdd(entry.GetHashCode(), entry);
            }
            else if (entry.State == EntityState.Modified)
            {
                modifiedEntities.TryAdd(entry.GetHashCode(), entry);
            }
            else if (entry.State == EntityState.Deleted)
            {
                deletedEntities.TryAdd(entry.GetHashCode(), entry);
            }
        }

        return (addedEntities.Values.ToImmutableList(),
            modifiedEntities.Values.ToImmutableList(),
            deletedEntities.Values.ToImmutableList());
    }


    public void AcceptChanges()
    {
        foreach (var entry in _trackedEntities.Values)
        {
            UpdateRelationships(entry);
        }

        var deletedEntities = _trackedEntities.Where(entityKeyValue => entityKeyValue.Value.State == EntityState.Deleted).Select(keyValue => keyValue.Value);
        foreach (var entry in deletedEntities)
        {
            Untrack(entry.Entity);
        }

        var states = new[] { EntityState.Added, EntityState.Modified };
        var entries = _trackedEntities.Where(entityKeyValue => states.Contains(entityKeyValue.Value.State)).Select(keyValue => keyValue.Value);

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Modified:
                    entry.State = EntityState.Unchanged;
                    entry.TakeSnapshot();
                    break;
            }
        }
    }

    private void DetectChanges()
    {
        foreach (var entry in _trackedEntities.Values.ToList())
        {
            UpdateRelationships(entry);

            if ((entry.State == EntityState.Unchanged || entry.State == EntityState.Added) && entry.HasChanged())
            {
                entry.State = EntityState.Modified;
            }

            if (entry.State == EntityState.Deleted && entry.IsParentEntity)
            {
                foreach (var child in entry.OneToManyCurrentCollections.Values.SelectMany(collection => collection))
                {
                    Track(child, EntityState.Deleted);
                }
            }
        }
    }

    private void UpdateRelationships(EntityChangeTracker entry, EntityState entityState = EntityState.Added)
    {
        if (!entry.IsParentEntity) return;

        entry.TakeNavigationSnapshots();
        ProcessCollectionChanges(
            entry.OneToManyOriginalCollections,
            entry.OneToManyCurrentCollections,
            entityState,
            entry.Entity);
        ProcessCollectionChanges(
            entry.ManyToManyOriginalCollections,
            entry.ManyToManyCurrentCollections,
            entityState,
            entry.Entity);
    }

    private void ProcessCollectionChanges(
        IReadOnlyDictionary<string, ConcurrentBag<object>> navegationOriginalCollections,
        IReadOnlyDictionary<string, ConcurrentBag<object>> navegationCurrentCollections,
        EntityState entityState,
        object parentEntity)
    {
        foreach (var navegationOriginalCollectionKeyValue in navegationOriginalCollections)
        {
            var naveationOriginalCollection = navegationOriginalCollectionKeyValue.Value;
            var navegationCurrentCollection = navegationCurrentCollections[navegationOriginalCollectionKeyValue.Key];

            foreach (var item in navegationCurrentCollection.Except(naveationOriginalCollection, _entityComparer))
            {
                Track(item, entityState, parentEntity);
            }

            foreach (var item in naveationOriginalCollection.Except(navegationCurrentCollection, _entityComparer))
            {
                Track(item, EntityState.Deleted);
            }
        }
    }
}
