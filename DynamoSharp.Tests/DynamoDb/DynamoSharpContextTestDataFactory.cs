using Amazon.Runtime;
using Amazon.Runtime.Credentials;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.Tests.TestContexts;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using EfficientDynamoDb.Operations.BatchWriteItem;
using EfficientDynamoDb.Operations.Shared;
using EfficientDynamoDb.Operations.Shared.Capacity;
using Moq;

using BatchWriteItemEfficientRequest = EfficientDynamoDb.Operations.BatchWriteItem.BatchWriteItemRequest;
using RegionEndpoint = EfficientDynamoDb.Configs.RegionEndpoint;

namespace DynamoSharp.Tests.DynamoDb;

public static class DynamoSharpContextTestDataFactory
{
    public static TableSchema GetTableSchema(string tableName)
    {
        return new TableSchema.Builder()
            .WithTableName(tableName)
            .Build();
    }

    public static DynamoDbContextConfig GetDynamoDbContextConfig()
    {
        var mockCredentials = new BasicAWSCredentials("fake-access-key", "fake-secret-key");
        var effDdbCredentials = mockCredentials.ToCredentialsProvider();
        return new DynamoDbContextConfig(RegionEndpoint.USEast1, effDdbCredentials);
    }

    public static DynamoDbContext GetDynamoDbContext(DynamoDbContextConfig config) => new DynamoDbContext(config);

    public static EcommerceDynamoChangeTrackerContext GetEcommerceDynamoChangeTrackerContext(DynamoDbContext dynamoDbContext, TableSchema tableSchema)
    {
        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext);
        return new EcommerceDynamoChangeTrackerContext(dynamoDbContextAdapter, tableSchema);
    }

    public static DynamoSharpContext GetEskaContext(IDynamoDbContext dynamoDbContext, TableSchema tableSchema)
    {
        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext);
        return new DynamoSharpContext(dynamoDbContextAdapter, tableSchema);
    }

    public static Mock<IDynamoDbLowLevelContext> GetMockDynamoDbLowLevelContext(BatchWriteItemResponse batchWriteItemResponse)
    {
        var dynamoDbLowLevelContext = new Mock<IDynamoDbLowLevelContext>();
        dynamoDbLowLevelContext
            .Setup(x => x.BatchWriteItemAsync(It.IsAny<BatchWriteItemEfficientRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchWriteItemResponse);
        return dynamoDbLowLevelContext;
    }

    public static Mock<IDynamoDbContext> GetMockDynamoDbContext(Mock<IDynamoDbLowLevelContext> dynamoDbLowLevelContext)
    {
        var dynamoDbContext = new Mock<IDynamoDbContext>();
        dynamoDbContext.Setup(x => x.LowLevel).Returns(dynamoDbLowLevelContext.Object);
        return dynamoDbContext;
    }

    public static BatchWriteItemResponse GetBatchWriteItemResponse()
    {
        var consumedCapacity = new List<FullConsumedCapacity>();
        var itemCollectionMetrics = new Dictionary<string, ItemCollectionMetrics>();
        var unprocessedItems = new Dictionary<string, IReadOnlyList<BatchWriteOperation>>();
        return new BatchWriteItemResponse(consumedCapacity, itemCollectionMetrics, unprocessedItems);
    }
}
