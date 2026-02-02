using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.DynamoEntities;
using DynamoSharp.Tests.Contexts.Models.Affiliation;
using DynamoSharp.Tests.Contexts.Models.Movies;
using DynamoSharp.Tests.TestContexts;
using DynamoSharp.Tests.TestContexts.Models.Ecommerce;
using EfficientDynamoDb;
using FluentAssertions;
using Newtonsoft.Json.Linq;

namespace DynamoSharp.Tests.DynamoDb.DynamoEntities;

public class DynamoEntityBuilderTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void CreateDynamoAddedEntities_WithPrefixPrimaryKey_ShouldReturnEntities(int totalAffiliations)
    {
        // arrange
        var (tableSchema, changeTracker, modelBuilder, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithPrefixPrimaryKey(totalAffiliations);
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var changes = changeTracker.FetchChanges();
        var dynamoAffiliations = new List<JObject>();

        // act
        foreach (var entityChangeTracker in changes.AddedEntities)
        {
            var entity = batchDynamoEntityBuilder.BuildAddedEntity(entityChangeTracker);
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
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var changes = changeTracker.FetchChanges();
        var dynamoAffiliations = new List<JObject>();

        // act
        foreach (var entityChangeTracker in changes.AddedEntities)
        {
            var entity = batchDynamoEntityBuilder.BuildAddedEntity(entityChangeTracker);
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
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = batchDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

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
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = batchDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

        // assert
        entity.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
        partitionKey?.ToString().Should().Be(string.Format("{0}#{1}", prefixPk, merchantId.ToString()));
    }

    [Fact]
    public void GetAddedEntities_ShouldReturnEntityWithSk()
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithSortKey();
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = batchDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

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
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = batchDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

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
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = batchDynamoEntityBuilder.BuildAddedEntity(addedEntities[0]);

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
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var result = changeTracker.FetchChanges();

        // act
        var entity = batchDynamoEntityBuilder.BuildAddedEntity(result.AddedEntities[0]);

        // assert
        entity.TryGetValue(gsiSkName, out var sortKey).Should().Be(true);
        sortKey?.ToString().Should().Be(terminalId.ToString());
    }

    [Theory]
    [InlineData("GSI1SK")]
    [InlineData("GSI2SK")]
    public void GetAddedEntities_ShouldReturnEntityWithGsiSkAsFloat(string gsiSkName)
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, terminalId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextWithGsiSkAsNumber(gsiSkName);
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var result = changeTracker.FetchChanges();

        // act
        var entity = batchDynamoEntityBuilder.BuildAddedEntity(result.AddedEntities[0]);

        // assert
        entity.TryGetValue(gsiSkName, out var sortKey).Should().Be(true);
        sortKey?.Type.Should().Be(JTokenType.Float);
    }

    [Fact]
    public void GetAddedEntities_ShouldReturnEntitiesWithOneToMany()
    {
        // arrange
        var result = DynamoEntityBuilderTestDataFactory.CreateOrderContextWithOneToMany();
        var tableSchema = result.TableSchema;
        var changeTracker = result.ChangeTracker;
        var order = result.Order;
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, result.ModelBuilder);
        var changes = changeTracker.FetchChanges();
        var addedEntities = changes.AddedEntities;
        var dynamoAddedEntities = new List<JObject>();

        // act
        foreach (var addedEntity in addedEntities) {
            var dynamoAddedEntity = batchDynamoEntityBuilder.BuildAddedEntity(addedEntity);
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
            var isStartsWithOrderItem = sortKey.ToString().StartsWith("ITEM#");

            return isStartsWithOrder && isStartsWithOrderItem;
        });
        oneToManyEntities.Count().Should().Be(2);
        foreach (var oneToManyEntity in oneToManyEntities)
        {
            oneToManyEntity.TryGetValue(tableSchema.PartitionKeyName, out var oneToManyEntityPartitionKey).Should().Be(true);
            oneToManyEntityPartitionKey?.ToString().Should().Be($"ORDER#{order.Id}#DATE#{order.Date.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK")}");

            var orderItem = order.Items.First(oi => oneToManyEntity[tableSchema.SortKeyName]!.ToString().Contains(oi.Id.ToString()));
            oneToManyEntity.TryGetValue(tableSchema.SortKeyName, out var oneToManyEntitySortKey).Should().Be(true);
            oneToManyEntitySortKey?.ToString().Should().Be($"ITEM#{orderItem.Id}#UNITPRICE#{orderItem.UnitPrice}");
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
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, result.ModelBuilder);
        var changes = changeTracker.FetchChanges();
        var addedEntities = changes.AddedEntities;
        var dynamoAddedEntities = new List<JObject>();

        // act
        foreach (var addedEntity in addedEntities)
        {
            var dynamoAddedEntity = batchDynamoEntityBuilder.BuildAddedEntity(addedEntity);
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

    [Fact]
    public void BuildDeletedEntity_ShouldReturnEntityWithPk()
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, merchantId) = DynamoEntityBuilderTestDataFactory.CreateAffiliationContextForSingleEntity();
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var (addedEntities, _, _) = changeTracker.FetchChanges();

        // act
        var entity = batchDynamoEntityBuilder.BuildDeletedEntity(addedEntities[0]);

        // assert
        entity.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
        partitionKey?.ToString().Should().Be(merchantId.ToString());
        entity.TryGetValue(tableSchema.SortKeyName, out var sortKey).Should().Be(true);
        sortKey?.ToString().Should().StartWith($"{Section.Default}#{CardBrand.Other}#{CountryOrRigion.MX}#{Bank.Default}#{AffiliationType.Default}");
    }

    [Fact]
    public void GetEntityTypeBuilder_ShouldCreatePrimaryKeyForEntity()
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var dynamoDbContextConfig = DynamoEntityBuilderTestDataFactory.GetDynamoDbContextConfig();
        var dynamoDbContext = new DynamoDbContext(dynamoDbContextConfig);
        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext);
        var ecommerceContext = new EcommerceContext(dynamoDbContextAdapter, tableSchema);
        ecommerceContext.OnModelCreating(ecommerceContext.ModelBuilder);

        // act
        ecommerceContext.Registration();

        // assert
        ecommerceContext.ModelBuilder.Entities.Count.Should().Be(1);
        var entityTypeBuilder = ecommerceContext.ModelBuilder.Entities.First().Value;
        entityTypeBuilder.PartitionKey.Should().NotBeNull();
        entityTypeBuilder.PartitionKey.First().Key.Should().Be("Id");
        entityTypeBuilder.PartitionKey.First().Value.Should().Be("ORDER");
        entityTypeBuilder.SortKey.Should().NotBeNull();
        entityTypeBuilder.SortKey.First().Key.Should().Be("Id");
        entityTypeBuilder.SortKey.First().Value.Should().Be("ORDER");
    }

    [Fact]
    public void GetEntityTypeBuilder_ShouldCreatePrimaryKeyForOneToMany()
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var dynamoDbContextConfig = DynamoEntityBuilderTestDataFactory.GetDynamoDbContextConfig();
        var dynamoDbContext = new DynamoDbContext(dynamoDbContextConfig);
        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext);
        var ecommerceOneToManyContext = new EcommerceOneToManyContext(dynamoDbContextAdapter, tableSchema);
        ecommerceOneToManyContext.OnModelCreating(ecommerceOneToManyContext.ModelBuilder);

        // act
        ecommerceOneToManyContext.Registration();

        // assert
        ecommerceOneToManyContext.ModelBuilder.Entities.Count.Should().Be(2);
        var orderEntityTypeBuilder = ecommerceOneToManyContext.ModelBuilder.Entities[typeof(Order)];
        orderEntityTypeBuilder.PartitionKey.Should().NotBeNull();
        orderEntityTypeBuilder.PartitionKey.Count().Should().Be(1);
        orderEntityTypeBuilder.PartitionKey.First().Key.Should().Be("Id");
        orderEntityTypeBuilder.PartitionKey.First().Value.Should().Be("ORDER");
        orderEntityTypeBuilder.SortKey.Should().NotBeNull();
        orderEntityTypeBuilder.SortKey.Count().Should().Be(1);
        orderEntityTypeBuilder.SortKey.First().Key.Should().Be("Id");
        orderEntityTypeBuilder.SortKey.First().Value.Should().Be("ORDER");
        orderEntityTypeBuilder.OneToMany.Should().NotBeNull();
        orderEntityTypeBuilder.OneToMany.Count().Should().Be(1);
        orderEntityTypeBuilder.OneToMany.Values.ToList()[0].Should().Be(typeof(Item));

        var orderItemEntityTypeBuilder = ecommerceOneToManyContext.ModelBuilder.Entities[typeof(Item)];
        orderItemEntityTypeBuilder.SortKey.Should().NotBeNull();
        orderItemEntityTypeBuilder.SortKey.Count().Should().Be(1);
        orderItemEntityTypeBuilder.SortKey.First().Key.Should().Be("Id");
        orderItemEntityTypeBuilder.SortKey.First().Value.Should().Be("ITEM");
    }

    [Fact]
    public void GetEntityTypeBuilder_ShouldCreatePrimaryKeyForManyToMany()
    {
        // arrange
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var dynamoDbContextConfig = DynamoEntityBuilderTestDataFactory.GetDynamoDbContextConfig();
        var dynamoDbContext = new DynamoDbContext(dynamoDbContextConfig);
        var dynamoDbContextAdapter = new DynamoDbContextAdapter(dynamoDbContext);
        var movieContext = new MovieContext(dynamoDbContextAdapter, tableSchema);
        movieContext.OnModelCreating(movieContext.ModelBuilder);

        // act
        movieContext.Registration();

        // assert
        movieContext.ModelBuilder.Entities.Count.Should().Be(3);
        var movieEntityTypeBuilder = movieContext.ModelBuilder.Entities[typeof(Movie)];
        movieEntityTypeBuilder.PartitionKey.Should().NotBeNull();
        movieEntityTypeBuilder.PartitionKey.Count().Should().Be(1);
        movieEntityTypeBuilder.PartitionKey.First().Key.Should().Be("Id");
        movieEntityTypeBuilder.PartitionKey.First().Value.Should().Be("MOVIE");
        movieEntityTypeBuilder.SortKey.Should().NotBeNull();
        movieEntityTypeBuilder.SortKey.Count().Should().Be(1);
        movieEntityTypeBuilder.SortKey.First().Key.Should().Be("Id");
        movieEntityTypeBuilder.SortKey.First().Value.Should().Be("MOVIE");
        movieEntityTypeBuilder.ManyToMany.Should().NotBeNull();
        movieEntityTypeBuilder.ManyToMany.Count().Should().Be(1);
        movieEntityTypeBuilder.ManyToMany.Values.ToList()[0].Should().Be(typeof(Performance));

        var actorEntityTypeBuilder = movieContext.ModelBuilder.Entities[typeof(Actor)];
        actorEntityTypeBuilder.PartitionKey.Should().NotBeNull();
        actorEntityTypeBuilder.PartitionKey.Count().Should().Be(1);
        actorEntityTypeBuilder.PartitionKey.First().Key.Should().Be("Id");
        actorEntityTypeBuilder.PartitionKey.First().Value.Should().Be("ACTOR");
        actorEntityTypeBuilder.SortKey.Should().NotBeNull();
        actorEntityTypeBuilder.SortKey.Count().Should().Be(1);
        actorEntityTypeBuilder.SortKey.First().Key.Should().Be("Id");
        actorEntityTypeBuilder.SortKey.First().Value.Should().Be("ACTOR");
        actorEntityTypeBuilder.ManyToMany.Should().NotBeNull();
        actorEntityTypeBuilder.ManyToMany.Count().Should().Be(1);
        actorEntityTypeBuilder.ManyToMany.Values.ToList()[0].Should().Be(typeof(Performance));
        actorEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.Should().NotBeNull();
        actorEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.Count().Should().Be(1);
        actorEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Key.Should().Be("GSI1PK");
        actorEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value[0].Path.Should().Be("Id");
        actorEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value[0].Prefix.Should().Be("ACTOR");
        actorEntityTypeBuilder.GlobalSecondaryIndexSortKey.Should().NotBeNull();
        actorEntityTypeBuilder.GlobalSecondaryIndexSortKey.Count().Should().Be(1);
        actorEntityTypeBuilder.GlobalSecondaryIndexSortKey.First().Key.Should().Be("GSI1SK");
        actorEntityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value[0].Path.Should().Be("Id");
        actorEntityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value[0].Prefix.Should().Be("ACTOR");

        var performanceEntityTypeBuilder = movieContext.ModelBuilder.Entities[typeof(Performance)];
        performanceEntityTypeBuilder.PartitionKey.Should().NotBeNull();
        performanceEntityTypeBuilder.PartitionKey.Count().Should().Be(1);
        performanceEntityTypeBuilder.PartitionKey.First().Key.Should().Be("MovieId");
        performanceEntityTypeBuilder.PartitionKey.First().Value.Should().Be("MOVIE");
        performanceEntityTypeBuilder.SortKey.Should().NotBeNull();
        performanceEntityTypeBuilder.SortKey.Count().Should().Be(1);
        performanceEntityTypeBuilder.SortKey.First().Key.Should().Be("ActorId");
        performanceEntityTypeBuilder.SortKey.First().Value.Should().Be("ACTOR");
        performanceEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.Should().NotBeNull();
        performanceEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.Count().Should().Be(1);
        performanceEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Key.Should().Be("GSI1PK");
        performanceEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value[0].Path.Should().Be("ActorId");
        performanceEntityTypeBuilder.GlobalSecondaryIndexPartitionKey.First().Value[0].Prefix.Should().Be("ACTOR");
        performanceEntityTypeBuilder.GlobalSecondaryIndexSortKey.Should().NotBeNull();
        performanceEntityTypeBuilder.GlobalSecondaryIndexSortKey.Count().Should().Be(1);
        performanceEntityTypeBuilder.GlobalSecondaryIndexSortKey.First().Key.Should().Be("GSI1SK");
        performanceEntityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value[0].Path.Should().Be("MovieId");
        performanceEntityTypeBuilder.GlobalSecondaryIndexSortKey.First().Value[0].Prefix.Should().Be("MOVIE");
    }
}
