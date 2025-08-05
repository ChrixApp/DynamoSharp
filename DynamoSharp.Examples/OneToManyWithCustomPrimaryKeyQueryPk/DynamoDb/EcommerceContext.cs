using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using OneToManyWithCustomPrimaryKeyQueryPk.Models;

namespace OneToManyWithCustomPrimaryKeyQueryPk.DynamoDb;

internal class EcommerceContext : DynamoSharpContext
{
    public IDynamoDbSet<Order> Orders { get; private set; } = null!;

    public EcommerceContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        // Example Partition Key: ORDER#<OrderId>
        modelBuilder.Entity<Order>()
            .HasPartitionKey(o => o.Id, "ORDER");

        // Example Sort Key: ORDER#<OrderId>
        modelBuilder.Entity<Order>()
            .HasSortKey(oi => oi.Id, "ORDER");

        modelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);

        // Example Partition Key: ITEM#<ItemId>
        modelBuilder.Entity<Item>()
            .HasSortKey(oi => oi.Id, "ITEM");
    }
}
