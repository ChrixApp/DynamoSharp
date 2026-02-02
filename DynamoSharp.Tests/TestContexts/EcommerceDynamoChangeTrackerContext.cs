using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.TestContexts.Models.Ecommerce;

namespace DynamoSharp.Tests.TestContexts;

public class EcommerceDynamoChangeTrackerContext : DynamoSharpContext
{
    public IDynamoDbSet<Order> Orders { get; private set; } = null!;

    public EcommerceDynamoChangeTrackerContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        ModelBuilder.Entity<Order>()
            .HasPartitionKey(o => o.Id, "ORDER");

        ModelBuilder.Entity<Order>()
            .HasSortKey(oi => oi.Id, "ORDER");

        ModelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);

        ModelBuilder.Entity<Item>()
            .HasSortKey(oi => oi.Id, "ORDERITEM");
    }
}
