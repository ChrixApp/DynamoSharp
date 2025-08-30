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
}
