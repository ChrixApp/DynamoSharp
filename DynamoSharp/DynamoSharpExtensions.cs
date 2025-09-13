using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime.Credentials;
using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using Microsoft.Extensions.DependencyInjection;
using EfficientDynamoDbRegionEndpoint = EfficientDynamoDb.Configs.RegionEndpoint;

namespace DynamoSharp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDynamoSharp(this IServiceCollection serviceCollection, RegionEndpoint regionEndpoint)
    {
        return serviceCollection.AddDynamoSharp(regionEndpoint, null!);
    }

    public static IServiceCollection AddDynamoSharp(this IServiceCollection serviceCollection, RegionEndpoint regionEndpoint, string requestUri)
    {
        return serviceCollection.AddDynamoSharp(regionEndpoint.Region, requestUri);
    }

    public static IServiceCollection AddDynamoSharp(this IServiceCollection serviceCollection, string region)
    {
        return serviceCollection.AddDynamoSharp(region, null!);
    }

    private static IServiceCollection AddDynamoSharp(this IServiceCollection serviceCollection, string region, string requestUri)
    {
        serviceCollection.AddDefaultAWSOptions(new AWSOptions
        {
            Region = Amazon.RegionEndpoint.GetBySystemName(region)
        });
        serviceCollection.AddAWSService<IAmazonDynamoDB>();
        serviceCollection.AddSingleton<IDynamoDbContext, DynamoDbContext>(context =>
        {
            var regionEndpoint = string.IsNullOrWhiteSpace(requestUri) ? EfficientDynamoDbRegionEndpoint.Create(region) : EfficientDynamoDbRegionEndpoint.Create(region, requestUri);
            var awsSdkCredentials = DefaultAWSCredentialsIdentityResolver.GetCredentials();
            var effDdbCredentials = awsSdkCredentials.ToCredentialsProvider();
            var config = new DynamoDbContextConfig(regionEndpoint, effDdbCredentials);
            return new DynamoDbContext(config);
        });
        return serviceCollection;
    }

    public static IServiceCollection AddDynamoSharpContext<TContext>(
        this IServiceCollection serviceCollection,
        TableSchema tableSchema)
    where TContext : class, IDynamoSharpContext
    {
        serviceCollection.AddScoped<IDynamoDbContextAdapter, DynamoDbContextAdapter>();
        
        serviceCollection.AddScoped<IDynamoSharpContext, TContext>(s =>
        {
            var objFact = ActivatorUtilities.CreateFactory(typeof(TContext), new Type[] { typeof(TableSchema), });
            var instance = (TContext)objFact.Invoke(s, new[] { tableSchema });
            instance.OnModelCreating(instance.ModelBuilder);
            instance.Registration();
            return instance;
        });

        serviceCollection.AddScoped(s =>
        {
            var objFact = ActivatorUtilities.CreateFactory(typeof(TContext), new Type[] { typeof(TableSchema) });
            var instance = (TContext)objFact.Invoke(s, new[] { tableSchema });
            instance.OnModelCreating(instance.ModelBuilder);
            instance.Registration();
            return instance;
        });

        return serviceCollection;
    }
}