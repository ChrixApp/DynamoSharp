using Amazon.DynamoDBv2.DocumentModel;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.QueryBuilder;
using DynamoSharp.DynamoDb.QueryBuilder.PartiQL.Filter;
using DynamoSharp.Tests.DynamoDb.QueryBuilder.PartiQL.Models;
using DynamoSharp.Tests.TestContexts;
using EfficientDynamoDb;
using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Operations.ExecuteStatement;
using Moq;
using System.Linq.Expressions;

namespace DynamoSharp.Tests.DynamoDb.QueryBuilder.PartiQL;

public static class PartiQLQueryBuilderTestDataFactory
{
    public static Query<PartiQLOrder> CreateNewQueryTestData()
    {
        var orders = QueryBuilderTestDataFactory.CreateOrderDocuments();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("orders")
            .AddGlobalSecondaryIndex("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK")
            .Build();
        var dynamoDbLowLevelPartiQLContext = new Mock<IDynamoDbLowLevelPartiQLContext>();
        dynamoDbLowLevelPartiQLContext
            .Setup(x => x.ExecuteStatementAsync(It.IsAny<ExecuteStatementRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecuteStatementResponse
            {
                Items = orders
            });
        var dynamoDbLowLevelContext = new Mock<IDynamoDbLowLevelContext>();
        dynamoDbLowLevelContext
            .Setup(x => x.PartiQL)
            .Returns(dynamoDbLowLevelPartiQLContext.Object);
        var dynamoDbContext = new Mock<IDynamoDbContext>();
        dynamoDbContext
            .Setup(x => x.LowLevel)
            .Returns(dynamoDbLowLevelContext.Object);

        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext.Object);
        var dynamoChangeTrackerContext = new EcommerceDynamoChangeTrackerContext(dynamoDbContextAdapter, tableSchema);
        dynamoChangeTrackerContext.OnModelCreating(dynamoChangeTrackerContext.ModelBuilder);
        dynamoChangeTrackerContext.Registration();
        var queryBuilder = (IQueryBuilder<PartiQLOrder>)new Query<PartiQLOrder>.Builder(dynamoChangeTrackerContext, tableSchema);
        var buyerId = Guid.Parse("272ce0c2-5190-4c52-ba85-bd3034490633");
        return queryBuilder
            .IndexName("GSI1PK-GSI1SK-index")
            .PartitionKey("ORDER#3002781c-e6de-4035-a2a9-b0f7641305bd")
            .SortKey(QueryOperator.Equal, "ORDER#3002781c-e6de-4035-a2a9-b0f7641305bd")
            .Filters(x => (x.Status == PartiQLStatus.Cancelled || (x.BuyerId == buyerId)))
            .Limit(10)
            .ScanIndexForward()
            .ConsistentRead()
            .Query;
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateSortKeyContainsFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.SortKey.Contains("ORDER#"));
    }

    public static (List<Guid> buyerIds, Expression<Func<PartiQLOrder, bool>> filterLambda) CreateBuyerIdsInFilterTestData()
    {
        var buyerIds = new List<Guid>
        {
            Guid.Parse("272CE0C2-5190-4C52-BA85-BD3034490633"),
            Guid.Parse("E5D6CDBF-68FF-4BB8-9D57-3E89FBC91290"),
            Guid.Parse("A67D8AEB-FA35-4C1A-A632-233E16B0C2EE")
        };
        var filterLambda = (Expression<Func<PartiQLOrder, bool>>)(x => buyerIds.Contains(x.BuyerId));
        return (buyerIds, filterLambda);
    }

    public static (List<decimal> numbersList, Expression<Func<PartiQLOrder, bool>> filterLambda) CreateDecimalListInFilterTestData()
    {
        var numbersList = new List<decimal> { 1, 2, 3 };
        var filterLambda = (Expression<Func<PartiQLOrder, bool>>)(x => numbersList.Contains(x.Total));
        return (numbersList, filterLambda);
    }

    public static Expression<Func<PartiQLOrder, bool>> CreatePartitionKeyEqualFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.PartitionKey == "Value");
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateEqualPartitionKeyFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => "Value" == x.PartitionKey);
    }

    public static (Guid buyerId, Expression<Func<PartiQLOrder, bool>> filterLambda) CreatePartitionKeyAndGuidFilterTestData()
    {
        var buyerId = Guid.Parse("272ce0c2-5190-4c52-ba85-bd3034490633");
        var filterLambda = (Expression<Func<PartiQLOrder, bool>>)(x => x.PartitionKey == "Value" && x.BuyerId == buyerId);
        return (buyerId, filterLambda);
    }

    public static (Guid buyerId, Expression<Func<PartiQLOrder, bool>> filterLambda) CreatePartitionKeyAndEqualGuidFilterTestData()
    {
        var buyerId = Guid.Parse("272ce0c2-5190-4c52-ba85-bd3034490633");
        var filterLambda = (Expression<Func<PartiQLOrder, bool>>)(x => x.PartitionKey == "Value" && buyerId == x.BuyerId);
        return (buyerId, filterLambda);
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateGuidEqualFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.BuyerId == Guid.Parse("272ce0c2-5190-4c52-ba85-bd3034490633"));
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateEqualGuidFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => Guid.Parse("272ce0c2-5190-4c52-ba85-bd3034490633") == x.BuyerId);
    }

    public static (DateTime now, Expression<Func<PartiQLOrder, bool>> filterLambda) CreateDateTimeLessThanFilterTestData()
    {
        var now = DateTime.UtcNow;
        var filterLambda = (Expression<Func<PartiQLOrder, bool>>)(x => x.PartitionKey == "Value" && x.Date < now);
        return (now, filterLambda);
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateDateTimeNowLessThanFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.PartitionKey == "Value" && x.Date < DateTime.Now);
    }

    public static Expression<Func<PartiQLOrder, bool>> CreatePartitionKeyAndTotalGreaterThanFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.PartitionKey == "Value" && x.Total > 100);
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateEqualPartitionKeyAndTotalLessThanFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => "Value" == x.PartitionKey && 100 > x.Total);
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateAndOrFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.PartitionKey == "Value" && (x.SortKey == "Test" || x.Total == 100));
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateStatusEqualFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.Status == PartiQLStatus.Shipped);
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateEqualStatusFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => PartiQLStatus.Shipped == x.Status);
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateNumberBetweenFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.Total.Between(10, 20));
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateStringBetweenFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.SortKey.Between("A", "C"));
    }

    public static Expression<Func<PartiQLOrder, bool>> CreateDecimalInFilter()
    {
        return (Expression<Func<PartiQLOrder, bool>>)(x => x.Total.In(10, 20, 30));
    }

    public static IEnumerable<object[]> GetInlineData()
    {
        yield return new object[]
        {
            "ORDER#1", AttributeType.String
        };
        yield return new object[]
        {
            DateTime.UtcNow, AttributeType.String
        };
        yield return new object[]
        {
            93, AttributeType.Number
        };
        yield return new object[]
        {
            26.00, AttributeType.Number
        };
        yield return new object[]
        {
            Guid.NewGuid(), AttributeType.String
        };
        yield return new object[]
        {
            DateTime.UtcNow, AttributeType.String
        };
    }
}
