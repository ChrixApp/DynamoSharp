using Amazon.DynamoDBv2.DocumentModel;
using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pagination.DynamoDB;
using Pagination.Models;

namespace Pagination;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1);
        builder.Services.AddDynamoSharpContext<OrganizationContext>("eska");
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var organizationContext = serviceScope.ServiceProvider.GetRequiredService<OrganizationContext>();

        var organizations = CreateOrganizations();

        foreach (var organization in organizations)
            organizationContext.Organizations.Add(organization);

        organizationContext.TransactWriter.WriteAsync().Wait();

        // User3, User4
        var userPage1 = organizationContext.Query<User>()
            .PartitionKey("ORG#Organization2")
            .SortKey(QueryOperator.GreaterThan, "ORG#Organization2")
            .Limit(2)
            .ToListAsync()
            .Result;

        // User5, User6
        var userPage2 = organizationContext.Query<User>()
            .PartitionKey("ORG#Organization2")
            .SortKey(QueryOperator.GreaterThan, $"USER#{userPage1[userPage1.Count - 1].Name}")
            .Limit(2)
            .ToListAsync()
            .Result;

        // User7, User8
        var userPage3 = organizationContext.Query<User>()
            .PartitionKey("ORG#Organization2")
            .SortKey(QueryOperator.GreaterThan, $"USER#{userPage2[userPage2.Count - 1].Name}")
            .Limit(2)
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
