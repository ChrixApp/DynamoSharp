using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models.Affiliation;

namespace DynamoSharp.Tests.DynamoDb.DynamoEntities;

public static class TransactDynamoEntityBuilderTestDataFactory
{
    public static (TableSchema, IModelBuilder, IChangeTracker, Guid) CreateAffiliationContextForSingleEntity()
    {
        var tableSchema = new TableSchema.TableSchemaBuilder()
            .WithTableName("affiliations")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Affiliation>()
            .HasPartitionKey(a => a.MerchantId);

        modelBuilder.Entity<Affiliation>()
            .HasSortKey(a => a.Section)
            .Include(a => a.CardBrand)
            .Include(a => a.CountryOrRigion)
            .Include(a => a.Bank)
            .Include(a => a.Type)
            .Include(a => a.TerminalId);

        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        var merchantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        var section = Section.Default;
        var cardBrand = CardBrand.Other;
        var countryOrRigion = CountryOrRigion.MX;
        var bank = Bank.Default;
        var type = AffiliationType.Default;
        var affiliation = new Affiliation(merchantId, terminalId, section, cardBrand, countryOrRigion, bank, type);
        changeTracker.Track(affiliation, EntityState.Added);

        affiliation.UpdateCountryOrRigion(CountryOrRigion.US);

        return (tableSchema, modelBuilder, changeTracker, merchantId);
    }

    public static (TableSchema, IModelBuilder, IChangeTracker, Guid) CreateAffiliationContextForSingleEntity(int version)
    {
        var tableSchema = new TableSchema.TableSchemaBuilder()
            .WithTableName("affiliations")
            .WithVersionName("v")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Affiliation>()
            .HasPartitionKey(a => a.MerchantId);

        modelBuilder.Entity<Affiliation>()
            .HasSortKey(a => a.Section)
            .Include(a => a.CardBrand)
            .Include(a => a.CountryOrRigion)
            .Include(a => a.Bank)
            .Include(a => a.Type)
            .Include(a => a.TerminalId);

        modelBuilder.Entity<Affiliation>()
            .HasVersioning();

        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        var merchantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        var section = Section.Default;
        var cardBrand = CardBrand.Other;
        var countryOrRigion = CountryOrRigion.MX;
        var bank = Bank.Default;
        var type = AffiliationType.Default;
        var affiliation = new Affiliation(merchantId, terminalId, section, cardBrand, countryOrRigion, bank, type);
        var entityChangeTracker = new EntityChangeTracker(modelBuilder, affiliation, EntityState.Unchanged, version);
        changeTracker.Track(entityChangeTracker);

        affiliation.UpdateCountryOrRigion(CountryOrRigion.US);

        return (tableSchema, modelBuilder, changeTracker, merchantId);
    }
}
