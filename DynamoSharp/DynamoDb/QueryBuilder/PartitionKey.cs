namespace DynamoSharp.DynamoDb.QueryBuilder;

public class PartitionKey
{
    public string AttributeName { get; private set; }
    public string AttributeValue { get; private set; }

    public PartitionKey(string attributeName, string attributeValue)
    {
        AttributeName = attributeName;
        AttributeValue = attributeValue;
    }

    public static PartitionKey Create(string attributeName, string attributeValue)
    {
        return new PartitionKey(attributeName, attributeValue);
    }

    public override string ToString()
    {
        return $"{AttributeName} = {AttributeValue}";
    }
}