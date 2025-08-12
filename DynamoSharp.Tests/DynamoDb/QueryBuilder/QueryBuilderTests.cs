using Amazon.DynamoDBv2.DocumentModel;
using EfficientDynamoDb;
using FluentAssertions;
using Moq;
using System.Globalization;
using System.Linq.Expressions;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.QueryBuilder;
using DynamoSharp.Tests.Contexts.Models;
using DynamoSharp.Tests.Contexts.Models.Affiliation;
using DynamoSharp.Tests.TestContexts;
using Document = EfficientDynamoDb.DocumentModel.Document;
using ExecuteStatementRequest = EfficientDynamoDb.Operations.ExecuteStatement.ExecuteStatementRequest;
using ExecuteStatementResponse = EfficientDynamoDb.Operations.ExecuteStatement.ExecuteStatementResponse;

namespace DynamoSharp.Tests.DynamoDb.QueryBuilder;

public class QueryBuilderTests
{
    [Fact]
    public void Create_ShouldReturnQueryBuilder()
    {
        // Arrange
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();

        // Act
        var queryBuilder = new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Assert
        queryBuilder.Should().NotBe(null);
        queryBuilder.Should().BeOfType<Query<Order>.Builder>();
        queryBuilder.Should().BeAssignableTo<IQueryBuilder<Order>>();
    }

    [Fact]
    public void AsNoTracking_ShouldSetAsNoTrackingToTrue()
    {
        // Arrange
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Act
        queryBuilder.AsNoTracking();

        // Assert
        queryBuilder.Query.AsNoTracking.Should().BeTrue();
    }

    [Fact]
    public void IndexName_ShouldSetIndexName()
    {
        // Arrange
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Act
        queryBuilder.IndexName("GSI1PK-GSI1SK-index");

        // Assert
        queryBuilder.Query.IndexName.Should().Be("GSI1PK-GSI1SK-index");
    }

    [Fact]
    public void Limit_ShouldSetLimit()
    {
        // Arrange
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Act
        queryBuilder.Limit(10);

        // Assert
        queryBuilder.Query.Limit.Should().Be(10);
    }

    [Fact]
    public void ConsistentRead_ShouldSetConsistentReadToTrue()
    {
        // Arrange
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Act
        queryBuilder.ConsistentRead();

        // Assert
        queryBuilder.Query.ConsistentRead.Should().BeTrue();
    }

    [Fact]
    public void ScanIndexForward_ShouldSetScanIndexForwardToFalse()
    {
        // Arrange
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Act
        queryBuilder.ScanIndexForward();

        // Assert
        queryBuilder.Query.ScanIndexForward.Should().BeFalse();
    }

    [Fact]
    public void PartitionKey_ShouldSetPartitionKeyValue()
    {
        // Arrange
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("orders")
            .WithPartitionKeyName("PK")
            .WithSortKeyName("SK")
            .Build();
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Act
        queryBuilder.PartitionKey("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af");

        // Assert
        queryBuilder.Query.PartitionKey?.AttributeName.Should().Be("PK");
        queryBuilder.Query.PartitionKey?.AttributeValue.Should().Be("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af");
    }

    [Fact]
    public void SortKey_ShouldSetSortKey()
    {
        // Arrange
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("orders")
            .WithPartitionKeyName("PK")
            .WithSortKeyName("SK")
            .Build();
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Act
        queryBuilder.SortKey(QueryOperator.Equal, "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af");

        // Assert
        queryBuilder.Query.SortKey?.AttributeName.Should().Be("SK");
        queryBuilder.Query.SortKey?.Operator.Should().Be(QueryOperator.Equal);
        queryBuilder.Query.SortKey?.AttributeValues[0].Should().Be("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af");
    }

