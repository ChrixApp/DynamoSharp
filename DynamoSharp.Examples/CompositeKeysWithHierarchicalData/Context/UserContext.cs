using CompositeKeysWithHierarchicalData.Models;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;

namespace CompositeKeysWithHierarchicalData.Context;

public class UserContext : DynamoSharpContext
{
    public IDynamoDbSet<User> Users { get; private set; } = null!;

    public UserContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        // Example Partition Key: ID#D1ADDCA5-2B7B-4DE5-A221-C626C0A677F9#EMAIL#example@example
        modelBuilder.Entity<User>()
            .HasPartitionKey(u => u.Id, "ID")
            .Include(u => u.Email, "EMAIL");

        // Example Sort Key: ID#D1ADDCA5-2B7B-4DE5-A221-C626C0A677F9#NAME#Chris
        modelBuilder.Entity<User>()
            .HasSortKey(u => u.Id, "ID")
            .Include(u => u.Name, "NAME");
    }
}
