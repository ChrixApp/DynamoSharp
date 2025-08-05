using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using GlobalSecondaryIndex.Models;

namespace GlobalSecondaryIndex.DynamoDb;

internal class EcommerceContext : DynamoSharpContext
{
    public IDynamoDbSet<Order> Orders { get; private set; } = null!;

    public EcommerceContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        // Example Partition Key: ORDER#{Guid}
        modelBuilder.Entity<Order>()
            .HasPartitionKey(o => o.Id, "ORDER");

        // Example Sort Key: ORder#{Guid}
        modelBuilder.Entity<Order>()
            .HasSortKey(oi => oi.Id, "ORDER");

        // Example Global Secondary Index Partition Key: BUYER#{Guid}
        modelBuilder.Entity<Order>()
            .HasGlobalSecondaryIndexPartitionKey("GSI1PK", o => o.BuyerId, "BUYER");

        // Example Global Secondary Index Sort Key: STATUS#{Status}#DATE#{DateTime}
        modelBuilder.Entity<Order>()
            .HasGlobalSecondaryIndexSortKey("GSI1SK", o => o.Status)
            .Include(o => o.Date);

        modelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);

        // Example Partition Key: ITEM#{Guid}
        modelBuilder.Entity<Item>()
            .HasSortKey(oi => oi.Id, "ITEM");
    }
}
