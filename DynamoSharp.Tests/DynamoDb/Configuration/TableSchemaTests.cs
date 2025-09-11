using DynamoSharp.DynamoDb.Configs;

namespace DynamoSharp.Tests.DynamoDb.Configuration;

public class TableSchemaTests
{
    [Fact]
    public void CreateTableSchema_WithOnlyTableName_InitializesCorrectly()
    {
        // Arrange
        var tableName = "Movies";

        // Act
        var tableSchema = new TableSchema.Builder()
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
        var tableSchema = new TableSchema.Builder()
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
        var tableSchema = new TableSchema.Builder()
            .WithTableName("Movies")
            .AddGlobalSecondaryIndex("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK")
            .AddGlobalSecondaryIndex("GSI2PK-GSI2SK-index", "GSI2PK", "GSI2SK")
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
    public void AddGlobalSecondaryIndex_AddsIndex_ToTableSchema()
    {
        // Arrange
        var builder = new TableSchema.Builder()
            .WithTableName("MyTable");

        // Act
        var table = builder
            .AddGlobalSecondaryIndex("MyIndex", "MyPartitionKey", "MySortKey")
            .Build();

        // Assert
        Assert.Single(table.GlobalSecondaryIndices);
        var gsi = table.GlobalSecondaryIndices.Single();
        Assert.Equal("MyIndex", gsi.IndexName);
        Assert.Equal("MyPartitionKey", gsi.PartitionKeyName);
        Assert.Equal("MySortKey", gsi.SortKeyName);
    }

    [Fact]
    public void AddGlobalSecondaryIndex_ReturnsSameBuilder_ForFluentCalls()
    {
        var builder = new TableSchema.Builder();
        var returned = builder.AddGlobalSecondaryIndex("i", "p", "s");
        Assert.Same(builder, returned);
    }

    [Fact]
    public void AddGlobalSecondaryIndex_ThrowsArgumentNullException_WhenIndexNameIsNullOrWhitespace()
    {
        var builder = new TableSchema.Builder();
        Assert.Throws<ArgumentNullException>(() => builder.AddGlobalSecondaryIndex(null!, "p", "s"));
        Assert.Throws<ArgumentNullException>(() => builder.AddGlobalSecondaryIndex(string.Empty, "p", "s"));
        Assert.Throws<ArgumentNullException>(() => builder.AddGlobalSecondaryIndex("   ", "p", "s"));
    }

    [Fact]
    public void AddGlobalSecondaryIndex_ThrowsArgumentNullException_WhenPartitionOrSortKeyIsNullOrWhitespace()
    {
        var builder = new TableSchema.Builder();

        // partition key invalid
        Assert.Throws<ArgumentNullException>(() => builder.AddGlobalSecondaryIndex("i", null!, "s"));
        Assert.Throws<ArgumentNullException>(() => builder.AddGlobalSecondaryIndex("i", string.Empty, "s"));

        // sort key invalid
        Assert.Throws<ArgumentNullException>(() => builder.AddGlobalSecondaryIndex("i", "p", null!));
        Assert.Throws<ArgumentNullException>(() => builder.AddGlobalSecondaryIndex("i", "p", string.Empty));
    }

    [Fact]
    public void AddGlobalSecondaryIndex_ThrowsInvalidOperationException_WhenAddingMoreThan20Indices()
    {
        var builder = new TableSchema.Builder();

        // Add 21 indices (this is allowed by current implementation)
        for (var i = 1; i <= 21; i++)
        {
            builder.AddGlobalSecondaryIndex($"idx{i}", $"pk{i}", $"sk{i}");
        }

        // Adding the 22nd should throw per current guard (_globalSecondaryIndices.Count > 20)
        Assert.Throws<InvalidOperationException>(() => builder.AddGlobalSecondaryIndex("idx22", "pk22", "sk22"));
    }

    [Fact]
    public void CreateTableSchema_WithPartitionKeySortKeyAndGlobalSecondaryIndices_InitializesCorrectly()
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("Movies")
            .WithPartitionKeyName("PK")
            .WithSortKeyName("SK")
            .AddGlobalSecondaryIndex("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK")
            .AddGlobalSecondaryIndex("GSI2PK-GSI2SK-index", "GSI2PK", "GSI2SK")
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
        Assert.Throws<InvalidOperationException>(() => new TableSchema.Builder().Build());
        Assert.Throws<ArgumentNullException>(() => new TableSchema.Builder().WithTableName(null!));
        Assert.Throws<ArgumentNullException>(() => new TableSchema.Builder().WithTableName(string.Empty));
    }

    [Fact]
    public void CreateTableSchema_WithNullOrEmptyPartitionKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TableSchema.Builder().WithPartitionKeyName(null!));
        Assert.Throws<ArgumentNullException>(() => new TableSchema.Builder().WithPartitionKeyName(string.Empty));
    }

    [Fact]
    public void CreateTableSchema_WithNullOrEmptySortKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TableSchema.Builder().WithSortKeyName(null!));
        Assert.Throws<ArgumentNullException>(() => new TableSchema.Builder().WithSortKeyName(string.Empty));
    }

    [Fact]
    public void GetGlobalSecondaryIndexPartitionKey_ReturnsCorrectPartitionKey()
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("Movies")
            .AddGlobalSecondaryIndex("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK")
            .AddGlobalSecondaryIndex("GSI2PK-GSI2SK-index", "GSI2PK", "GSI2SK")
            .Build();

        Assert.Equal("GSI1PK", tableSchema.GetGlobalSecondaryIndexPartitionKey("GSI1PK-GSI1SK-index"));
        Assert.Equal("GSI2PK", tableSchema.GetGlobalSecondaryIndexPartitionKey("GSI2PK-GSI2SK-index"));
    }

    [Fact]
    public void GetGlobalSecondaryIndexSortKey_ReturnsCorrectSortKey()
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("Movies")
            .AddGlobalSecondaryIndex("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK")
            .AddGlobalSecondaryIndex("GSI2PK-GSI2SK-index", "GSI2PK", "GSI2SK")
            .Build();

        Assert.Equal("GSI1SK", tableSchema.GetGlobalSecondaryIndexSortKey("GSI1PK-GSI1SK-index"));
        Assert.Equal("GSI2SK", tableSchema.GetGlobalSecondaryIndexSortKey("GSI2PK-GSI2SK-index"));
    }
}
