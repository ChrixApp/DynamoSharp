using EfficientDynamoDb;
using EfficientDynamoDb.Operations.BatchWriteItem;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using DynamoSharp.ChangeTracking;
using DynamoSharp.Converters.Entities;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.DynamoEntities;
using DynamoSharp.DynamoDb.ModelsBuilder;

namespace DynamoSharp.DynamoDb.Writers;

public class BatchWriter : BaseWriter
{
    private readonly IEntityConverter _entityConverter;
    private readonly IDynamoDbContext _dynamoDbContext;
    private readonly TableSchema _tableSchema;
    private readonly IChangeTracker _changeTracker;
    private readonly IDynamoEntityBuilder _dynamoEntityBuilder;

    public BatchWriter(
        IEntityConverter entityConverter,
        IDynamoDbContext dynamoDbContext,
        TableSchema tableSchema,
        IModelBuilder modelBuilder,
        IChangeTracker changeTracker)
    {
        _entityConverter = entityConverter;
        _dynamoDbContext = dynamoDbContext;
        _tableSchema = tableSchema;
        _changeTracker = changeTracker;
        _dynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
    }
    
    public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = _changeTracker.FetchChanges();

        if (!AnyEntity(changes.AddedEntities, changes.ModifiedEntities, changes.DeletedEntities)) return;

        var dynamoAddedEntities = ConvertToJObject(changes.AddedEntities, EntityState.Added);
        var dynamoModifiedEntities = ConvertToJObject(changes.ModifiedEntities, EntityState.Modified);
        var dynamoDeletedEntities = ConvertToJObject(changes.DeletedEntities, EntityState.Deleted);
        var batchWriteItemRequest = CreaBatchWriteItemRequest(
            dynamoAddedEntities,
            dynamoModifiedEntities,
            dynamoDeletedEntities);
        await _dynamoDbContext.LowLevel.BatchWriteItemAsync(batchWriteItemRequest, cancellationToken).ConfigureAwait(false);
        _changeTracker.AcceptChanges();
    }

    private List<JObject> ConvertToJObject(IImmutableList<EntityChangeTracker> entityChangeTrackers, EntityState entityState)
    {
        var jObjects = new List<JObject>();
        foreach (var entityChangeTracker in entityChangeTrackers)
        {
            var entityBuilder = entityState switch
            {
                EntityState.Added => _dynamoEntityBuilder.BuildAddedEntity(entityChangeTracker),
                EntityState.Modified => _dynamoEntityBuilder.BuildModifiedEntity(entityChangeTracker),
                EntityState.Deleted => _dynamoEntityBuilder.BuildDeletedEntity(entityChangeTracker),
                _ => throw new NotImplementedException()
            };
            jObjects.Add(entityBuilder);
        }
        return jObjects;
    }

    private BatchWriteItemRequest CreaBatchWriteItemRequest(List<JObject> addedEntitiesDao, List<JObject> modifiedEntitiesDao, List<JObject> deletedEntitiesDao)
    {
        var addedEntitiesDoc = _entityConverter.JsonListToDocuments(addedEntitiesDao);
        var modifiedEntitiesDoc = _entityConverter.JsonListToDocuments(modifiedEntitiesDao);
        var deletedEntitiesDoc = _entityConverter.JsonListToDocuments(deletedEntitiesDao);

        var batchWriteOperation = new List<BatchWriteOperation>();
        batchWriteOperation.AddRange(_entityConverter.DocumentsToBatchWritePutRequests(addedEntitiesDoc));
        batchWriteOperation.AddRange(_entityConverter.DocumentsToBatchWritePutRequests(modifiedEntitiesDoc));
        batchWriteOperation.AddRange(_entityConverter.DocumentsToBatchWriteDeleteRequests(deletedEntitiesDoc));

        var requestItems = new Dictionary<string, IReadOnlyList<BatchWriteOperation>>
        {
            { _tableSchema.TableName, batchWriteOperation.AsReadOnly() }
        };
        return new BatchWriteItemRequest { RequestItems = requestItems };
    }
}
