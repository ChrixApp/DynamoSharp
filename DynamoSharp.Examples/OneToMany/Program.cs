using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OneToMany.Context;
using OneToMany.Models;
using System.Reflection;

namespace OneToMany;

public static class Program
{
    static async Task Main(string[] args)
    {
        PropertyInspector.Inspect<Order>();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1, "http://localhost:4566");
        builder.Services.AddDynamoSharpContext<EcommerceContext>(
            new TableSchema.Builder()
                .WithTableName("ecommerce")
                .AddGlobalSecondaryIndex("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK")
                .Build()
        );
        var app = builder.Build();

        var buyerId = Guid.Parse("6dbefed7-1d09-40ca-9733-b1667efb95f3");
        var street = "Street 1";
        var city = "City 1";
        var state = "State 1";
        var zipCode = "ZipCode 1";

        var newOrder = new Order.Builder()
            .WithId(Guid.Parse("3002781c-e6de-4035-a2a9-b0f7641305bd"))
            .WithBuyerId(buyerId)
            .WithAddress(street, city, state, zipCode)
            .WithStatus(Status.Pending)
            .WithDate(DateTime.Now)
            .Build();

        newOrder.AddProduct(Guid.Parse("b3bc0076-03c9-4705-985d-39c0736680ef"), "Product 1", 10, 100);
        newOrder.AddProduct(Guid.Parse("16e999e5-5a94-4171-ab1b-cd53ddc769e3"), "Product 2", 10, 100);

        using var serviceScope = app.Services.CreateScope();
        var ecommerceContext = serviceScope.ServiceProvider.GetRequiredService<EcommerceContext>();
        ecommerceContext.Orders.Add(newOrder);
        ecommerceContext.BatchWriter.SaveChangesAsync().Wait();

        var order = ecommerceContext.Query<Order>()
            .PartitionKey($"ORDER#{newOrder.Id}")
            .AsNoTracking()
            .ToEntityAsync()
            .Result;

        Console.ReadKey();
    }
}

public static class PropertyInspector
{
    private static bool HasCompilerBackingField(Type type, PropertyInfo p) =>
        type.GetField($"<{p.Name}>k__BackingField",
                      BindingFlags.NonPublic | BindingFlags.Instance) != null;

    public static bool IsComputedProperty(Type type, PropertyInfo p) =>
        p.CanRead
        && p.GetMethod != null
        && !HasCompilerBackingField(type, p);

    public static bool IsAutoProperty(Type type, PropertyInfo p) =>
        HasCompilerBackingField(type, p);

    public static void Inspect<T>()
    {
        var t = typeof(T);
        Console.WriteLine($"Type: {t.Name}");
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            string kind = IsComputedProperty(t, p) ? "Calculada"
                        : IsAutoProperty(t, p) ? "Auto-property"
                        : "Manual (getter/setter personalizados)";
            Console.WriteLine($"{p.Name} : {p.PropertyType.Name} => {kind}");
        }
    }
}