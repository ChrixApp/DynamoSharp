using EfficientDynamoDb;
using EfficientDynamoDb.Operations.TransactWriteItems;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using DynamoSharp.ChangeTracking;
using DynamoSharp.Converters.Entities;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.DynamoEntities;
using DynamoSharp.DynamoDb.ModelsBuilder;

namespace DynamoSharp.DynamoDb.Writers;

public class TransactWriter : BaseWriter
{
    private readonly IEntityConverter _entityConverter;
    private readonly IDynamoDbContext _dynamoDbContext;
    private readonly IChangeTracker _changeTracker;
    private readonly IDynamoEntityBuilder _dynamoEntityBuilder;

    public TransactWriter(
        IEntityConverter entityConverter,
        IDynamoDbContext dynamoDbContext,
        TableSchema tableSchema,
        IModelBuilder modelBuilder,
        IChangeTracker changeTracker)
    {
        _entityConverter = entityConverter;
        _dynamoDbContext = dynamoDbContext;
        _changeTracker = changeTracker;
        _dynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
    }

    [Obsolete("This method is deprecated. Use SaveChangesAsync method instead.")]
    public override async Task WriteAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
    }
    
    public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = _changeTracker.FetchChanges();

        if (!AnyEntity(changes.AddedEntities, changes.ModifiedEntities, changes.DeletedEntities)) return;

        var dynamoAddedEntities = ConvertToJObject(changes.AddedEntities, EntityState.Added);
        var dynamoModifiedEntities = ConvertToJObject(changes.ModifiedEntities, EntityState.Modified);
        var dynamoDeletedEntities = ConvertToJObject(changes.DeletedEntities, EntityState.Deleted);
        var transactWriteItemRequest = CreateTransactWriteItemsRequest(
            dynamoAddedEntities,
            dynamoModifiedEntities,
            dynamoDeletedEntities);
        await _dynamoDbContext.LowLevel.TransactWriteItemsAsync(transactWriteItemRequest, cancellationToken).ConfigureAwait(false);
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

    private TransactWriteItemsRequest CreateTransactWriteItemsRequest
        (List<JObject> addedEntitiesDao,
        List<JObject> modifiedEntitiesDao,
        List<JObject> deletedEntitiesDao)
    {
        var addedEntitiesDoc = _entityConverter.JsonListToDocuments(addedEntitiesDao);
        var modifiedEntitiesDoc = _entityConverter.JsonListToDocuments(modifiedEntitiesDao);
        var deletedEntitiesDoc = _entityConverter.JsonListToDocuments(deletedEntitiesDao);

        var transactWriteItems = new List<TransactWriteItem>();
        transactWriteItems.AddRange(_entityConverter.DocumentsToTransactPutWriteItems(addedEntitiesDoc));
        transactWriteItems.AddRange(_entityConverter.DocumentsToTransactUpdateWriteItems(modifiedEntitiesDoc));
        transactWriteItems.AddRange(_entityConverter.DocumentsToTransactDeleteWriteItems(deletedEntitiesDoc));

        return new TransactWriteItemsRequest
        {
            ClientRequestToken = Guid.NewGuid().ToString(),
            TransactItems = transactWriteItems
        };
    }
}
