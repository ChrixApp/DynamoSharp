using DynamoSharp.DynamoDb.Configs;
using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Operations.TransactWriteItems;

namespace DynamoSharp.Converters.Entities;

public class TransactPutItemConverter : IConverter
{
    private readonly TableSchema _tableSchema;

    public TransactPutItemConverter(TableSchema tableSchema)
    {
        _tableSchema = tableSchema;
    }

    public TransactWriteItem Convert(Document document)
    {
        return new TransactWriteItem(new TransactPutItem
        {
            TableName = _tableSchema.TableName,
            Item = document,
            ConditionExpression = $"attribute_not_exists({_tableSchema.PartitionKeyName}) AND attribute_not_exists({_tableSchema.SortKeyName})"
        });
    }
}
