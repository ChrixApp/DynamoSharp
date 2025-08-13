using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OptimisticLockingUpdate.Context;
using OptimisticLockingUpdate.Models;

namespace OptimisticLockingUpdate;

internal class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1, "http://localhost:4566/");
        // 'v' represents the version attribute for optimistic locking
        builder.Services.AddDynamoSharpContext<EcommerceContext>(
            new TableSchema.Builder()
                .WithTableName("dynamosharp")
                .WithVersionName("v")
                .Build()
        );
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var ecommerceContext = serviceScope.ServiceProvider.GetRequiredService<EcommerceContext>();

        var order = ecommerceContext.Query<Order>()
           .PartitionKey("ORDER#3002781c-e6de-4035-a2a9-b0f7641305bd")
           .ToEntityAsync()
           .Result;

        order?.UpdateAddress("Street 2", "City 2", "State 2", "ZipCode 2");
        ecommerceContext.TransactWriter.SaveChangesAsync().Wait();
    }
}
