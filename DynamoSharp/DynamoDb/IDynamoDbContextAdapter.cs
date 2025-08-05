using EfficientDynamoDb;

namespace DynamoSharp.DynamoDb;

public interface IDynamoDbContextAdapter
{
    public IDynamoDbContext DynamoDbContext { get; }
}