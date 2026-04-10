using ChildAndParent.DynamoDb;
using ChildAndParent.Models;
using DynamoSharp;
using DynamoSharp.DynamoDb.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ChildAndParent;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDynamoSharp(RegionEndpoint.USEast1, "http://localhost:8000");
        builder.Services.AddDynamoSharpContext<OrganizationContext>(
            new TableSchema.Builder()
                .WithTableName("sanvalplus-sherlock")
                .Build()
        );
        var app = builder.Build();

        using var serviceScope = app.Services.CreateScope();
        var organizationContext = serviceScope.ServiceProvider.GetRequiredService<OrganizationContext>();
        var organizations = CreateOrganizations();

        foreach (var organization in organizations)
            organizationContext.Organizations.Add(organization);

        organizationContext.BatchWriter.SaveChangesAsync().Wait();

        var organization1 = organizationContext.Query<PrivateOrganization>()
            .PartitionKey("ORG#Organization1")
            .ToEntityAsync()
            .Result;

        var organization2 = organizationContext.Query<PrivateOrganization>()
            .PartitionKey("ORG#Organization2")
            .ToEntityAsync()
            .Result;

        if (!(organization1 is 
            {   Name : "Organization1",
                SubscriptionLevel: SubscriptionLevel.Pro,
                Users.Count: 2,
                CEO: "CEO1",
                CTO: "CTO1" 
            } ))
        {
            throw new Exception("organization1 values do not match the expected values.");
        }

        if(!(organization2 is 
            {   Name : "Organization2",
                SubscriptionLevel: SubscriptionLevel.Enterprise,
                Users.Count: 6,
                CEO: "CEO2",
                CTO: "CTO2" 
            } ))
        {
            throw new Exception("organization2 values do not match the expected values.");
        }

        // TODO: I change the Users List from Organization to PrivateOrganization, this because in Organization cause an error when I try to get the data.
    }

    private static List<PrivateOrganization> CreateOrganizations()
    {
        var organizations = new List<PrivateOrganization>();

        var organization1 = new PrivateOrganization("Organization1", SubscriptionLevel.Pro, "CEO1", "CTO1");
        organization1.AddUser("User1", SubscriptionLevel.Member);
        organization1.AddUser("User2", SubscriptionLevel.Admin);
        organizations.Add(organization1);

        var organization2 = new PrivateOrganization("Organization2", SubscriptionLevel.Enterprise, "CEO2", "CTO2");
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
