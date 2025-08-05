using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PrimaryKeyUsingNestedProperties.Context;
using PrimaryKeyUsingNestedProperties.Models;

namespace PrimaryKeyUsingNestedProperties;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<StoreContext>("eska");
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var storeContext = serviceScope.ServiceProvider.GetRequiredService<StoreContext>();

        var store = new Store.Builder()
            .WithId(Guid.NewGuid())
            .WithName("Store 1")
            .WithPhone("123456789")
            .WithEmail("store@example.com")
            .WithAddress("street", "city", "state", "000000", "country")
            .Build();

        storeContext.Stores.Add(store);
        storeContext.BatchWriter.SaveChangesAsync().Wait();

        store.ChangeAddress("new street", "new city", "new state", "111111", "new country");

        storeContext.BatchWriter.SaveChangesAsync().Wait();

        Console.ReadKey();
    }
}
