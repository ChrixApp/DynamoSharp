using Amazon.DynamoDBv2.DocumentModel;
using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SortKeyWithHierarchicalDataQuery.DynamoDb;
using SortKeyWithHierarchicalDataQuery.Models;

namespace SortKeyWithHierarchicalDataQuery;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<StoreContext>(
            new TableSchema.Builder()
                .WithTableName("dynamosharp")
                .Build()
        );
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        
        var storeContext = serviceScope.ServiceProvider.GetRequiredService<StoreContext>();
        var stores = storeContext.Query<Store>()
            .IndexName("GSI1PK-GSI1SK-index")
            .PartitionKey("USA")
            .SortKey(QueryOperator.GreaterThanOrEqual, "TX#Houston")
            .ToListAsync()
            .Result;

        Console.ReadKey();
    }
}