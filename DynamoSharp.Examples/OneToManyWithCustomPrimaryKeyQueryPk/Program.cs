using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OneToManyWithCustomPrimaryKeyQueryPk.DynamoDb;
using OneToManyWithCustomPrimaryKeyQueryPk.Models;

namespace OneToManyWithCustomPrimaryKeyQueryPk;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<EcommerceContext>(
            new TableSchema.Builder()
                .WithTableName("dynamosharp")
                .Build()
        );
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var ecommerceContext = serviceScope.ServiceProvider.GetRequiredService<EcommerceContext>();
        
        var order = ecommerceContext.Query<Order>()
            .PartitionKey("ORDER#3002781c-e6de-4035-a2a9-b0f7641305bd")
            .ToEntityAsync()
            .Result;

        Console.ReadKey();
    }
}