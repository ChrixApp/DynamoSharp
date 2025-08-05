using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using PrimaryKeyUsingNestedProperties.Models;

namespace PrimaryKeyUsingNestedProperties.Context;

public class StoreContext : DynamoSharpContext
{
    public IDynamoDbSet<Store> Stores { get; private set; } = null!;

    public StoreContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        // Partition Key and Sort Key with nested properties
        // Example Global Secondary Index Partition Key: COUNTRY#Mexico
        modelBuilder.Entity<Store>()
            .HasPartitionKey(s => s.Address!.Country, "COUNTRY");

        // Example Global Secondary Index Sort Key: STATE#Tabasco
        modelBuilder.Entity<Store>()
            .HasSortKey(s => s.Address!.State, "STATE");
    }
}
