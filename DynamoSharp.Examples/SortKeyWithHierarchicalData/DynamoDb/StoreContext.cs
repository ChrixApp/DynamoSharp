using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using SortKeyWithHierarchicalData.Models;

namespace SortKeyWithHierarchicalData.DynamoDb;

public class StoreContext : DynamoSharpContext
{
    public IDynamoDbSet<Store> Stores { get; private set; } = null!;

    public StoreContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        // Example Partition Key: STORE#D1ADDCA5-2B7B-4DE5-A221-C626C0A677F9
        modelBuilder.Entity<Store>()
            .HasPartitionKey(s => s.Id, "STORE");

        // Example Sort Key: STORE#TacosElGuero
        modelBuilder.Entity<Store>()
            .HasSortKey(s => s.Name, "STORE");

        // Example Global Secondary Index Partition Key: COUNTRY#Mexico
        modelBuilder.Entity<Store>()
            .HasGlobalSecondaryIndexPartitionKey("GSI1PK", s => s.Address!.Country);

        // Example Global Secondary Index Sort Key: STATE#Jalisco#CITY#Villahermosa#ZIPCODE#44100
        modelBuilder.Entity<Store>()
            .HasGlobalSecondaryIndexSortKey("GSI1SK", s => s.Address!.State)
            .Include(s => s.Address!.City)
            .Include(s => s.Address!.ZipCode);
    }
}
