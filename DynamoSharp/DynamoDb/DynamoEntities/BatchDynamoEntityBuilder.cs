using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.DynamoDb.DynamoEntities;

public class BatchDynamoEntityBuilder : DynamoEntityBuilder
{
    private readonly TableSchema _tableSchema;
    private readonly IModelBuilder _modelBuilder;

    public BatchDynamoEntityBuilder(TableSchema tableSchema, IModelBuilder modelBuilder) : base(tableSchema, modelBuilder)
    {
        _tableSchema = tableSchema;
        _modelBuilder = modelBuilder;
    }

    public override JObject BuildModifiedEntity(EntityChangeTracker entityEntry)
    {
        var (partitionKey, sortKey) = BuildPrimaryKey(_modelBuilder, entityEntry);
        var modifiedEntity = (JObject)entityEntry.EntityAsJObject.DeepClone();
        modifiedEntity.Add(_tableSchema.PartitionKeyName, partitionKey);
        modifiedEntity.Add(_tableSchema.SortKeyName, sortKey);
        return modifiedEntity;
    }
}
