using Amazon.DynamoDBv2.DocumentModel;

namespace DynamoSharp.DynamoDb.QueryBuilder;

public class SortKey
{
    public string AttributeName { get; private set; }
    public QueryOperator Operator { get; private set; }
    public string[] AttributeValues { get; private set; }

    public SortKey(string attributeName, QueryOperator queryOperator, params string[] attributeValues)
    {
        if (queryOperator is QueryOperator.Between && attributeValues.Count() is not 2)
            throw new ArgumentException("Values must contain 2 elements for QueryOperator.Between");
        else if (queryOperator is not QueryOperator.Between && attributeValues.Count() is not 1)
            throw new ArgumentException("Values must contain 1 element for QueryOperator other than QueryOperator.Between");

        AttributeName = attributeName;
        Operator = queryOperator;
        AttributeValues = attributeValues;
    }
}