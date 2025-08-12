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
        var tableSchema = DynamoSharpContextTestDataFactory.GetTableSchema("orders");
        var batchWriteItemResponse = DynamoSharpContextTestDataFactory.GetBatchWriteItemResponse();
        var dynamoDbLowLevelContext = DynamoSharpContextTestDataFactory.GetMockDynamoDbLowLevelContext(batchWriteItemResponse);
        var dynamoDbContext = DynamoSharpContextTestDataFactory.GetMockDynamoDbContext(dynamoDbLowLevelContext);
        var dynamoChangeTrackerContext = DynamoSharpContextTestDataFactory.GetEskaContext(dynamoDbContext.Object, tableSchema);

        // act
        var batchWriteAsyncTask = dynamoChangeTrackerContext.BatchWriter.SaveChangesAsync();

        // assert
        batchWriteAsyncTask.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Fact]
    public void TransactionWriteAsync_ShouldReturnTask()
    {
        // arrange
        var tableSchema = DynamoSharpContextTestDataFactory.GetTableSchema("orders");
        var batchWriteItemResponse = DynamoSharpContextTestDataFactory.GetBatchWriteItemResponse();
        var dynamoDbLowLevelContext = DynamoSharpContextTestDataFactory.GetMockDynamoDbLowLevelContext(batchWriteItemResponse);
        var dynamoDbContext = DynamoSharpContextTestDataFactory.GetMockDynamoDbContext(dynamoDbLowLevelContext);
        var dynamoChangeTrackerContext = DynamoSharpContextTestDataFactory.GetEskaContext(dynamoDbContext.Object, tableSchema);

        // act
        var batchWriteAsyncTask = dynamoChangeTrackerContext.TransactWriter.SaveChangesAsync();

        // assert
        batchWriteAsyncTask.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    [Fact]
    public void Query_ShouldReturnQueryBuilder()
    {
        // arrange
        var tableSchema = DynamoSharpContextTestDataFactory.GetTableSchema("orders");
        var batchWriteItemResponse = DynamoSharpContextTestDataFactory.GetBatchWriteItemResponse();
        var dynamoDbLowLevelContext = DynamoSharpContextTestDataFactory.GetMockDynamoDbLowLevelContext(batchWriteItemResponse);
        var dynamoDbContext = DynamoSharpContextTestDataFactory.GetMockDynamoDbContext(dynamoDbLowLevelContext);
        var dynamoChangeTrackerContext = DynamoSharpContextTestDataFactory.GetEskaContext(dynamoDbContext.Object, tableSchema);

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
        var tableSchema = DynamoSharpContextTestDataFactory.GetTableSchema("orders");
        var config = DynamoSharpContextTestDataFactory.GetDynamoDbContextConfig();
        var dynamoDbContext = DynamoSharpContextTestDataFactory.GetDynamoDbContext(config);
        var ecommerceDynamoChangeTrackerContext = DynamoSharpContextTestDataFactory.GetEcommerceDynamoChangeTrackerContext(dynamoDbContext, tableSchema);

        // act
        ecommerceDynamoChangeTrackerContext.OnModelCreating(ecommerceDynamoChangeTrackerContext.ModelBuilder);

        // assert
        ecommerceDynamoChangeTrackerContext.ModelBuilder.Entities.Should().NotBeEmpty();
    }
}
