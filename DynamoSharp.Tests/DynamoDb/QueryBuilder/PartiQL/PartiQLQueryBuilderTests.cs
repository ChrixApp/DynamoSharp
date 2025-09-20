using Amazon.DynamoDBv2.DocumentModel;
using DynamoSharp.DynamoDb.QueryBuilder;
using DynamoSharp.DynamoDb.QueryBuilder.PartiQL;
using EfficientDynamoDb.DocumentModel;
using FluentAssertions;

namespace DynamoSharp.Tests.DynamoDb.QueryBuilder.PartiQL;

public class PartiQLQueryBuilderTests
{
    [Fact]
    public void PartiQLQueryBuilder_CreateNewQuery_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var query = PartiQLQueryBuilderTestDataFactory.CreateNewQueryTestData();

        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
            .SelectFrom(query.TableName, query.IndexName)
            .WithPartitionKey(query.PartitionKey)
            .WithSortKey(query.SortKey)
            .WithFilters(query.Filters)
            .OrderBy(query.SortKey, !query.ScanIndexForward)
            .Build();

        // Assert
        Assert.Equal("SELECT * FROM \"orders\".\"GSI1PK-GSI1SK-index\" WHERE GSI1PK = ? AND GSI1SK = ? AND (Status = ? OR BuyerId = ?) ORDER BY GSI1SK DESC", partiQLQuery.Statement);
    }

    [Fact]
    public void PartiQLQueryBuilder_WithSortKeyBeginsWith_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var partiQLQueryBuilder = new PartiQLQueryBuilder();
        var sortKey = new SortKey("SortKey", QueryOperator.BeginsWith, "ORDER#");

        // Act
        var partiQLQuery = partiQLQueryBuilder
            .WithSortKey(sortKey)
            .Build();

        // Assert
        Assert.Equal(" AND begins_with(SortKey, ?)", partiQLQuery.Statement);
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal("ORDER#", partiQLQuery.Parameters[0].AsString());
    }

    [Theory]
    [MemberData(nameof(PartiQLQueryBuilderTestDataFactory.GetInlineData), MemberType = typeof(PartiQLQueryBuilderTestDataFactory))]
    public void PartiQLQueryBuilder_AddsSingleParameterWithExpectedAttributeType(object sortKeyValue, AttributeType attributeType)
    {
        // Arrange
        var partiQLQueryBuilder = new PartiQLQueryBuilder();
        var sortKey = new SortKey("SortKey", QueryOperator.Equal, sortKeyValue);

        // Act
        var partiQLQuery = partiQLQueryBuilder
            .WithSortKey(sortKey)
            .Build();

        // Assert
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal(attributeType, partiQLQuery.Parameters[0].Type);
    }

    [Theory]
    [InlineData(26, null, AttributeType.Number, AttributeType.Null)]
    public void PartiQLQueryBuilder_AddsTwoParametersWithExpectedAttributeTypes(object sortKeyValue1, object sortKeyValue2, AttributeType attributeType1, AttributeType attributeType2)
    {
        // Arrange
        var partiQLQueryBuilder = new PartiQLQueryBuilder();
        var sortKey = new SortKey("SortKey", QueryOperator.Between, sortKeyValue1, sortKeyValue2);

        // Act
        var partiQLQuery = partiQLQueryBuilder
            .WithSortKey(sortKey)
            .Build();

        // Assert
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal(attributeType1, partiQLQuery.Parameters[0].Type);
        Assert.Equal(attributeType2, partiQLQuery.Parameters[1].Type);
    }

    [Fact]
    public void PartiQLQueryBuilder_WithSortKeyContains_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateSortKeyContainsFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
                .WithFilters(filterLambda)
                .Build();

        // Assert
        Assert.Equal(" AND contains(SortKey, ?)", partiQLQuery.Statement);
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal("ORDER#", partiQLQuery.Parameters[0].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithBuyerIdsInFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var (buyerIds, filterLambda) = PartiQLQueryBuilderTestDataFactory.CreateBuyerIdsInFilterTestData();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
                .WithFilters(filterLambda)
                .Build();

        // Assert
        Assert.Equal(" AND BuyerId IN [?, ?, ?]", partiQLQuery.Statement);
        Assert.Equal(3, partiQLQuery.Parameters.Count);
        Assert.Equal(buyerIds[0].ToString(), partiQLQuery.Parameters[0].AsString());
        Assert.Equal(buyerIds[1].ToString(), partiQLQuery.Parameters[1].AsString());
        Assert.Equal(buyerIds[2].ToString(), partiQLQuery.Parameters[2].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithDecimalListInFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var (numbersList, filterLambda) = PartiQLQueryBuilderTestDataFactory.CreateDecimalListInFilterTestData();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
               .WithFilters(filterLambda)
               .Build();

        // Assert
        Assert.Equal(" AND Total IN [?, ?, ?]", partiQLQuery.Statement);
        Assert.Equal(3, partiQLQuery.Parameters.Count);
        Assert.Equal(numbersList[0].ToString(), partiQLQuery.Parameters[0].AsNumberAttribute().Value);
        Assert.Equal(numbersList[1].ToString(), partiQLQuery.Parameters[1].AsNumberAttribute().Value);
        Assert.Equal(numbersList[2].ToString(), partiQLQuery.Parameters[2].AsNumberAttribute().Value);
    }

    [Fact]
    public void PartiQLQueryBuilder_WithPartitionKeyEqualFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreatePartitionKeyEqualFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
               .WithFilters(filterLambda)
               .Build();

        // Assert
        Assert.Equal(" AND PartitionKey = ?", partiQLQuery.Statement);
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithEqualPartitionKeyFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateEqualPartitionKeyFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND ? = PartitionKey", partiQLQuery.Statement);
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithPartitionKeyAndGuidFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var (buyerId, filterLambda) = PartiQLQueryBuilderTestDataFactory.CreatePartitionKeyAndGuidFilterTestData();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND (PartitionKey = ? AND BuyerId = ?)", partiQLQuery.Statement);
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
        Assert.Equal(buyerId.ToString(), partiQLQuery.Parameters[1].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithPartitionKeyAndEqualGuidFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var (buyerId, filterLambda) = PartiQLQueryBuilderTestDataFactory.CreatePartitionKeyAndEqualGuidFilterTestData();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND (PartitionKey = ? AND ? = BuyerId)", partiQLQuery.Statement);
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
        Assert.Equal(buyerId.ToString(), partiQLQuery.Parameters[1].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithGuidEqualFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateGuidEqualFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND BuyerId = ?", partiQLQuery.Statement);
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal("272ce0c2-5190-4c52-ba85-bd3034490633", partiQLQuery.Parameters[0].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithEqualGuidFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateEqualGuidFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND ? = BuyerId", partiQLQuery.Statement);
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal("272ce0c2-5190-4c52-ba85-bd3034490633", partiQLQuery.Parameters[0].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithDateTimeLessThanFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var (now, filterLambda) = PartiQLQueryBuilderTestDataFactory.CreateDateTimeLessThanFilterTestData();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND (PartitionKey = ? AND Date < ?)", partiQLQuery.Statement);
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
        Assert.Equal(now.ToString("o"), partiQLQuery.Parameters[1].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithDateTimeNowLessThanFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateDateTimeNowLessThanFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND (PartitionKey = ? AND Date < ?)", partiQLQuery.Statement);
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
        Assert.Equal(DateTime.Now.ToString("o").Substring(0, 20), partiQLQuery.Parameters[1].AsString().Substring(0, 20));
        Assert.Equal(DateTime.Now.ToString("o").Substring(27), partiQLQuery.Parameters[1].AsString().Substring(27));
    }

    [Fact]
    public void PartiQLQueryBuilder_WithPartitionKeyAndTotalGreaterThanFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreatePartitionKeyAndTotalGreaterThanFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND (PartitionKey = ? AND Total > ?)", partiQLQuery.Statement);
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
        Assert.Equal(100, partiQLQuery.Parameters[1].AsNumberAttribute().ToInt());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithEqualPartitionKeyAndTotalLessThanFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateEqualPartitionKeyAndTotalLessThanFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND (? = PartitionKey AND ? > Total)", partiQLQuery.Statement);
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
        Assert.Equal(100, partiQLQuery.Parameters[1].AsNumberAttribute().ToInt());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithAndOrFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateAndOrFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND (PartitionKey = ? AND (SortKey = ? OR Total = ?))", partiQLQuery.Statement);
        Assert.Equal(3, partiQLQuery.Parameters.Count);
        Assert.Equal("Value", partiQLQuery.Parameters[0].AsString());
        Assert.Equal("Test", partiQLQuery.Parameters[1].AsString());
        Assert.Equal(100, partiQLQuery.Parameters[2].AsNumberAttribute().ToInt());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithStatusEqualFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateStatusEqualFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND Status = ?", partiQLQuery.Statement);
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal("Shipped", partiQLQuery.Parameters[0].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithEqualStatusFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateEqualStatusFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND ? = Status", partiQLQuery.Statement);
        Assert.Single(partiQLQuery.Parameters);
        Assert.Equal("Shipped", partiQLQuery.Parameters[0].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithNumberBetweenFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateNumberBetweenFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        //// Assert
        Assert.Equal(" AND Total BETWEEN ? AND ?", partiQLQuery.Statement);
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal(10, partiQLQuery.Parameters[0].AsNumberAttribute().ToInt());
        Assert.Equal(20, partiQLQuery.Parameters[1].AsNumberAttribute().ToInt());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithStringBetweenFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateStringBetweenFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND SortKey BETWEEN ? AND ?", partiQLQuery.Statement);
        Assert.Equal(2, partiQLQuery.Parameters.Count);
        Assert.Equal("A", partiQLQuery.Parameters[0].AsString());
        Assert.Equal("C", partiQLQuery.Parameters[1].AsString());
    }

    [Fact]
    public void PartiQLQueryBuilder_WithDecimalInFilter_ShouldGenerateCorrectQuery()
    {
        // Arrange
        var filterLambda = PartiQLQueryBuilderTestDataFactory.CreateDecimalInFilter();
        var partiQLQueryBuilder = new PartiQLQueryBuilder();

        // Act
        var partiQLQuery = partiQLQueryBuilder
           .WithFilters(filterLambda)
           .Build();

        // Assert
        Assert.Equal(" AND Total IN (?, ?, ?)", partiQLQuery.Statement);
        Assert.Equal(3, partiQLQuery.Parameters.Count);
        Assert.Equal(10, partiQLQuery.Parameters[0].AsNumberAttribute().ToInt());
        Assert.Equal(20, partiQLQuery.Parameters[1].AsNumberAttribute().ToInt());
        Assert.Equal(30, partiQLQuery.Parameters[2].AsNumberAttribute().ToInt());
    }
}
