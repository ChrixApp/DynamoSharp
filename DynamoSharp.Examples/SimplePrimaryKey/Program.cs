using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimplePrimaryKey.Context;
using SimplePrimaryKey.Models;

namespace SimplePrimaryKey;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<UserContext>(
            new TableSchema.Builder()
                .WithTableName("dynamosharp")
                .Build()
        );
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var userContext = serviceScope.ServiceProvider.GetRequiredService<UserContext>();

        var newUser = new User.Builder()
            .WithName("John Doe")
            .WithEmail("example@example.com")
            .WithPassword("1234567890")
            .Build();

        userContext.Users.Add(newUser);
        userContext.BatchWriter.SaveChangesAsync().Wait();

        Console.ReadKey();
    }
}
