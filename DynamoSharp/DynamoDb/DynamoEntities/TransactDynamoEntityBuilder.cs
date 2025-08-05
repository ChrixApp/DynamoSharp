using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.DynamoDb.DynamoEntities;

public class TransactDynamoEntityBuilder : DynamoEntityBuilder
{
    private readonly TableSchema _tableSchema;
    private readonly IModelBuilder _modelBuilder;

    public TransactDynamoEntityBuilder(TableSchema tableSchema, IModelBuilder modelBuilder) : base(tableSchema, modelBuilder)
    {
        _tableSchema = tableSchema;
        _modelBuilder = modelBuilder;
    }

    public override JObject BuildAddedEntity(EntityChangeTracker entityEntry)
    {
        var parentType = entityEntry.IsParentEntity ? entityEntry.Entity.GetType() : entityEntry.ParentEntity?.GetType();
        var entityTypeBuilder = _modelBuilder.Entities[parentType!];
        var jsonSerializer = GetJsonSerializer(_modelBuilder, entityEntry.Entity.GetType());
        var (partitionKey, sortKey) = BuildPrimaryKey(_modelBuilder, entityEntry);
        var dynamoEntity = JObject.FromObject(entityEntry.Entity, jsonSerializer);
        dynamoEntity.Add(_tableSchema.PartitionKeyName, partitionKey);
        dynamoEntity.Add(_tableSchema.SortKeyName, sortKey);
        AddGlobalSecondaryIndexes(entityTypeBuilder, dynamoEntity);

        if (_tableSchema.HasVersioning())
        {
            dynamoEntity.Add(_tableSchema.VersionName, entityEntry.Version);
        }

        return dynamoEntity;
    }

    public override JObject BuildModifiedEntity(EntityChangeTracker entityEntry)
    {
        var (partitionKey, sortKey) = BuildPrimaryKey(_modelBuilder, entityEntry);
        var modifiedEntity = (JObject)entityEntry.ModifiedProperties.DeepClone();
        modifiedEntity.Add(_tableSchema.PartitionKeyName, partitionKey);
        modifiedEntity.Add(_tableSchema.SortKeyName, sortKey);

        var entityTypeBuilder = _modelBuilder.Entities[entityEntry.Entity.GetType()];
        if (_tableSchema.HasVersioning() && entityTypeBuilder.Versioning)
        {
            var version = entityEntry.Version;
            modifiedEntity.Add(_tableSchema.VersionName, version);
        }

        return modifiedEntity;
    }
}
