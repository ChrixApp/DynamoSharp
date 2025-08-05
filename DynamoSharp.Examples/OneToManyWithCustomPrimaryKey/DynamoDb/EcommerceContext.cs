using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using OneToManyWithCustomPrimaryKey.Models;

namespace OneToManyWithCustomPrimaryKey.DynamoDb;

internal class EcommerceContext : DynamoSharpContext
{
    public IDynamoDbSet<Order> Orders { get; private set; } = null!;

    public EcommerceContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        // Example Partition Key: ORDER#<Guid>
        modelBuilder.Entity<Order>()
            .HasPartitionKey(o => o.Id, "ORDER");

        // Example Sort Key: ORDER#<Guid>
        modelBuilder.Entity<Order>()
            .HasSortKey(oi => oi.Id, "ORDER");

        modelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);

        // Example Partition Key: ITEM#<Guid>
        modelBuilder.Entity<Item>()
            .HasSortKey(oi => oi.Id, "ITEM");
    }
}
