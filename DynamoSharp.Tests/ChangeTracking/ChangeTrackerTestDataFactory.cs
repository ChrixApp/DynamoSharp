using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models;
using DynamoSharp.Tests.Contexts.Models.Affiliation;

namespace DynamoSharp.Tests.ChangeTracking;

public static class ChangeTrackerTestDataFactory
{
    public static IModelBuilder CreateModelBuilderForAffiliation()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Affiliation>()
            .HasPartitionKey(a => a.MerchantId);

        modelBuilder.Entity<Affiliation>()
            .HasSortKey(a => a.Section)
            .Include(a => a.CardBrand)
            .Include(a => a.CountryOrRigion)
            .Include(a => a.Bank)
            .Include(a => a.Type);

        return modelBuilder;
    }

    public static Affiliation CreateAffiliation()
    {
        var merchantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        var section = Section.Default;
        var cardBrand = CardBrand.Other;
        var countryOrRigion = CountryOrRigion.MX;
        var bank = Bank.Default;
        var type = AffiliationType.Default;
        return new Affiliation(
            merchantId,
            terminalId,
            section,
            cardBrand,
            countryOrRigion,
            bank,
            type);
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

    public static Order CreateOrder(
        Guid buyerId,
        string street = "Street 1",
        string city = "City 1",
        string state = "State 1",
        string zipCode = "ZipCode 1",
        int productCount = 0)
    {
        var order = new Order.Builder()
            .WithBuyerId(buyerId)
            .WithAddress(street, city, state, zipCode)
            .WithDate(DateTime.Now)
            .Build();

        for (int i = 1; i <= productCount; i++)
        {
            order.AddProduct(Guid.NewGuid(), $"Product {i}", 10);
        }

        return order;
    }

    public static IModelBuilder CreateModelBuilderForOrder()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Order>()
            .HasPartitionKey(o => o.Id, "ORDER");
        modelBuilder.Entity<Order>()
            .HasSortKey(o => o.Id, "ORDER");
        modelBuilder.Entity<Order>()
            .HasOneToMany(o => o.Items);
        modelBuilder.Entity<Item>()
            .HasSortKey(o => o.Id, "ITEM");
        return modelBuilder;
    }
}
