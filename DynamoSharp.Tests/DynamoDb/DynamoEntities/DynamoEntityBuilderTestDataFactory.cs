using Amazon.Runtime;
using DynamoSharp.ChangeTracking;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models.Affiliation;
using DynamoSharp.Tests.TestContexts.Models.Ecommerce;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;

using RegionEndpoint = EfficientDynamoDb.Configs.RegionEndpoint;

namespace DynamoSharp.Tests.DynamoDb.DynamoEntities;

public static class DynamoEntityBuilderTestDataFactory
{
    public static (TableSchema, IChangeTracker, IModelBuilder, Guid) CreateAffiliationContextWithPrefixPrimaryKey(
        int totalOrders,
        string prefixPk = "MERCHANT",
        string prefixSk1 = "SECTION",
        string prefixSk2 = "CARDBRAND",
        string prefixSk3 = "COUNTRYORREGION",
        string prefixSk4 = "BANK",
        string prefixSk5 = "TYPE")
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("affiliations")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Affiliation>()
            .HasPartitionKey(a => a.MerchantId, prefixPk);

        modelBuilder.Entity<Affiliation>()
            .HasSortKey(a => a.Section, prefixSk1)
            .Include(a => a.CardBrand, prefixSk2)
            .Include(a => a.CountryOrRigion, prefixSk3)
            .Include(a => a.Bank, prefixSk4)
            .Include(a => a.Type, prefixSk5)
            .Include(a => a.TerminalId);

        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        var merchantId = Guid.NewGuid();
        for (int i = 0; i < totalOrders; i++)
        {
            var terminalId = Guid.NewGuid();
            var section = Section.Default;
            var cardBrand = CardBrand.Other;
            var countryOrRigion = CountryOrRigion.MX;
            var bank = Bank.Default;
            var type = AffiliationType.Default;
            var affiliation = new Affiliation(merchantId, terminalId, section, cardBrand, countryOrRigion, bank, type);
            changeTracker.Track(affiliation, EntityState.Added);
        }

