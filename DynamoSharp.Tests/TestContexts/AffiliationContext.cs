using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models.Affiliation;

namespace DynamoSharp.Tests.TestContexts;

internal class AffiliationContext : DynamoSharpContext
{
    public IDynamoDbSet<Affiliation> Affiliations { get; private set; } = null!;

    public AffiliationContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        ModelBuilder.Entity<Affiliation>()
            .HasPartitionKey(a => a.MerchantId, "MERCHANT");

        ModelBuilder.Entity<Affiliation>()
            .HasSortKey(a => a.Section)
            .Include(a => a.CardBrand)
            .Include(a => a.CountryOrRigion)
            .Include(a => a.Bank)
            .Include(a => a.Type);
    }
}
