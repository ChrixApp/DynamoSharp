using DynamoSharp.DynamoDb.QueryBuilder;
using DynamoSharp.Tests.Contexts.Models;
using DynamoSharp.DynamoDb;
using DynamoSharp.Exceptions;
using FluentAssertions;
using Xunit;

namespace DynamoSharp.Tests.DynamoDb;

public class DynamoSharpContextTests
{
    [Fact]
    public void BatchWriteAsync_ShouldReturnTask()
    {
        // arrange
        var tableSchema = EskaContextTestDataFactory.GetTableSchema("orders");
        var batchWriteItemResponse = EskaContextTestDataFactory.GetBatchWriteItemResponse();
        var dynamoDbLowLevelContext = EskaContextTestDataFactory.GetMockDynamoDbLowLevelContext(batchWriteItemResponse);
        var dynamoDbContext = EskaContextTestDataFactory.GetMockDynamoDbContext(dynamoDbLowLevelContext);
        var dynamoChangeTrackerContext = EskaContextTestDataFactory.GetEskaContext(dynamoDbContext.Object, tableSchema);

        // act
        var batchWriteAsyncTask = dynamoChangeTrackerContext.BatchWriter.WriteAsync();

        // assert
        batchWriteAsyncTask.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Fact]
    public void TransactionWriteAsync_ShouldReturnTask()
    {
        // arrange
        var tableSchema = EskaContextTestDataFactory.GetTableSchema("orders");
        var batchWriteItemResponse = EskaContextTestDataFactory.GetBatchWriteItemResponse();
        var dynamoDbLowLevelContext = EskaContextTestDataFactory.GetMockDynamoDbLowLevelContext(batchWriteItemResponse);
        var dynamoDbContext = EskaContextTestDataFactory.GetMockDynamoDbContext(dynamoDbLowLevelContext);
        var dynamoChangeTrackerContext = EskaContextTestDataFactory.GetEskaContext(dynamoDbContext.Object, tableSchema);

        // act
        var batchWriteAsyncTask = dynamoChangeTrackerContext.TransactWriter.WriteAsync();

        // assert
        batchWriteAsyncTask.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Fact]
    public void Query_ShouldReturnQueryBuilder()
    {
        // arrange
        var tableSchema = EskaContextTestDataFactory.GetTableSchema("orders");
        var batchWriteItemResponse = EskaContextTestDataFactory.GetBatchWriteItemResponse();
        var dynamoDbLowLevelContext = EskaContextTestDataFactory.GetMockDynamoDbLowLevelContext(batchWriteItemResponse);
        var dynamoDbContext = EskaContextTestDataFactory.GetMockDynamoDbContext(dynamoDbLowLevelContext);
        var dynamoChangeTrackerContext = EskaContextTestDataFactory.GetEskaContext(dynamoDbContext.Object, tableSchema);

        // act
        var queryBuilder = dynamoChangeTrackerContext.Query<Order>();

        // assert
        queryBuilder.Should().NotBe(null);
        queryBuilder.Should().BeOfType<Query<Order>.Builder>();
        queryBuilder.Should().BeAssignableTo<IQueryBuilder<Order>>();
    }

    [Fact]
    public void OnModelCreating_ShouldRegisterEntitiesInModelBuilder()
    {
        // arrange
        var tableSchema = EskaContextTestDataFactory.GetTableSchema("orders");
        var config = EskaContextTestDataFactory.GetDynamoDbContextConfig();
        var dynamoDbContext = EskaContextTestDataFactory.GetDynamoDbContext(config);
        var ecommerceDynamoChangeTrackerContext = EskaContextTestDataFactory.GetEcommerceDynamoChangeTrackerContext(dynamoDbContext, tableSchema);

        // act
        ecommerceDynamoChangeTrackerContext.OnModelCreating(ecommerceDynamoChangeTrackerContext.ModelBuilder);

        // assert
        ecommerceDynamoChangeTrackerContext.ModelBuilder.Entities.Should().NotBeEmpty();
    }
}
