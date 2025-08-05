using EfficientDynamoDb;

namespace DynamoSharp.DynamoDb;

public class DynamoDbContextAdapter : IDynamoDbContextAdapter
{
    private readonly IDynamoDbContext _dynamoDbContext;

    public DynamoDbContextAdapter(IDynamoDbContext dynamoDbContext)
    {
        _dynamoDbContext = dynamoDbContext;
    }

    public IDynamoDbContext DynamoDbContext => _dynamoDbContext;
}