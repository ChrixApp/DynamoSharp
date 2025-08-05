using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SortKeyWithHierarchicalData.DynamoDb;
using SortKeyWithHierarchicalData.Models;

namespace SortKeyWithHierarchicalData;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<StoreContext>("eska");
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var storeContext = serviceScope.ServiceProvider.GetRequiredService<StoreContext>();

        var stores = CreateStores();

        foreach (var store in stores)
            storeContext.Stores.Add(store);

        storeContext.BatchWriter.WriteAsync().Wait();

        Console.ReadKey();
    }

    private static List<Store> CreateStores()
    {
        var stores = new List<Store>();

        var store1 = CreateStore("83293159-3573-4d04-92ff-4ad047d95481", "Store 1", "123456789", "store1@store.com", "Street 1", "Houston", "TX", "77000", "USA");
        var store2 = CreateStore("6bc1bdd6-91dc-47ea-9d6a-f87130be1cb5", "Store 2", "123456789", "store2@store.com", "Street 2", "Houston", "TX", "77000", "USA");
        var store3 = CreateStore("85ea4b92-9c3e-4025-ae9c-64d9d452522c", "Store 3", "123456789", "store3@store.com", "Street 3", "Dallas", "TX", "75201", "USA");
        var store4 = CreateStore("ec381c7c-3a78-4146-9adc-cd8bf124c327", "Store 4", "123456789", "store4@store.com", "Street 4", "CDMX", "TX", "01000", "MX");

        stores.Add(store1);
        stores.Add(store2);
        stores.Add(store3);
        stores.Add(store4);

        return stores;
    }

    private static Store CreateStore(string id, string name, string phone, string email, string street, string city, string state, string zipCode, string country)
    {
        return new Store.Builder()
            .WithId(Guid.Parse(id))
            .WithName(name)
            .WithPhone(phone)
            .WithEmail(email)
            .WithAddress(street, city, state, zipCode, country)
            .Build();
    }
}