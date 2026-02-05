using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models.Affiliation;
using DynamoSharp.Tests.TestContexts.Models.Ecommerce;
using EfficientDynamoDb.DocumentModel;

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

    public static List<Document> CreateOrderDocuments()
    {
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
        return new List<Document>
        {
            CreateOrderDocument("ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af", "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af", "85cafc37-e6bb-4693-9283-f2eaec9828af", "68139DA0-A9F5-42FB-97FA-0585E9BCC8B1", dateString),
            new Document
            {
                ["PartitionKey"] = "ORDER#85cafc37-e6bb-4693-9283-f2eaec9828af",
                ["SortKey"] = "ITEM#3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["Id"] = "3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["ProductName"] = "Product 1",
                ["UnitPrice"] = new NumberAttributeValue("10.99"),
                ["Units"] = new NumberAttributeValue("1")
            },
        };
    }

    private static Document CreateOrderDocument(string partitionKey, string sortKey, string id, string buyerId, string dateString)
    {
        return new Document
        {
            ["PartitionKey"] = partitionKey,
            ["SortKey"] = sortKey,
            ["Id"] = id,
            ["BuyerId"] = buyerId,
            ["Address"] = new Document
            {
                ["Street"] = "Street 1",
                ["City"] = "City 1",
                ["State"] = "State 1",
                ["ZipCode"] = "ZipCode 1"
            },
            ["Date"] = dateString,
        };
    }

    public static List<Document> CreateActorDocuments()
    {
        return new List<Document>
        {
            new Document
            {
                ["MovieId"] = "dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["ActorId"] = "7d06c835-0ddc-4866-b42b-525221ded86c",
                ["MovieTitle"] = "3DD8F3EE-6445-4D2F-BEEE-2BED65C17ECD",
                ["ActorName"] = "The Matrix",
                ["RoleName"] = "Keanu Reeves",
                ["PartitionKey"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5",
                ["SortKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1PK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1SK"] = "MOVIE#dfcf58ec-0127-41df-b10c-9c3da1ce6da5"
            },
            new Document
            {
                ["MovieId"] = "d0316fa4-ac1b-49d4-8404-41158503721c",
                ["ActorId"] = "7d06c835-0ddc-4866-b42b-525221ded86c",
                ["MovieTitle"] = "John Wick",
                ["ActorName"] = "Keanu Reeves",
                ["RoleName"] = "John Wick",
                ["PartitionKey"] = "MOVIE#d0316fa4-ac1b-49d4-8404-41158503721c",
                ["SortKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1PK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1SK"] = "MOVIE#d0316fa4-ac1b-49d4-8404-41158503721c"
            },
            new Document
            {
                ["Id"] = "7d06c835-0ddc-4866-b42b-525221ded86c",
                ["Name"] = "Keanu Reevez",
                ["BirthDate"] = "1964-09-02T00:00:00",
                ["PartitionKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["SortKey"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1PK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c",
                ["GSI1SK"] = "ACTOR#7d06c835-0ddc-4866-b42b-525221ded86c"
            }
        };
    }
}
