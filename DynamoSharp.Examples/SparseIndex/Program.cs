using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SparseIndex.DynamoDb;
using SparseIndex.Models;

namespace SparseIndex;

public class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1, "http://localhost:4566/");
        builder.Services.AddDynamoSharpContext<OrganizationContext>(
            "eska",
            new GlobalSecondaryIndexSchema("GSI1PK-GSI1SK-index", "GSI1PK", "GSI1SK"));
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();

        var organizationContext = serviceScope.ServiceProvider.GetRequiredService<OrganizationContext>();
        var organizations1 = CreateOrganizations();

        organizationContext.Organizations.AddRange(organizations1);
        organizationContext.BatchWriter.SaveChangesAsync().Wait();

        var organizations2 = organizationContext.Query<Organization>()
            .IndexName("GSI1PK-GSI1SK-index")
            .PartitionKey("ORGANIZATIONS")
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
