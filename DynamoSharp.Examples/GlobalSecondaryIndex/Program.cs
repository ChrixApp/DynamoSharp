using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using GlobalSecondaryIndex.DynamoDb;
using GlobalSecondaryIndex.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GlobalSecondaryIndex;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<EcommerceContext>("eska");
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var orderContext = serviceScope.ServiceProvider.GetRequiredService<EcommerceContext>();
        var orders = CreateOrders();

        foreach (var order in orders)
        {
            orderContext.Orders.Add(order);
        }
        orderContext.TransactWriter.WriteAsync().Wait();

        Console.ReadKey();
    }

    private static List<Order> CreateOrders()
    {
        var orders = new List<Order>();

        var buyerId1 = "de8fd122-03c4-4980-92c8-cd03fd3458db";
        var buyerId2 = "6d1299c2-72e4-40a2-85e4-2a0a2aaed4e5";

        var orderId1 = "a025f2ed-40f2-498d-8c99-b26f36728a81";
        var orderId2 = "26624158-6b02-4533-aff9-b1805a716feb";
        var orderId3 = "4e2e3f30-75c7-446b-8846-203eb427e0c5";
        var orderId4 = "9652cb7d-191a-4388-a152-9008aca38b2d";

        var street = "Street 1";
        var city = "City 1";
        var state = "State 1";
        var zipCode = "ZipCode 1";

        var productId1 = "9652cb7d-191a-4388-a152-9008aca38b2d";
        var productId2 = "dd1ee588-5e73-423e-8816-6659f18358a5";

        var productName1 = "Product 1";
        var productName2 = "Product 2";

        var order1 = CreateOrder(orderId1, buyerId1, Status.Cancelled, street, city, state, zipCode, "2024-08-08T23:47:59.8170928-06:00", productId1, productName1, 100, 2);
        var order2 = CreateOrder(orderId2, buyerId1, Status.Delivered, street, city, state, zipCode, "2024-07-08T23:47:59.8170928-06:00", productId2, productName2, 200, 3);
        var order3 = CreateOrder(orderId3, buyerId1, Status.Delivered, street, city, state, zipCode, "2024-07-08T23:47:59.8170928-06:00", productId2, productName2, 200, 3);
        var order4 = CreateOrder(orderId4, buyerId2, Status.Shipped, street, city, state, zipCode, "2024-08-08T23:47:59.8170928-06:00", productId1, productName1, 100, 2);

        orders.Add(order1);
        orders.Add(order2);
        orders.Add(order3);
        orders.Add(order4);

        return orders;
    }

    private static Order CreateOrder(string orderId, string buyerId, Status status, string street, string city, string state, string zipCode, string date, string productId, string productName, int unitPrice, int units)
    {
        var order = new Order.Builder()
            .WithId(Guid.Parse(orderId))
            .WithBuyerId(Guid.Parse(buyerId))
            .WithStatus(status)
            .WithAddress(street, city, state, zipCode)
            .WithDate(DateTime.Parse(date))
            .Build();
        order.AddProduct(Guid.Parse(productId), productName, unitPrice, units);
        return order;
    }
}