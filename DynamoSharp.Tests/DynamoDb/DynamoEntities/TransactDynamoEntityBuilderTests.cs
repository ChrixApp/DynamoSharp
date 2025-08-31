using DynamoSharp.DynamoDb.DynamoEntities;
using DynamoSharp.Tests.Contexts.Models.Affiliation;
using FluentAssertions;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Tests.DynamoDb.DynamoEntities;

public class TransactDynamoEntityBuilderTests
{
    [Fact]
    public void BuildModifiedEntity_ShouldReturnDynamoModifiedEntity()
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, merchantId) = TransactDynamoEntityBuilderTestDataFactory.CreateAffiliationContextForSingleEntity();
        var transactionDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var (_, modifiedEntities, _) = changeTracker.FetchChanges();
        var entity = (Affiliation)modifiedEntities[0].Entity;

        // act
        var dynamoModifiedEntity = transactionDynamoEntityBuilder.BuildModifiedEntity(modifiedEntities[0]);

        // assert
        dynamoModifiedEntity.Children().Should().HaveCount(3);
        dynamoModifiedEntity.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
        partitionKey?.ToString().Should().Be(merchantId.ToString());
        dynamoModifiedEntity.TryGetValue(tableSchema.SortKeyName, out var sortKey).Should().Be(true);
        sortKey?.ToString().Should().StartWith($"{Section.Default}#{CardBrand.Other}#{CountryOrRigion.US}#{Bank.Default}#{AffiliationType.Default}");
        dynamoModifiedEntity.TryGetValue("CountryOrRigion", out var countryOrRigion).Should().Be(true);
        countryOrRigion?.ToString().Should().Be(entity.CountryOrRigion.ToString());
    }

    [Theory]
    [InlineData(8)]
    [InlineData(13)]
    [InlineData(26)]
    public void BuildModifiedEntity_ShouldReturnDynamoModifiedEntityWithVersion(int version)
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, merchantId) = TransactDynamoEntityBuilderTestDataFactory.CreateAffiliationContextForSingleEntity(version);
        var transactionDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var (_, modifiedEntities, _) = changeTracker.FetchChanges();
        var entity = (Affiliation)modifiedEntities[0].Entity;

        // act
        var dynamoModifiedEntity = transactionDynamoEntityBuilder.BuildModifiedEntity(modifiedEntities[0]);

        // assert
        dynamoModifiedEntity.Children().Should().HaveCount(4);
        dynamoModifiedEntity.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
        partitionKey?.ToString().Should().Be(merchantId.ToString());
        dynamoModifiedEntity.TryGetValue(tableSchema.SortKeyName, out var sortKey).Should().Be(true);
        sortKey?.ToString().Should().StartWith($"{Section.Default}#{CardBrand.Other}#{CountryOrRigion.US}#{Bank.Default}#{AffiliationType.Default}");
        dynamoModifiedEntity.TryGetValue("CountryOrRigion", out var countryOrRigion).Should().Be(true);
        countryOrRigion?.ToString().Should().Be(entity.CountryOrRigion.ToString());
        dynamoModifiedEntity.TryGetValue(tableSchema.VersionName, out var versionProperty).Should().Be(true);
        ((JValue?)versionProperty)?.Value<int>().Should().Be(version);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void CreateDynamoAddedEntities_WithPrefixPrimaryKey_ShouldReturnEntities(int totalAffiliations)
    {
        // arrange
        var (tableSchema, changeTracker, modelBuilder, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithPrefixPrimaryKey(totalAffiliations);
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var changes = changeTracker.FetchChanges();
        var dynamoAffiliations = new List<JObject>();

        // act
        foreach (var entityChangeTracker in changes.AddedEntities)
        {
            var entity = transactDynamoEntityBuilder.BuildAddedEntity(entityChangeTracker);
            dynamoAffiliations.Add(entity);
        }

        // assert
        dynamoAffiliations.Count.Should().Be(totalAffiliations);
        foreach (var dynamoAffiliation in dynamoAffiliations)
        {
            dynamoAffiliation.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
            partitionKey?.ToString().Should().Be($"MERCHANT#{merchantId}");
            dynamoAffiliation.TryGetValue(tableSchema.SortKeyName, out var sortKey).Should().Be(true);
            sortKey?.ToString().Should().StartWith($"SECTION#{Section.Default}#CARDBRAND#{CardBrand.Other}#COUNTRYORREGION#{CountryOrRigion.MX}#BANK#{Bank.Default}#TYPE#{AffiliationType.Default}");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void CreateDynamoAddedEntities_WithPrimaryKey_ShouldReturnEntities(int totalAffiliations)
    {
        // arrange
        var (tableSchema, changeTracker, modelBuilder, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithPrimaryKey(totalAffiliations);
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var changes = changeTracker.FetchChanges();
        var dynamoAffiliations = new List<JObject>();

        // act
        foreach (var entityChangeTracker in changes.AddedEntities)
        {
            var entity = transactDynamoEntityBuilder.BuildAddedEntity(entityChangeTracker);
            dynamoAffiliations.Add(entity);
        }

        // assert
        dynamoAffiliations.Count.Should().Be(totalAffiliations);
        foreach (var dynamoAffiliation in dynamoAffiliations)
        {
            dynamoAffiliation.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
            partitionKey?.ToString().Should().Be(merchantId.ToString());
            dynamoAffiliation.TryGetValue(tableSchema.SortKeyName, out var sortKey).Should().Be(true);
            sortKey?.ToString().Should().StartWith($"{Section.Default}#{CardBrand.Other}#{CountryOrRigion.MX}#{Bank.Default}#{AffiliationType.Default}");
        }
    }

    [Fact]
    public void GetAddedEntities_ShouldReturnEntityWithPk()
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextForSingleEntity();
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = transactDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

        // assert
        entity.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
        partitionKey?.ToString().Should().Be(merchantId.ToString());
    }

    [Theory]
    [InlineData("MERCHANT")]
    [InlineData("MERCHANT_ID")]
    public void GetAddedEntities_ShouldReturnEntityWithPrefixPk(string prefixPk)
    {
        // arrange
        var (tableSchema, changeTracker, modelBuilder, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithPrefixPrimaryKey(1, prefixPk);
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = transactDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

        // assert
        entity.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
        partitionKey?.ToString().Should().Be(string.Format("{0}#{1}", prefixPk, merchantId.ToString()));
    }

    [Fact]
    public void GetAddedEntities_ShouldReturnEntityWithSk()
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithSortKey();
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = transactDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

        // assert
        entity.TryGetValue(tableSchema.SortKeyName, out var sortKey).Should().Be(true);
        sortKey?.ToString().Should().Be(merchantId.ToString());
    }

    [Theory]
    [InlineData("MERCHANT")]
    [InlineData("MERCHANT_ID")]
    public void GetAddedEntities_ShouldReturnEntityWithPrefixSk(string prefixPk)
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithPrefixSortKey(prefixPk);
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = transactDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

        // assert
        entity.TryGetValue(tableSchema.SortKeyName, out var sortKey).Should().Be(true);
        sortKey?.ToString().Should().Be(string.Format("{0}#{1}", prefixPk, merchantId.ToString()));
    }

    [Theory]
    [InlineData("GSI1PK")]
    [InlineData("GSI2PK")]
    public void GetAddedEntities_ShouldReturnEntityWithGsiPk(string gsiPkName)
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, terminalId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithGsiPk(gsiPkName);
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = transactDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

        // assert
        entity.TryGetValue(gsiPkName, out var partitionKey).Should().Be(true);
        partitionKey?.ToString().Should().Be(terminalId.ToString());
    }

    [Theory]
    [InlineData("GSI1SK")]
    [InlineData("GSI2SK")]
    public void GetAddedEntities_ShouldReturnEntityWithGsiSk(string gsiSkName)
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, terminalId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithGsiSk(gsiSkName);
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, modelBuilder);
        var result = changeTracker.FetchChanges();

        // act
        var entity = transactDynamoEntityBuilder.BuildAddedEntity(result.AddedEntities[0]);

        // assert
        entity.TryGetValue(gsiSkName, out var sortKey).Should().Be(true);
        sortKey?.ToString().Should().Be(terminalId.ToString());
    }

    [Fact]
    public void GetAddedEntities_ShouldReturnEntitiesWithOneToMany()
    {
        // arrange
        var result = DynamoEntityBuilderTestDataFactory.CreateOrderContextWithOneToMany();
        var tableSchema = result.TableSchema;
        var changeTracker = result.ChangeTracker;
        var order = result.Order;
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, result.ModelBuilder);
        var changes = changeTracker.FetchChanges();
        var addedEntities = changes.AddedEntities;
        var dynamoAddedEntities = new List<JObject>();

        // act
        foreach (var addedEntity in addedEntities)
        {
            var dynamoAddedEntity = transactDynamoEntityBuilder.BuildAddedEntity(addedEntity);
            dynamoAddedEntities.Add(dynamoAddedEntity);
        }

        // assert
        dynamoAddedEntities.Count.Should().Be(3);
        var rootEntity = dynamoAddedEntities.First(e =>
        {
            return e.Value<string>(tableSchema.PartitionKeyName)!.Contains("ORDER") &&
                e.Value<string>(tableSchema.PartitionKeyName)!.Contains("DATE") &&
                e.Value<string>(tableSchema.SortKeyName)!.Contains("ORDER") &&
                e.Value<string>(tableSchema.SortKeyName)!.Contains("BUYER");
        });
        rootEntity.TryGetValue(tableSchema.PartitionKeyName, out var rootEntityPartitionKey).Should().Be(true);
        rootEntityPartitionKey?.ToString().Should().Be($"ORDER#{order.Id}#DATE#{order.Date.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK")}");
        rootEntity.TryGetValue(tableSchema.SortKeyName, out var rootEntitySortKey).Should().Be(true);
        rootEntitySortKey?.ToString().Should().Be($"ORDER#{order.Id}#BUYER#{order.BuyerId}");

        var oneToManyEntities = dynamoAddedEntities.Where(e =>
        {
            if (!e.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey)) return false;
            var isStartsWithOrder = partitionKey.ToString().StartsWith("ORDER#");

            if (!e.TryGetValue(tableSchema.SortKeyName, out var sortKey)) return false;
            var isStartsWithOrderItem = sortKey.ToString().StartsWith("ORDERITEM#");

            return isStartsWithOrder && isStartsWithOrderItem;
        });
        oneToManyEntities.Count().Should().Be(2);
        foreach (var oneToManyEntity in oneToManyEntities)
        {
            oneToManyEntity.TryGetValue(tableSchema.PartitionKeyName, out var oneToManyEntityPartitionKey).Should().Be(true);
            oneToManyEntityPartitionKey?.ToString().Should().Be($"ORDER#{order.Id}#DATE#{order.Date.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK")}");

            var orderItem = order.Items.First(oi => oneToManyEntity[tableSchema.SortKeyName]!.ToString().Contains(oi.Id.ToString()));
            oneToManyEntity.TryGetValue(tableSchema.SortKeyName, out var oneToManyEntitySortKey).Should().Be(true);
            oneToManyEntitySortKey?.ToString().Should().Be($"ORDERITEM#{orderItem.Id}#UNITPRICE#{orderItem.UnitPrice}");
        }
    }

    [Fact]
    public void GetAddedEntities_ShouldReturnEntitiesWithoutOneToMany()
    {
        // arrange
        var result = DynamoEntityBuilderTestDataFactory.CreateOrderContextWithoutOneToMany();
        var tableSchema = result.TableSchema;
        var changeTracker = result.ChangeTracker;
        var order = result.Order;
        var transactDynamoEntityBuilder = new TransactDynamoEntityBuilder(tableSchema, result.ModelBuilder);
        var changes = changeTracker.FetchChanges();
        var addedEntities = changes.AddedEntities;
        var dynamoAddedEntities = new List<JObject>();

        // act
        foreach (var addedEntity in addedEntities)
        {
            var dynamoAddedEntity = transactDynamoEntityBuilder.BuildAddedEntity(addedEntity);
            dynamoAddedEntities.Add(dynamoAddedEntity);
        }

        // assert
        dynamoAddedEntities.Count.Should().Be(1);
        var rootEntity = dynamoAddedEntities.First(e =>
        {
            var containsPartitionKey = e.ContainsKey(tableSchema.PartitionKeyName);
            var containsSortKey = e.ContainsKey(tableSchema.SortKeyName);
            return containsPartitionKey && containsSortKey;
        });
        rootEntity.TryGetValue(tableSchema.PartitionKeyName, out var rootEntityPartitionKey).Should().Be(true);
        rootEntityPartitionKey?.ToString().Should().Be($"ORDER#{order.Id}#DATE#{order.Date.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK")}");
        rootEntity.TryGetValue(tableSchema.SortKeyName, out var rootEntitySortKey).Should().Be(true);
        rootEntitySortKey?.ToString().Should().Be($"ORDER#{order.Id}#BUYER#{order.BuyerId}");
    }
}
