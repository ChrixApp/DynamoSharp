using EfficientDynamoDb.DocumentModel;

namespace DynamoSharp.DynamoDb.QueryBuilder.PartiQL;

public class PartiQLQuery
{
    public string Statement { get; }
    public IReadOnlyList<AttributeValue> Parameters { get; }

    public PartiQLQuery(string statement, IReadOnlyList<AttributeValue> parameters)
    {
        Statement = statement;
        Parameters = parameters;
    }
}
