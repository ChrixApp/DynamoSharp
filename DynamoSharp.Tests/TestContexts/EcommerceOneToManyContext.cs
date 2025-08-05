using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models;

namespace DynamoSharp.Tests.TestContexts;

public class EcommerceOneToManyContext : DynamoSharpContext
{
    public IDynamoDbSet<Order> Orders { get; private set; } = null!;

    public EcommerceOneToManyContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);
    }
}