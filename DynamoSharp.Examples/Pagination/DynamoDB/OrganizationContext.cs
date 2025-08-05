using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using Pagination.Models;

namespace Pagination.DynamoDB;

public class OrganizationContext : DynamoSharpContext
{
    public IDynamoDbSet<Organization> Organizations { get; private set; } = null!;
    public IDynamoDbSet<User> Users { get; private set; } = null!;

    public OrganizationContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        // Example Partition Key: ORG#{Name}
        modelBuilder.Entity<Organization>()
            .HasPartitionKey(o => o.Name, "ORG");

        // Example Sort Key: ORG#{Name}
        modelBuilder.Entity<Organization>()
            .HasSortKey(u => u.Name, "ORG");

        // Example Global Secondary Index Partition Key: ORGANIZATIONS
        modelBuilder.Entity<Organization>()
            .HasGlobalSecondaryIndexPartitionKey("GSI1PK", "ORGANIZATIONS");

        // Example Global Secondary Index Sort Key: ORG#{Name}
        modelBuilder.Entity<Organization>()
            .HasGlobalSecondaryIndexPartitionKey("GSI1SK", o => o.Name);

        modelBuilder.Entity<Organization>()
            .HasOneToMany(u => u.Users);

        // Example Partition Key: USER#{Name}
        modelBuilder.Entity<User>()
            .HasSortKey(u => u.Name, "USER");
    }
}
