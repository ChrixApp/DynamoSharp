using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.TestContexts.Models.Ecommerce;

namespace DynamoSharp.Tests.TestContexts;

public class NewEcommerceDynamoChangeTrackerContext : DynamoSharpContext
{
    public IDynamoDbSet<NewOrder> Orders { get; private set; } = null!;

    public NewEcommerceDynamoChangeTrackerContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewOrder>()
            .HasPartitionKey(o => o.Id, "ORDER");

        modelBuilder.Entity<NewOrder>()
            .HasSortKey(oi => oi.Id, "ORDER");

        modelBuilder.Entity<NewOrder>()
            .HasOneToMany(o => o.Items);

        modelBuilder.Entity<Item>()
            .HasSortKey(oi => oi.Id, "ITEM");
    }
}
