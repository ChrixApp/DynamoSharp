using DynamoSharp.DynamoDb.Configs;
using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Operations.Shared;
using EfficientDynamoDb.Operations.TransactWriteItems;

namespace DynamoSharp.Converters.Entities;

public class TransactDeleteItemConverter : IConverter
{
    private readonly TableSchema _tableSchema;

    public TransactDeleteItemConverter(TableSchema tableSchema)
    {
        _tableSchema = tableSchema;
    }

    public TransactWriteItem Convert(Document document)
    {
        return new TransactWriteItem(new TransactDeleteItem
        {
            TableName = _tableSchema.TableName,
            Key = new PrimaryKey(_tableSchema.PartitionKeyName, document[_tableSchema.PartitionKeyName], _tableSchema.SortKeyName, document[_tableSchema.SortKeyName]),
            ConditionExpression = $"attribute_exists({_tableSchema.PartitionKeyName}) AND attribute_exists({_tableSchema.SortKeyName})"
        });
    }
}
