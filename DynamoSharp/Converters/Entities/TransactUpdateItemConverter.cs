using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Operations.Shared;
using EfficientDynamoDb.Operations.TransactWriteItems;
using System.Text;
using DynamoSharp.DynamoDb.Configs;

namespace DynamoSharp.Converters.Entities;

public class TransactUpdateItemConverter : IConverter
{
    private readonly TableSchema _tableSchema;

    public TransactUpdateItemConverter(TableSchema tableSchema)
    {
        _tableSchema = tableSchema;
    }

    public TransactWriteItem Convert(Document document)
    {
        var primaryKey = new PrimaryKey(_tableSchema.PartitionKeyName, document[_tableSchema.PartitionKeyName], _tableSchema.SortKeyName, document[_tableSchema.SortKeyName]);
        document.Remove(_tableSchema.PartitionKeyName);
        document.Remove(_tableSchema.SortKeyName);
        return new TransactWriteItem(new TransactUpdateItem
        {
            TableName = _tableSchema.TableName,
            Key = primaryKey,
            ConditionExpression = BuildConditionExpression(_tableSchema, document),
            UpdateExpression = BuildUpdateExpression(document, _tableSchema.VersionName),
            ExpressionAttributeNames = BuildExpressionAttributeNames(document),
            ExpressionAttributeValues = BuildExpressionAttributeValues(document, _tableSchema.VersionName)
        });
    }

    private static string BuildConditionExpression(TableSchema tableSchema, Document document)
    {
        var conditionExpression = new StringBuilder("attribute_exists(");
        conditionExpression.Append(tableSchema.PartitionKeyName);
        conditionExpression.Append(") AND attribute_exists(");
        conditionExpression.Append(tableSchema.SortKeyName);
        conditionExpression.Append(")");

        if (document.ContainsKey(tableSchema.VersionName))
        {
            conditionExpression.AppendFormat(" AND {0} = :{1}", tableSchema.VersionName, tableSchema.VersionName);
        }
        return conditionExpression.ToString();
    }

    private static string BuildUpdateExpression(Document document, string versionName)
    {
        var updateExpression = new StringBuilder("SET ");
        for (int i = 0; i < document.Keys.Count; i++)
        {
            if (document.Keys.ElementAt(i) == versionName) continue;

            updateExpression.Append("#");
            updateExpression.Append(document.Keys.ElementAt(i));
            updateExpression.Append(" = :");
            updateExpression.Append(document.Keys.ElementAt(i));
            if (i < document.Keys.Count - 1)
            {
                updateExpression.Append(", ");
            }
        }

        if (document.ContainsKey(versionName))
        {
            updateExpression.AppendFormat("#{0} = #{1} + :one", versionName, versionName);
        }
        return updateExpression.ToString();
    }

    private static Dictionary<string, string> BuildExpressionAttributeNames(Document document)
    {
        var expressionAttributeNames = new Dictionary<string, string>();
        foreach (var key in document.Keys)
        {
            expressionAttributeNames.Add($"#{key}", key);
        }

        return expressionAttributeNames;
    }

    private static Dictionary<string, AttributeValue> BuildExpressionAttributeValues(Document document, string versionName)
    {
        var expressionAttributeValues = new Dictionary<string, AttributeValue>();
        foreach (var key in document.Keys)
        {
            if (key == versionName) continue;
            expressionAttributeValues.Add($":{key}", document[key]);
        }

        if (document.ContainsKey(versionName))
        {
            var version = document[versionName].ToString();
            expressionAttributeValues.Add($":{versionName}", new NumberAttributeValue(version));
            expressionAttributeValues.Add(":one", new NumberAttributeValue("1"));
        }
        return expressionAttributeValues;
    }
}
