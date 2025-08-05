using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.Tests.Contexts.Models;

namespace DynamoSharp.Tests.TestContexts;

public class EcommerceContext : DynamoSharpContext
{
    public IDynamoDbSet<Order> Orders { get; private set; } = null!;

    public EcommerceContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }
}
