using DynamoSharp.DynamoDb.Configs;
using Xunit;

namespace DynamoSharp.Tests.DynamoDb.Configuration;

public class TableSchemaTests
{
    [Fact]
    public void CreateTableSchema_WithOnlyTableName_InitializesCorrectly()
    {
        // Arrange
        var tableName = "Movies";

        // Act
        var tableSchema = new TableSchema.TableSchemaBuilder()
            .WithTableName(tableName)
            .Build();

        // Assert
        Assert.Equal(tableName, tableSchema.TableName);
        Assert.Equal("PartitionKey", tableSchema.PartitionKeyName);
        Assert.Equal("SortKey", tableSchema.SortKeyName);
        Assert.Empty(tableSchema.GlobalSecondaryIndices);
    }

    [Fact]
    public void CreateTableSchema_WithPartitionKeyAndSortKey_InitializesCorrectly()
    {
        var tableSchema = new TableSchema.TableSchemaBuilder()
            .WithTableName("Movies")
            .WithPartitionKeyName("PK")
            .WithSortKeyName("SK")
            .Build();

        Assert.Equal("Movies", tableSchema.TableName);
        Assert.Equal("PK", tableSchema.PartitionKeyName);
        Assert.Equal("SK", tableSchema.SortKeyName);
        Assert.Empty(tableSchema.GlobalSecondaryIndices);
    }

    [Fact]
    public void CreateTableSchema_WithGlobalSecondaryIndices_InitializesCorrectly()
    {
        var gsi1 = new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK");
        var gsi2 = new GlobalSecondaryIndexSchema("GSI2PK-GSI2SK-index", "GSI2PK", "GSI2SK");

        var tableSchema = new TableSchema.TableSchemaBuilder()
            .WithTableName("Movies")
            .AddGlobalSecondaryIndex(gsi1, gsi2)
            .Build();

        Assert.Equal("Movies", tableSchema.TableName);
        Assert.Equal("PartitionKey", tableSchema.PartitionKeyName);
        Assert.Equal("SortKey", tableSchema.SortKeyName);
        Assert.Equal(2, tableSchema.GlobalSecondaryIndices.Count);
        Assert.Equal("GSI1PK-GSI1SK-index", tableSchema.GlobalSecondaryIndices[0].IndexName);
        Assert.Equal("GSI1PK", tableSchema.GlobalSecondaryIndices[0].PartitionKeyName);
        Assert.Equal("GSI1SK", tableSchema.GlobalSecondaryIndices[0].SortKeyName);
        Assert.Equal("GSI2PK-GSI2SK-index", tableSchema.GlobalSecondaryIndices[tableSchema.GlobalSecondaryIndices.Count - 1].IndexName);
        Assert.Equal("GSI2PK", tableSchema.GlobalSecondaryIndices[tableSchema.GlobalSecondaryIndices.Count - 1].PartitionKeyName);
        Assert.Equal("GSI2SK", tableSchema.GlobalSecondaryIndices[tableSchema.GlobalSecondaryIndices.Count - 1].SortKeyName);
    }

    [Fact]
    public void CreateTableSchema_WithPartitionKeySortKeyAndGlobalSecondaryIndices_InitializesCorrectly()
    {
        var gsi1 = new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK");
        var gsi2 = new GlobalSecondaryIndexSchema("GSI2PK-GSI2SK-index", "GSI2PK", "GSI2SK");

        var tableSchema = new TableSchema.TableSchemaBuilder()
            .WithTableName("Movies")
            .WithPartitionKeyName("PK")
            .WithSortKeyName("SK")
            .AddGlobalSecondaryIndex(gsi1, gsi2)
            .Build();

        Assert.Equal("Movies", tableSchema.TableName);
        Assert.Equal("PK", tableSchema.PartitionKeyName);
        Assert.Equal("SK", tableSchema.SortKeyName);
        Assert.Equal(2, tableSchema.GlobalSecondaryIndices.Count);
        Assert.Equal("GSI1PK-GSI1SK-index", tableSchema.GlobalSecondaryIndices[0].IndexName);
        Assert.Equal("GSI1PK", tableSchema.GlobalSecondaryIndices[0].PartitionKeyName);
        Assert.Equal("GSI1SK", tableSchema.GlobalSecondaryIndices[0].SortKeyName);
        Assert.Equal("GSI2PK-GSI2SK-index", tableSchema.GlobalSecondaryIndices[tableSchema.GlobalSecondaryIndices.Count - 1].IndexName);
        Assert.Equal("GSI2PK", tableSchema.GlobalSecondaryIndices[tableSchema.GlobalSecondaryIndices.Count - 1].PartitionKeyName);
        Assert.Equal("GSI2SK", tableSchema.GlobalSecondaryIndices[tableSchema.GlobalSecondaryIndices.Count - 1].SortKeyName);
    }

