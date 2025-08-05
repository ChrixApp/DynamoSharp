using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Operations.BatchWriteItem;
using EfficientDynamoDb.Operations.TransactWriteItems;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Converters.Entities;

public interface IEntityConverter
{
    List<BatchWriteOperation> DocumentsToBatchWriteDeleteRequests(List<Document> documents);
    List<BatchWriteOperation> DocumentsToBatchWritePutRequests(List<Document> documents);
    List<TransactWriteItem> DocumentsToTransactDeleteWriteItems(List<Document> documents);
    List<TransactWriteItem> DocumentsToTransactPutWriteItems(List<Document> documents);
    List<TransactWriteItem> DocumentsToTransactUpdateWriteItems(List<Document> documents);
    List<Document> JsonListToDocuments(List<JObject> entitiesJson);
}