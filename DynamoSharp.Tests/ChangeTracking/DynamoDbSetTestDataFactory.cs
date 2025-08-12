using Amazon.Runtime.Credentials;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.Tests.TestContexts;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using RegionEndpoint = EfficientDynamoDb.Configs.RegionEndpoint;

namespace DynamoSharp.Tests.ChangeTracking;

public static class DynamoDbSetTestDataFactory
{
    public static TableSchema GetTableSchema(string tableName)
    {
        return new TableSchema.Builder()
            .WithTableName(tableName)
            .Build();
    }

    public static DynamoDbContextConfig GetDynamoDbContextConfig()
    {
        var awsSdkCredentials = DefaultAWSCredentialsIdentityResolver.GetCredentials();
        var effDdbCredentials = awsSdkCredentials.ToCredentialsProvider();
        return new DynamoDbContextConfig(RegionEndpoint.USEast1, effDdbCredentials);
    }

    public static DynamoDbContext GetDynamoDbContext(DynamoDbContextConfig config) => new DynamoDbContext(config);

    public static EcommerceDynamoChangeTrackerContext GetEcommerceDynamoChangeTrackerContext(DynamoDbContext dynamoDbContext, TableSchema tableSchema)
    {
        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext);
        return new EcommerceDynamoChangeTrackerContext(dynamoDbContextAdapter, tableSchema);
    }
}
