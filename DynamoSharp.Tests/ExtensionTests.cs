using Amazon.DynamoDBv2;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using EfficientDynamoDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RegionEndpoint = DynamoSharp.DynamoDb.Configs.RegionEndpoint;

namespace DynamoSharp.Tests;

public class ModelItem
{
    public string Message { get; set; } = string.Empty;
}

public class DynamoSharpTestContext1 : DynamoSharpContext
{
    public IDynamoDbSet<ModelItem> Items { get; private set; } = null!;

    public DynamoSharpTestContext1(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema) { }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        ModelBuilder.Entity<ModelItem>().HasPartitionKey(x => x.Message);
        ModelBuilder.Entity<ModelItem>().HasSortKey(x => x.Message);
    }
}

public class DynamoSharpTestContext2 : DynamoSharpContext
{
    public IDynamoDbSet<ModelItem> Items { get; private set; } = null!;

    public DynamoSharpTestContext2(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema) { }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        ModelBuilder.Entity<ModelItem>().HasPartitionKey(x => x.Message);
        ModelBuilder.Entity<ModelItem>().HasSortKey(x => x.Message);
    }
}

public class DynamoSharpExtensionsTests
{
    public DynamoSharpExtensionsTests()
    {
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "test-access-key");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "test-secret-key");
        Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "test-session-token");
    }

    [Fact]
    public void AddDynamoSharp_WithRegionEndpointAsEnum_ShouldRegisterServices()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        var app = builder.Build();

        Assert.NotNull(app);

        using var serviceScope = app.Services.CreateScope();
        var amazonDynamoDB = serviceScope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
        var dynamoDbContext = serviceScope.ServiceProvider.GetRequiredService<IDynamoDbContext>();
        Assert.NotNull(amazonDynamoDB);
        Assert.NotNull(dynamoDbContext);
    }

    [Fact]
    public void AddDynamoSharp_WithRegionEndpointAsString_ShouldRegisterServices()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp("us-east-1");
        var app = builder.Build();

        Assert.NotNull(app);

        using var serviceScope = app.Services.CreateScope();
        var amazonDynamoDB = serviceScope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
        var dynamoDbContext = serviceScope.ServiceProvider.GetRequiredService<IDynamoDbContext>();
        Assert.NotNull(amazonDynamoDB);
        Assert.NotNull(dynamoDbContext);
    }

    [Theory]
    [InlineData("tableNameContext1", "tableNameContext2")]
    public void MultipleTableContext_ShouldRegisterMultipleContexts(string tableNameContext1, string tableNameContext2)
    {
        var dynamoDbContext = Mock.Of<IDynamoDbContext>();
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(dynamoDbContext);
        builder.Services.AddDynamoSharpContext<DynamoSharpTestContext1>(
            new TableSchema.Builder()
                .WithTableName(tableNameContext1)
                .Build()
        );
        builder.Services.AddDynamoSharpContext<DynamoSharpTestContext2>(
            new TableSchema.Builder()
                .WithTableName(tableNameContext2)
                .Build()
        );

        var app = builder.Build();

        Assert.NotNull(app);

        using var serviceScope = app.Services.CreateScope();
        var eskaTestContext1 = serviceScope.ServiceProvider.GetRequiredService<DynamoSharpTestContext1>();
        var eskaTestContext2 = serviceScope.ServiceProvider.GetRequiredService<DynamoSharpTestContext2>();
        Assert.Equal(tableNameContext1, eskaTestContext1.TableSchema.TableName);
        Assert.Equal(tableNameContext2, eskaTestContext2.TableSchema.TableName);
        Assert.NotNull(eskaTestContext1.Items);
        Assert.NotNull(eskaTestContext2.Items);
    }
}

