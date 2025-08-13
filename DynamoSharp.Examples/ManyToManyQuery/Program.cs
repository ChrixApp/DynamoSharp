using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using ManyToManyQuery.Context;
using ManyToManyQuery.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ManyToManyQuery;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<MovieContext>(
            new TableSchema.Builder()
                .WithTableName("dynamosharp")
                .Build()
        );
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var movieContext = serviceScope.ServiceProvider.GetRequiredService<MovieContext>();

        var theMatrix = movieContext.Query<Movie>()
            .PartitionKey("MOVIE#9184826c-fcea-4aaf-b4bb-283af20c76df")
            .ToEntityAsync()
            .Result;

        var keanuReeves = movieContext.Query<Actor>()
            .IndexName("GSI1PK-GSI1SK-index")
            .PartitionKey("ACTOR#c9a7b7d5-7c25-4ef2-bf15-0305dd42be25")
            .ToEntityAsync()
            .Result;

        Console.ReadKey();
    }
}
