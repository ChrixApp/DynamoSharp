using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.Json;
using DynamoSharp.Converters.Jsons;
using DynamoSharp.DynamoDb.ModelsBuilder;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace DynamoSharp.ChangeTracking;

public class EntityChangeTracker
{
    public object Entity { get; }
    public EntityState State { get; set; }
    public JObject OriginalEntity { get; private set; } = new();
    public JObject ModifiedProperties { get; private set; } = new();
    public object? ParentEntity { get; }
    public int? Version { get; private set; }
    private readonly ConcurrentDictionary<string, ConcurrentBag<object>> _oneToManyOriginalCollections = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<object>> _oneToManyCurrentCollections = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<object>> _manyToManyOriginalCollections = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<object>> _manyToManyCurrentCollections = new();
    private readonly IEntityTypeBuilder _entityTypeBuilder;
    private readonly JsonSerializer _jsonSerializer;

    public IReadOnlyDictionary<string, ConcurrentBag<object>> OneToManyOriginalCollections => new ReadOnlyDictionary<string, ConcurrentBag<object>>(_oneToManyOriginalCollections);
    public IReadOnlyDictionary<string, ConcurrentBag<object>> OneToManyCurrentCollections => new ReadOnlyDictionary<string, ConcurrentBag<object>>(_oneToManyCurrentCollections);
    public IReadOnlyDictionary<string, ConcurrentBag<object>> ManyToManyOriginalCollections => new ReadOnlyDictionary<string, ConcurrentBag<object>>(_manyToManyOriginalCollections);
    public IReadOnlyDictionary<string, ConcurrentBag<object>> ManyToManyCurrentCollections => new ReadOnlyDictionary<string, ConcurrentBag<object>>(_manyToManyCurrentCollections);
    public bool IsParentEntity => ParentEntity is null;
    public JObject EntityAsJObject => JObject.FromObject(Entity, _jsonSerializer);

    public EntityChangeTracker(IModelBuilder modelBuilder, object entity, EntityState state)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(modelBuilder));
        ArgumentException.ThrowIfNullOrEmpty(nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(nameof(state));

        Entity = entity;
        State = state;
        
        _entityTypeBuilder = modelBuilder.Entities[entity.GetType()];

        if (_entityTypeBuilder.Versioning)
        {
            Version = 1;
        }

        _jsonSerializer = GetJsonSerializer(_entityTypeBuilder);
        TakeSnapshot();
    }

    public EntityChangeTracker(IModelBuilder modelBuilder, object entity, EntityState state, int version)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(modelBuilder));
        ArgumentException.ThrowIfNullOrEmpty(nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(nameof(state));

        Entity = entity;
        State = state;
        Version = version;
        _entityTypeBuilder = modelBuilder.Entities[entity.GetType()];
        _jsonSerializer = GetJsonSerializer(_entityTypeBuilder);
        TakeSnapshot();
    }

    public EntityChangeTracker(IModelBuilder modelBuilder, object entity, EntityState state, object? parentEntity)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(modelBuilder));
        ArgumentException.ThrowIfNullOrEmpty(nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(nameof(state));
        ArgumentException.ThrowIfNullOrEmpty(nameof(parentEntity));

        Entity = entity;
        State = state;
        ParentEntity = parentEntity;
        _entityTypeBuilder = modelBuilder.Entities[entity.GetType()];

        if (_entityTypeBuilder.Versioning)
        {
            Version = 1;
        }

        _jsonSerializer = GetJsonSerializer(_entityTypeBuilder);
        TakeSnapshot();
    }

    public EntityChangeTracker(IModelBuilder modelBuilder, object entity, EntityState state, object? parentEntity, int version)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(modelBuilder));
        ArgumentException.ThrowIfNullOrEmpty(nameof(entity));
        ArgumentException.ThrowIfNullOrEmpty(nameof(state));
        ArgumentException.ThrowIfNullOrEmpty(nameof(parentEntity));

        Entity = entity;
        State = state;
        ParentEntity = parentEntity;
        Version = version;
        _entityTypeBuilder = modelBuilder.Entities[entity.GetType()];
        _jsonSerializer = GetJsonSerializer(_entityTypeBuilder);
        TakeSnapshot();
    }

    public void TakeSnapshot()
    {
        if (Version.HasValue && HasChanged())
        {
            Version++;
        }
        OriginalEntity = JObject.FromObject(Entity, _jsonSerializer);
    }

    public void TakeNavigationSnapshots()
    {
        foreach (var collectionName in _entityTypeBuilder.OneToMany.Keys)
        {
            var collectionProperty = Entity.GetType().GetProperty(collectionName);
            var currentCollection = (IEnumerable<object>?)collectionProperty?.GetValue(Entity) as IEnumerable<object>;
            var originalCollection = _oneToManyOriginalCollections.ContainsKey(collectionName) ? 
                _oneToManyCurrentCollections[collectionName] : new ConcurrentBag<object>();
            _oneToManyOriginalCollections[collectionName] = originalCollection;
            _oneToManyCurrentCollections[collectionName] = new ConcurrentBag<object>(currentCollection!.ToList()) ?? new ConcurrentBag<object>();
        }

        foreach (var collectionName in _entityTypeBuilder.ManyToMany.Keys)
        {
            var collectionProperty = Entity.GetType().GetProperty(collectionName);
            var currentCollection = (IEnumerable<object>?)collectionProperty?.GetValue(Entity) as IEnumerable<object>;
            var originalCollection = _manyToManyOriginalCollections.ContainsKey(collectionName) ?
                _manyToManyCurrentCollections[collectionName] : new ConcurrentBag<object>();
            _manyToManyOriginalCollections[collectionName] = originalCollection;
            _manyToManyCurrentCollections[collectionName] = new ConcurrentBag<object>(currentCollection!.ToList()) ?? new ConcurrentBag<object>();
        }
    }

    public bool HasChanged()
    {
        ModifiedProperties = new JObject();
        var newEntity = JObject.FromObject(Entity, _jsonSerializer);

        foreach (var property in OriginalEntity)
        {
            if (!JToken.DeepEquals(property.Value, newEntity[property.Key]))
            {
                ModifiedProperties.Add(property.Key, newEntity[property.Key]);
            }
        }

        return ModifiedProperties.Count > 0;
    }

    private static JsonSerializer GetJsonSerializer(IEntityTypeBuilder entityTypeBuilder)
    {
        var collectionsToIgnore = GetCollectionsToIgnore(entityTypeBuilder);
        return JsonSerializerBuilder.Build(collectionsToIgnore);
    }

    private static List<string> GetCollectionsToIgnore(IEntityTypeBuilder entityTypeBuilder)
    {
        var collectionsToIgnore = entityTypeBuilder.OneToMany.Select(otm => otm.Key).ToList();
        collectionsToIgnore.AddRange(entityTypeBuilder.ManyToMany.Select(mtm => mtm.Key).ToList());
        return collectionsToIgnore;
    }
}
