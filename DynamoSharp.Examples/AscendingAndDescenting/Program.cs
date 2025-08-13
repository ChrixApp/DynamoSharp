using Amazon.DynamoDBv2.DocumentModel;
using AscendingAndDescenting.DynamoDb;
using AscendingAndDescenting.Models;
using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AscendingAndDescenting;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<OrganizationContext>(
            new TableSchema.Builder()
                .WithTableName("dynamosharp")
                .Build()
        );
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var organizationContext = serviceScope.ServiceProvider.GetRequiredService<OrganizationContext>();
        var organizations = CreateOrganizations();

        foreach (var organization in organizations)
            organizationContext.Organizations.Add(organization);

        organizationContext.BatchWriter.SaveChangesAsync().Wait();

        // the following code to order the results in ascending order
        // User3, User4
        var ascending = organizationContext.Query<User>()
            .PartitionKey("ORG#Organization2")
            .SortKey(QueryOperator.GreaterThan, "ORG#Organization2")
            .Limit(2)
            .ToListAsync()
            .Result;

        // The same query but use ScanIndexForward to order the results in descending order
        // User8, User7
        var descending = organizationContext.Query<User>()
            .PartitionKey("ORG#Organization2")
            .SortKey(QueryOperator.GreaterThan, "ORG#Organization2")
            .Limit(2)
            .ScanIndexForward()
            .ToListAsync()
            .Result;

        Console.ReadKey();
    }

    private static List<Organization> CreateOrganizations()
    {
        var organizations = new List<Organization>();

        var organization1 = new Organization("Organization1", SubscriptionLevel.Pro);
        organization1.AddUser("User1", SubscriptionLevel.Member);
        organization1.AddUser("User2", SubscriptionLevel.Admin);
        organizations.Add(organization1);

        var organization2 = new Organization("Organization2", SubscriptionLevel.Enterprise);
        organization2.AddUser("User3", SubscriptionLevel.Member);
        organization2.AddUser("User4", SubscriptionLevel.Admin);
        organization2.AddUser("User5", SubscriptionLevel.Member);
        organization2.AddUser("User6", SubscriptionLevel.Member);
        organization2.AddUser("User7", SubscriptionLevel.Member);
        organization2.AddUser("User8", SubscriptionLevel.Member);
        organizations.Add(organization2);

        return organizations;
    }
}
