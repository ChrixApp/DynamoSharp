using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OptimisticLockingSave.Context;
using OptimisticLockingSave.Models;

namespace OptimisticLockingSave;

public class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1, "http://localhost:4566/");
        // 'v' represents the version attribute for optimistic locking
        builder.Services.AddDynamoSharpContext<EcommerceContext>("eska", "v");
        var app = builder.Build();

        var buyerId = Guid.Parse("6dbefed7-1d09-40ca-9733-b1667efb95f3");
        var street = "Street 1";
        var city = "City 1";
        var state = "State 1";
        var zipCode = "ZipCode 1";

        var order = new Order.Builder()
            .WithId(Guid.Parse("3002781c-e6de-4035-a2a9-b0f7641305bd"))
            .WithBuyerId(buyerId)
            .WithAddress(street, city, state, zipCode)
            .WithStatus(Status.Pending)
            .WithDate(DateTime.Now)
            .Build();

        order.AddProduct(Guid.Parse("b3bc0076-03c9-4705-985d-39c0736680ef"), "Product 1", 10, 100);

        using var serviceScope = app.Services.CreateScope();
        var ecommerceContext = serviceScope.ServiceProvider.GetRequiredService<EcommerceContext>();

        ecommerceContext.Orders.Add(order);
        ecommerceContext.TransactWriter.SaveChangesAsync().Wait();


        Console.ReadKey();
    }
}