    [Fact]
    public void CreateTableSchema_WithNullOrEmptyTableName_ThrowsArgumentNullException()
    {
        Assert.Throws<InvalidOperationException>(() => new TableSchema.TableSchemaBuilder().Build());
        Assert.Throws<ArgumentNullException>(() => new TableSchema.TableSchemaBuilder().WithTableName(null!));
        Assert.Throws<ArgumentNullException>(() => new TableSchema.TableSchemaBuilder().WithTableName(string.Empty));
    }

    [Fact]
    public void CreateTableSchema_WithNullOrEmptyPartitionKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TableSchema.TableSchemaBuilder().WithPartitionKeyName(null!));
        Assert.Throws<ArgumentNullException>(() => new TableSchema.TableSchemaBuilder().WithPartitionKeyName(string.Empty));
    }

    [Fact]
    public void CreateTableSchema_WithNullOrEmptySortKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TableSchema.TableSchemaBuilder().WithSortKeyName(null!));
        Assert.Throws<ArgumentNullException>(() => new TableSchema.TableSchemaBuilder().WithSortKeyName(string.Empty));
    }

    [Fact]
    public void CreateTableSchema_WithNullGlobalSecondaryIndices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TableSchema.TableSchemaBuilder().AddGlobalSecondaryIndex(null!));
    }

    [Fact]
    public void CreateTableSchema_WithNullGlobalSecondaryIndex_ThrowsArgumentNullException()
    {
        var gsi1 = new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK");
        Assert.Throws<ArgumentNullException>(() => new TableSchema.TableSchemaBuilder().AddGlobalSecondaryIndex(gsi1, null!));
    }

    [Fact]
    public void GetGlobalSecondaryIndexPartitionKey_ReturnsCorrectPartitionKey()
    {
        var gsi1 = new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK");
        var gsi2 = new GlobalSecondaryIndexSchema("GSI2PK-GSI2SK-index", "GSI2PK", "GSI2SK");

        var tableSchema = new TableSchema.TableSchemaBuilder()
            .WithTableName("Movies")
            .AddGlobalSecondaryIndex(gsi1, gsi2)
            .Build();

        Assert.Equal("GSI1PK", tableSchema.GetGlobalSecondaryIndexPartitionKey("GSI1PK-GSI1SK-index"));
        Assert.Equal("GSI2PK", tableSchema.GetGlobalSecondaryIndexPartitionKey("GSI2PK-GSI2SK-index"));
    }

    [Fact]
    public void GetGlobalSecondaryIndexSortKey_ReturnsCorrectSortKey()
    {
        var gsi1 = new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK");
        var gsi2 = new GlobalSecondaryIndexSchema("GSI2PK-GSI2SK-index", "GSI2PK", "GSI2SK");

        var tableSchema = new TableSchema.TableSchemaBuilder()
            .WithTableName("Movies")
            .AddGlobalSecondaryIndex(gsi1, gsi2)
            .Build();

        Assert.Equal("GSI1SK", tableSchema.GetGlobalSecondaryIndexSortKey("GSI1PK-GSI1SK-index"));
        Assert.Equal("GSI2SK", tableSchema.GetGlobalSecondaryIndexSortKey("GSI2PK-GSI2SK-index"));
    }
}
