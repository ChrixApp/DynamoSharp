using Newtonsoft.Json;
using System.Collections;
using System.Reflection;
using DynamoSharp.Converters.Jsons;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models;

namespace DynamoSharp.Tests.ChangeTracking;

public static class EntityChangeTrackerTestDataFactory
{
    public static Order CreateOrder(Guid buyerId, string street, string city, string state, string zipCode, int productCount = 0)
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

    public static IModelBuilder CreateModelBuilder()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Order>().HasPartitionKey(o => o.Id, "ORDER");
        modelBuilder.Entity<Order>().HasSortKey(o => o.Id, "ORDER");
        modelBuilder.Entity<Order>().HasOneToMany(o => o.Items);
        modelBuilder.Entity<Item>().HasSortKey(o => o.Id, "ITEM");

        return modelBuilder;
    }

    public static int CountPropertiesExcludingLists(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Count(prop => !typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) || prop.PropertyType == typeof(string));
    }

    public static JsonSerializer GetJsonSerializer(IEntityTypeBuilder entityTypeBuilder)
    {
        var collectionsToIgnore = GetCollectionsToIgnore(entityTypeBuilder);
        return JsonSerializerBuilder.Build(collectionsToIgnore);
    }

    private static List<string> GetCollectionsToIgnore(IEntityTypeBuilder entityTypeBuilder)
    {
        var collectionsToIgnore = entityTypeBuilder.OneToMany.Select(otm => otm.Key).ToList();
        collectionsToIgnore.AddRange(entityTypeBuilder.ManyToMany.Select(mtm => mtm.Key).ToList());
        return collectionsToIgnore;
    }
}
