using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using OptimisticLockingSave.Models;

namespace OptimisticLockingSave.Context;

public class EcommerceContext : DynamoSharpContext
{
    public IDynamoDbSet<Order> Orders { get; private set; } = null!;

    public EcommerceContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);

        // indicate that Order has optimistic locking
        modelBuilder.Entity<Order>()
            .HasVersioning();

        // indicate that Item has optimistic locking
        modelBuilder.Entity<Item>()
            .HasVersioning();
    }
}
