using Amazon.DynamoDBv2.DocumentModel;
using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using GlobalSecondaryIndexQueryPkAndSk.DynamoDb;
using GlobalSecondaryIndexQueryPkAndSk.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GlobalSecondaryIndexQueryPkAndSk;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<EcommerceContext>("eska");
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        //var order = orderRepository.GetList("GSI1PK-GSI1SK-index", "BUYER#de8fd122-03c4-4980-92c8-cd03fd3458db", QueryOperator.BeginsWith, "Delivered", default).Result;
        var orderContext = serviceScope.ServiceProvider.GetRequiredService<EcommerceContext>();
        var orders = orderContext.Query<Order>()
            .IndexName("GSI1PK-GSI1SK-index")
            .PartitionKey("BUYER#de8fd122-03c4-4980-92c8-cd03fd3458db")
            .SortKey(QueryOperator.BeginsWith, "Delivered")
            .ToListAsync()
            .Result;

        Console.ReadKey();
    }
}