        return (tableSchema, changeTracker, modelBuilder, merchantId);
    }

    public static (TableSchema, IChangeTracker, IModelBuilder, Guid) CreateAffiliationContextWithPrimaryKey(int totalOrders)
    {
        return CreateAffiliationContextWithPrefixPrimaryKey(
            totalOrders,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);
    }

    public static (TableSchema, IModelBuilder, IChangeTracker, Guid) CreateAffiliationContextForSingleEntity()
    {
        var tableSchema = new TableSchema.Builder()
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

        return (tableSchema, modelBuilder, changeTracker, merchantId);
    }

    public static (TableSchema, IModelBuilder, IChangeTracker, Guid merchantId) CreateAffiliationContextWithSortKey()
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("affiliations")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Affiliation>()
            .HasSortKey(a => a.MerchantId);

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

        return (tableSchema, modelBuilder, changeTracker, merchantId);
    }

    public static (TableSchema, IModelBuilder, IChangeTracker, Guid merchantId) CreateAffiliationContextWithPrefixSortKey(string prefixSk)
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("affiliations")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Affiliation>()
            .HasSortKey(a => a.MerchantId, prefixSk);

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

        return (tableSchema, modelBuilder, changeTracker, merchantId);
    }

    public static (TableSchema, IModelBuilder, IChangeTracker, Guid) CreateAffiliationContextWithGsiPk(string gsiPkName)
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("affiliations")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Affiliation>()
            .HasGlobalSecondaryIndexPartitionKey(gsiPkName, a => a.TerminalId);

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

        return (tableSchema, modelBuilder, changeTracker, terminalId);
    }

    public static (TableSchema, IModelBuilder, IChangeTracker, Guid) CreateAffiliationContextWithGsiSk(string gsiSkName)
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("affiliations")
            .Build();
        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity<Affiliation>()
            .HasGlobalSecondaryIndexSortKey(gsiSkName, a => a.TerminalId);

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

        return (tableSchema, modelBuilder, changeTracker, terminalId);
    }

    public static (TableSchema, IModelBuilder, IChangeTracker, Guid) CreateAffiliationContextWithGsiSkAsNumber(string gsiSkName)
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("affiliations")
            .Build();
        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity<Affiliation>()
            .HasGlobalSecondaryIndexSortKey(gsiSkName, a => a.Percentage);

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

        return (tableSchema, modelBuilder, changeTracker, terminalId);
    }

    public static (TableSchema TableSchema, IModelBuilder ModelBuilder, IChangeTracker ChangeTracker, Order Order) CreateOrderContextWithOneToMany()
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Order>()
            .HasPartitionKey(o => o.Id, "ORDER")
            .Include(o => o.Date, "DATE");

        modelBuilder.Entity<Order>()
            .HasSortKey(o => o.Id, "ORDER")
            .Include(o => o.BuyerId, "BUYER");

        modelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);

        modelBuilder.Entity<Item>()
            .HasSortKey(oi => oi.Id, "ORDERITEM")
            .Include(oi => oi.UnitPrice, "UNITPRICE");

        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        var order = CreateOrder();
        order.AddProduct(Guid.NewGuid(), $"Product 1", 10);
        order.AddProduct(Guid.NewGuid(), $"Product 2", 15, 10);
        changeTracker.Track(order, EntityState.Added);

        return (tableSchema, modelBuilder, changeTracker, order);
    }

    public static Order CreateOrder()
    {
        var buyerId = Guid.Parse("330AE372-0D53-453B-961D-38F3B8716941");
        var street = "Street 1";
        var city = "City 1";
        var state = "State 1";
        var zipCode = "ZipCode 1";
        return new Order.Builder()
            .WithBuyerId(buyerId)
            .WithAddress(street, city, state, zipCode)
            .WithDate(DateTime.Now)
            .Build();
    }

    public static (TableSchema TableSchema, IModelBuilder ModelBuilder, IChangeTracker ChangeTracker, Order Order) CreateOrderContextWithoutOneToMany()
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Order>()
            .HasPartitionKey(o => o.Id, "ORDER")
            .Include(o => o.Date, "DATE");

        modelBuilder.Entity<Order>()
            .HasSortKey(o => o.Id, "ORDER")
            .Include(o => o.BuyerId, "BUYER");

        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        var order = CreateOrder();
        order.AddProduct(Guid.NewGuid(), $"Product 1", 10);
        order.AddProduct(Guid.NewGuid(), $"Product 2", 15, 10);
        changeTracker.Track(order, EntityState.Added);

        return (tableSchema, modelBuilder, changeTracker, order);
    }

    public static DynamoDbContextConfig GetDynamoDbContextConfig()
    {
        var mockCredentials = new BasicAWSCredentials("fake-access-key", "fake-secret-key");
        var effDdbCredentials = mockCredentials.ToCredentialsProvider();
        return new DynamoDbContextConfig(RegionEndpoint.USEast1, effDdbCredentials);
    }

    public static (TableSchema TableSchema, IModelBuilder ModelBuilder, IChangeTracker ChangeTracker, Order Order) CreateOrderWithSparseIndex()
    {
        var tableSchema = new TableSchema.Builder()
            .WithTableName("order")
            .Build();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);
        modelBuilder.Entity<Order>()
            .HasGlobalSecondaryIndexPartitionKey("GSI1PK", "ORDERS");
        modelBuilder.Entity<Order>()
            .HasGlobalSecondaryIndexSortKey("GSI1SK", o => o.Id, "ORDER");
        modelBuilder.Entity<Item>();
        var changeTracker = new ChangeTracker(tableSchema, modelBuilder);

        var order1 = CreateOrder();
        order1.AddProduct(Guid.NewGuid(), $"Product 1", 10);
        changeTracker.Track(order1, EntityState.Added);

        var order2 = CreateOrder();
        order2.AddProduct(Guid.NewGuid(), $"Product 2", 15, 10);
        changeTracker.Track(order2, EntityState.Added);

        return (tableSchema, modelBuilder, changeTracker, null!);
    }
}
