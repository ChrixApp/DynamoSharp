using DynamoSharp.DynamoDb.DynamoEntities;
using DynamoSharp.Tests.Contexts.Models.Affiliation;
using FluentAssertions;

namespace DynamoSharp.Tests.DynamoDb.DynamoEntities;

public class BatchDynamoEntityBuilderTests
{
    [Fact]
    public void BuildModifiedEntity_ShouldReturnDynamoModifiedEntity()
    {
        // arrange
        var (tableSchema, modelBuilder, changeTracker, merchantId) = BatchDynamoEntityBuilderTestDataFactory.CreateAffiliationContextForSingleEntity();
        var batchDynamoEntityBuilder = new BatchDynamoEntityBuilder(tableSchema, modelBuilder);
        var (_, modifiedEntities, _) = changeTracker.FetchChanges();
        var entity = (Affiliation)modifiedEntities[0].Entity;

        // act
        var dynamoModifiedEntity = batchDynamoEntityBuilder.BuildModifiedEntity(modifiedEntities[0]);

        // assert
        dynamoModifiedEntity.Children().Should().HaveCount(12);
        dynamoModifiedEntity.TryGetValue(tableSchema.PartitionKeyName, out var partitionKey).Should().Be(true);
        partitionKey?.ToString().Should().Be(merchantId.ToString());
        dynamoModifiedEntity.TryGetValue(tableSchema.SortKeyName, out var sortKey).Should().Be(true);
        sortKey?.ToString().Should().StartWith($"{Section.Default}#{CardBrand.Other}#{CountryOrRigion.US}#{Bank.Default}#{AffiliationType.Default}");
        dynamoModifiedEntity.TryGetValue("Percentage", out var percentage).Should().Be(true);
        percentage?.ToString().Should().Be(entity.Percentage.ToString());
        dynamoModifiedEntity.TryGetValue("MerchantId", out var merchantIdValue).Should().Be(true);
        merchantIdValue?.ToString().Should().Be(merchantId.ToString());
        dynamoModifiedEntity.TryGetValue("TerminalId", out var terminalId).Should().Be(true);
        terminalId?.ToString().Should().Be(entity.TerminalId.ToString());
        dynamoModifiedEntity.TryGetValue("Id", out var id).Should().Be(true);
        id?.ToString().Should().Be(entity.Id.ToString());
        dynamoModifiedEntity.TryGetValue("CardBrand", out var cardBrand).Should().Be(true);
        cardBrand?.ToString().Should().Be(entity.CardBrand.ToString());
        dynamoModifiedEntity.TryGetValue("Section", out var section).Should().Be(true);
        section?.ToString().Should().Be(entity.Section.ToString());
        dynamoModifiedEntity.TryGetValue("CountryOrRigion", out var countryOrRigion).Should().Be(true);
        countryOrRigion?.ToString().Should().Be(entity.CountryOrRigion.ToString());
        dynamoModifiedEntity.TryGetValue("Bank", out var bank).Should().Be(true);
        bank?.ToString().Should().Be(entity.Bank.ToString());
        dynamoModifiedEntity.TryGetValue("Type", out var type).Should().Be(true);
        type?.ToString().Should().Be(entity.Type.ToString());
        dynamoModifiedEntity.TryGetValue("CreatedAt", out var createdAt).Should().Be(true);
        createdAt?.ToString().Should().Be(entity.CreatedAt.ToString());
    }
}
