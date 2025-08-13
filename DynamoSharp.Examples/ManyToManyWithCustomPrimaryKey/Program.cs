using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using ManyToManyWithCustomPrimaryKey.DynamoDb;
using ManyToManyWithCustomPrimaryKey.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ManyToManyWithCustomPrimaryKey;

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

        var actors = new List<Actor>();
        var keanuReeves = new Actor("Keanu Reeves", DateTime.Parse("1964-09-02"));
        var carrieAnneMoss = new Actor("Carrie-Anne Moss", DateTime.Parse("1967-08-21"));
        var elijahWood = new Actor("Elijah Wood", DateTime.Parse("1981-01-28"));
        actors.Add(keanuReeves);
        actors.Add(carrieAnneMoss);
        actors.Add(elijahWood);
        movieContext.Actors.AddRange(actors);

        var movies = new List<Movie>();
        var theMatrix = new Movie("The Matrix", 1999, "Science Fiction", 8.7f);
        theMatrix.AddActor(keanuReeves, "NEO");
        theMatrix.AddActor(carrieAnneMoss, "Trinity");
        var johnWick = new Movie("John Wick", 2014, "Action", 7.4f);
        johnWick.AddActor(keanuReeves, "John Wick");
        movies.Add(theMatrix);
        movies.Add(johnWick);
        var theLordOfTheRings = new Movie("The Lord of the Rings: The Fellowship of the Ring", 2001, "Adventure", 8.8f);
        theLordOfTheRings.AddActor(elijahWood, "Frodo");
        movieContext.Movies.AddRange(movies);
        movieContext.TransactWriter.SaveChangesAsync().Wait();

        Console.ReadKey();
    }
}