using DynamoSharp.Converters.Jsons;
using DynamoSharp.DynamoDb.Configs;
using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Operations.BatchWriteItem;
using EfficientDynamoDb.Operations.TransactWriteItems;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Converters.Entities;

public class EntityConverter : IEntityConverter
{
    private readonly IConverter _putStrategy;
    private readonly IConverter _deleteStrategy;
    private readonly IConverter _updateStrategy;

    public EntityConverter(TableSchema tableSchema)
    {
        _putStrategy = new TransactPutItemConverter(tableSchema);
        _deleteStrategy = new TransactDeleteItemConverter(tableSchema);
        _updateStrategy = new TransactUpdateItemConverter(tableSchema);
    }

    public List<Document> JsonListToDocuments(List<JObject> entitiesJson)
    {
        var docsEfficient = new List<Document>();
        entitiesJson.ForEach(e =>
        {
            docsEfficient.Add(JObjectToDynamoDbConverter.Instance.ConvertJObjectToDocument(e));
        });
        return docsEfficient;
    }

    public List<TransactWriteItem> DocumentsToTransactPutWriteItems(List<Document> documents)
    {
        return documents.Select(doc => _putStrategy.Convert(doc)).ToList();
    }

    public List<TransactWriteItem> DocumentsToTransactDeleteWriteItems(List<Document> documents)
    {
        return documents.Select(doc => _deleteStrategy.Convert(doc)).ToList();
    }

    public List<TransactWriteItem> DocumentsToTransactUpdateWriteItems(List<Document> documents)
    {
        return documents.Select(doc => _updateStrategy.Convert(doc)).ToList();
    }

    public List<BatchWriteOperation> DocumentsToBatchWritePutRequests(List<Document> documents)
    {
        var batchWriteRequests = new List<BatchWriteOperation>();
        batchWriteRequests.AddRange(documents.Select(doc => new BatchWriteOperation(new BatchWritePutRequest(doc))));
        return batchWriteRequests;
    }

    public List<BatchWriteOperation> DocumentsToBatchWriteDeleteRequests(List<Document> documents)
    {
        var batchWriteRequests = new List<BatchWriteOperation>();
        batchWriteRequests.AddRange(documents.Select(doc => new BatchWriteOperation(new BatchWriteDeleteRequest(doc))));
        return batchWriteRequests;
    }
}