    [Fact]
    public void Filters_ShouldAddFilter()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var filterExpected = (Expression<Func<Order, bool>>?)(x => x.BuyerId == buyerId);
        var dynamoChangeTrackerContext = new Mock<IDynamoSharpContext>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("orders")
            .WithPartitionKeyName("PK")
            .WithSortKeyName("SK")
            .Build();
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext.Object, tableSchema);

        // Act
        queryBuilder.Filters(x => x.BuyerId == buyerId);

        // Assert
        var filter = queryBuilder.Query.Filters;
        filter.Should().NotBeNull();
        filter?.ToString().Should().Be(filterExpected.ToString());
    }

    [Fact]
    public async Task ToEntityAsync_ShouldReturnEntityWithRelationships()
    {
        // Arrange
        var orders = QueryBuilderTestDataFactory.CreateOrderDocuments();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
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
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext, tableSchema);

        // Act
        var entity = await queryBuilder
            .PartitionKey("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af")
            .ToEntityAsync();

        // Assert
        entity.Should().NotBeNull();
        entity.Should().BeOfType<Order>();
        entity?.Id.Should().Be("85cafc37-e6bb-4693-9283-f2eaec9828af");
        entity?.BuyerId.Should().Be("68139DA0-A9F5-42FB-97FA-0585E9BCC8B1");
        entity?.Date.Should().Be(DateTime.ParseExact(orders[0]["Date"].ToString(), "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        entity?.Address.Should().NotBeNull();
        entity?.Address.Should().BeOfType<Address>();
        entity?.Address?.Street.Should().Be("Street 1");
        entity?.Address?.City.Should().Be("City 1");
        entity?.Address?.State.Should().Be("State 1");
        entity?.Address?.ZipCode.Should().Be("ZipCode 1");
        entity?.Items.Should().HaveCount(1);
        entity?.Items[0].Id.Should().Be("3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD");
        entity?.Items[0].ProductName.Should().Be("Product 1");
        entity?.Items[0].UnitPrice.Should().Be(10.99m);
        entity?.Items[0].Units.Should().Be(1);
    }

    [Fact]
    public async Task ToEntityAsync_ShouldReturnNull()
    {
        // Arrange
        var orders = new List<Document>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
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
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext, tableSchema);

        // Act
        var entity = await queryBuilder.PartitionKey("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af").ToEntityAsync();

        // Assert
        entity.Should().BeNull();
    }

    [Fact]
    public async Task ToEntityAsync_ShouldReturnEntityWithoutRelationships()
    {
        // Arrange
        var orders = QueryBuilderTestDataFactory.CreateOrderDocuments().GetRange(0, 1);
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
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
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext, tableSchema);

        // Act
        var entity = await queryBuilder.PartitionKey("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af").ToEntityAsync();

        // Assert
        entity.Should().NotBeNull();
        entity.Should().BeOfType<Order>();
        entity?.Id.Should().Be("85cafc37-e6bb-4693-9283-f2eaec9828af");
        entity?.BuyerId.Should().Be("68139DA0-A9F5-42FB-97FA-0585E9BCC8B1");
        entity?.Date.Should().Be(DateTime.ParseExact(orders[0]["Date"].ToString(), "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        entity?.Address?.Should().NotBeNull();
        entity?.Address?.Should().BeOfType<Address>();
        entity?.Address?.Street.Should().Be("Street 1");
        entity?.Address?.City.Should().Be("City 1");
        entity?.Address?.State.Should().Be("State 1");
        entity?.Address?.ZipCode.Should().Be("ZipCode 1");
        entity?.Items.Should().HaveCount(0);
    }

    [Fact]
    public async Task ToEntityAsync_ShouldReturnEntityWhenPartitionKeyAndSortKeyAreDifferent()
    {
        // Arrange
        var affiliations = QueryBuilderTestDataFactory.CreateAffiliationDocuments();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var dynamoDbLowLevelPartiQLContext = new Mock<IDynamoDbLowLevelPartiQLContext>();
        dynamoDbLowLevelPartiQLContext
            .Setup(x => x.ExecuteStatementAsync(It.IsAny<ExecuteStatementRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecuteStatementResponse
            {
                Items = affiliations
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
        var dynamoChangeTrackerContext = new AffiliationContext(dynamoDbContextAdapter, tableSchema);
        dynamoChangeTrackerContext.OnModelCreating(dynamoChangeTrackerContext.ModelBuilder);
        dynamoChangeTrackerContext.Registration();
        var queryBuilder = (IQueryBuilder<Affiliation>)new Query<Affiliation>.Builder(dynamoChangeTrackerContext, tableSchema);

        // Act
        var entity = await queryBuilder
            .PartitionKey("MERCHANT#68139DA0-A9F5-42FB-97FA-0585E9BCC8B1")
            .SortKey(QueryOperator.Equal, "Default#Other#MX#Default#Default")
            .ToEntityAsync();

        // Assert
        entity.Should().NotBeNull();
        entity.Should().BeOfType<Affiliation>();
        entity?.Id.Should().Be("85cafc37-e6bb-4693-9283-f2eaec9828af");
        entity?.MerchantId.Should().Be("68139DA0-A9F5-42FB-97FA-0585E9BCC8B1");
        entity?.TerminalId.Should().Be("9F9CE712-D241-4D06-9D12-4A3D470E8682");
        entity?.Section.Should().Be(Section.Default);
        entity?.CardBrand.Should().Be(CardBrand.Other);
        entity?.CountryOrRigion.Should().Be(CountryOrRigion.MX);
        entity?.Bank.Should().Be(Bank.Default);
        entity?.Type.Should().Be(AffiliationType.Default);
        entity?.Percentage.Should().Be(10.99f);
        entity?.CreatedAt.Should().Be(DateTime.ParseExact(affiliations[0]["CreatedAt"].ToString(), "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
    }

    [Fact]
    public async Task ToListAsync_ShouldReturnListOfEntities()
    {
        // Arrange
        var orders = QueryBuilderTestDataFactory.CreateOrderDocumentsForListTest();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("orders")
            .AddGlobalSecondaryIndex(new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK"))
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
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext, tableSchema);

        // Act
        var entities = await queryBuilder
            .IndexName("GSI1PK-GSI1SK-index")
            .PartitionKey("ORDER")
            .SortKey(QueryOperator.Equal, "ORDER")
            .ToListAsync(default);

        // Assert
        entities.Should().NotBeNull();
        entities.Should().BeOfType<List<Order>>();
        entities.Should().HaveCount(2);
        entities[0].Id.Should().Be("85cafc37-e6bb-4693-9283-f2eaec9828af");
        entities[0].BuyerId.Should().Be("68139DA0-A9F5-42FB-97FA-0585E9BCC8B1");
        entities[0].Date.Should().Be(DateTime.ParseExact(orders[0]["Date"].ToString(), "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        entities[0].Address?.Should().NotBeNull();
        entities[0].Address?.Should().BeOfType<Address>();
        entities[0].Address?.Street.Should().Be("Street 1");
        entities[0].Address?.City.Should().Be("City 1");
        entities[0].Address?.State.Should().Be("State 1");
        entities[0].Address?.ZipCode.Should().Be("ZipCode 1");
        entities[1].Id.Should().Be("85cafc37-e6bb-4693-9283-f2eaec9828af");
        entities[1].BuyerId.Should().Be("68139DA0-A9F5-42FB-97FA-0585E9BCC8B1");
        entities[1].Date.Should().Be(DateTime.ParseExact(orders[1]["Date"].ToString(), "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        entities[1].Address.Should().NotBeNull();
        entities[1].Address.Should().BeOfType<Address>();
        entities[1].Address?.Street.Should().Be("Street 1");
        entities[1].Address?.City.Should().Be("City 1");
        entities[1].Address?.State.Should().Be("State 1");
        entities[1].Address?.ZipCode.Should().Be("ZipCode 1");
    }

    [Fact]
    public async Task ToListAsync_ShouldReturnEmptyList()
    {
        // Arrange
        var orders = new List<Document>();
        var tableSchema = new TableSchema.Builder()
            .WithTableName("orders")
            .AddGlobalSecondaryIndex(new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK"))
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
        var queryBuilder = (IQueryBuilder<Order>)new Query<Order>.Builder(dynamoChangeTrackerContext, tableSchema);

        // Act
        var entities = await queryBuilder
            .IndexName("GSI1PK-GSI1SK-index")
            .PartitionKey("ORDER")
            .SortKey(QueryOperator.Equal, "ORDER")
            .ToListAsync(default);

        // Assert
        entities.Should().NotBeNull();
        entities.Should().BeOfType<List<Order>>();
        entities.Should().HaveCount(0);
    }
}
