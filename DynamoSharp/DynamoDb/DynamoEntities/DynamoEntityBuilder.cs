using DynamoSharp.ChangeTracking;
using DynamoSharp.Converters.Jsons;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.DynamoDb.DynamoEntities;

public abstract class DynamoEntityBuilder : IDynamoEntityBuilder
{
    private readonly TableSchema _tableSchema;
    private readonly IModelBuilder _modelBuilder;

    protected DynamoEntityBuilder(TableSchema tableSchema, IModelBuilder modelBuilder)
    {
        _tableSchema = tableSchema;
        _modelBuilder = modelBuilder;
    }

    public virtual JObject BuildAddedEntity(EntityChangeTracker entityEntry)
    {
        var entityTypeBuilder = _modelBuilder.Entities[entityEntry.Entity.GetType()];
        var jsonSerializer = GetJsonSerializer(_modelBuilder, entityEntry.Entity.GetType());
        var (partitionKey, sortKey) = BuildPrimaryKey(_modelBuilder, entityEntry);
        var dynamoEntity = JObject.FromObject(entityEntry.Entity, jsonSerializer);
        dynamoEntity.Add(_tableSchema.PartitionKeyName, partitionKey);
        dynamoEntity.Add(_tableSchema.SortKeyName, sortKey);
        AddGlobalSecondaryIndexes(entityTypeBuilder, dynamoEntity);
        return dynamoEntity;
    }

    protected static JsonSerializer GetJsonSerializer(IModelBuilder modelBuilder, Type type)
    {
        var entityTypeBuilder = modelBuilder.Entities[type];
        var propertiesToIgnore = new List<string>(entityTypeBuilder.OneToMany.Keys.ToList());
        propertiesToIgnore.AddRange(entityTypeBuilder.ManyToMany.Keys.ToList());
        return JsonSerializerBuilder.Build(propertiesToIgnore);
    }

    protected static void AddGlobalSecondaryIndexes(IEntityTypeBuilder entityTypeBuilder, JObject dynamoEntity)
    {
        foreach (var gsi in entityTypeBuilder.GlobalSecondaryIndexPartitionKey)
        {
            var indexKey = BuildIndexKey(gsi.Value, dynamoEntity);
            dynamoEntity.Add(gsi.Key, indexKey);
        }

        foreach (var gsi in entityTypeBuilder.GlobalSecondaryIndexSortKey)
        {
            var indexKey = BuildIndexKey(gsi.Value, dynamoEntity);
            dynamoEntity.Add(gsi.Key, indexKey);
        }
    }

    private static string BuildIndexKey(IList<GlobalSecondaryIndex> globalSecondaryIndices, JObject entity)
    {
        var prefixWithValues = new List<string>();

        foreach (var gsi in globalSecondaryIndices)
        {
            if (!gsi.HasPath())
            {
                prefixWithValues.Add(gsi.Value);
                continue;
            }

            var property = entity.SelectToken(gsi.Path);
            var propertyValue = property?.ToString();
            if (property?.Type == JTokenType.Date)
            {
                var date = property.ToObject<DateTime>();
                propertyValue = date.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
            }

            if (string.IsNullOrWhiteSpace(gsi.Prefix))
            {
                Thrower.ThrowIfNull<PropertyValueNullException>(propertyValue, $"Property value for '{gsi.Path}' is null.");
                prefixWithValues.Add(propertyValue);
                continue;
            }

            Thrower.ThrowIfNull<PropertyValueNullException>(propertyValue, $"Property value for '{gsi.Path}' is null.");

            propertyValue = string.Format("{0}#{1}", gsi.Prefix, propertyValue);
            prefixWithValues.Add(propertyValue);
        }

        return string.Join('#', prefixWithValues);
    }

    public JObject BuildDeletedEntity(EntityChangeTracker entityEntry)
    {
        var (partitionKey, sortKey) = BuildPrimaryKey(_modelBuilder, entityEntry);
        return new JObject
        {
            { _tableSchema.PartitionKeyName, partitionKey },
            { _tableSchema.SortKeyName, sortKey }
        };
    }

    public abstract JObject BuildModifiedEntity(EntityChangeTracker entityEntry);

    protected static (string, string) BuildPrimaryKey(IModelBuilder modelBuilder, EntityChangeTracker entityEntry)
    {
        var entityTypeBuilder = modelBuilder.Entities[entityEntry.Entity.GetType()];
        var partitionKeyPath = entityEntry.IsParentEntity ?
            entityTypeBuilder.PartitionKey :
            modelBuilder.Entities[entityEntry.ParentEntity!.GetType()].PartitionKey;
        var jsonSerializer = GetJsonSerializer(modelBuilder, entityEntry.Entity.GetType());
        var entityEntryJson = JObject.FromObject(entityEntry.Entity, jsonSerializer);
        var partitionKey = entityEntry.IsParentEntity ?
            KeyBuilder.BuildKey(partitionKeyPath, entityEntryJson) :
            KeyBuilder.BuildKey(partitionKeyPath, JObject.FromObject(entityEntry.ParentEntity!, jsonSerializer));
        var sortKey = KeyBuilder.BuildKey(entityTypeBuilder.SortKey, entityEntryJson);
        return (partitionKey, sortKey);
    }
}
