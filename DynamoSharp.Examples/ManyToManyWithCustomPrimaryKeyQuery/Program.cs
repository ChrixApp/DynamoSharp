using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using ManyToManyWithCustomPrimaryKeyQuery.DynamoDb;
using ManyToManyWithCustomPrimaryKeyQuery.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ManyToManyWithCustomPrimaryKeyQuery;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<MovieContext>(
            "eska",
            new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK"));
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var movieContext = serviceScope.ServiceProvider.GetRequiredService<MovieContext>();

        //var theMatrix = movieRepository.Get("MOVIE#The Matrix", cancellationToken: default).Result;
        //var keanuReeves = actorRepository.Get("GSI1PK-GSI1SK-index", "ACTOR#Keanu Reeves", cancellationToken: default).Result;

        var theMatrix = movieContext.Query<Movie>()
            .PartitionKey("MOVIE#The Matrix")
            .ToEntityAsync()
            .Result;

        var keanuReeves = movieContext.Query<Actor>()
            .IndexName("GSI1PK-GSI1SK-index")
            .PartitionKey("ACTOR#Keanu Reeves")
            .ToEntityAsync()
            .Result;

        keanuReeves?.ChangeRole("The Matrix", "Neo");
        movieContext.TransactWriter.WriteAsync().Wait();

        Console.ReadKey();
    }
}
