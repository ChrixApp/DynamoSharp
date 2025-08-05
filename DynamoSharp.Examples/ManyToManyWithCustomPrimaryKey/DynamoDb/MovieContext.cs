using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using ManyToManyWithCustomPrimaryKey.Models;

namespace ManyToManyWithCustomPrimaryKey.DynamoDb;

public class MovieContext : DynamoSharpContext
{
    public IDynamoDbSet<Movie> Movies { get; private set; } = null!;
    public IDynamoDbSet<Actor> Actors { get; private set; } = null!;

    public MovieContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        MovieModelBuilder(modelBuilder);

        ActorModelBuilder(modelBuilder);

        PerformanceModelBuilder(modelBuilder);
    }

    private static void MovieModelBuilder(IModelBuilder modelBuilder)
    {
        // Example Partition Key: MOVIE#The Matrix
        modelBuilder.Entity<Movie>()
            .HasPartitionKey(m => m.Title, "MOVIE");

        // Example Sort Key: MOVIE#The Matrix
        modelBuilder.Entity<Movie>()
            .HasSortKey(m => m.Title, "MOVIE");

        modelBuilder.Entity<Movie>()
            .HasManyToMany(m => m.Actors);
    }

    private static void ActorModelBuilder(IModelBuilder modelBuilder)
    {
        // Example Partition Key: ACTOR#Keanu Reeves
        modelBuilder.Entity<Actor>()
            .HasPartitionKey(a => a.Name, "ACTOR");

        // Example Sort Key: ACTOR#Keanu Reeves
        modelBuilder.Entity<Actor>()
            .HasSortKey(a => a.Name, "ACTOR");

        modelBuilder.Entity<Actor>()
            .HasManyToMany(a => a.Movies);

        // Example Global Secondary Index Partition Key: ACTOR#Keanu Reeves
        modelBuilder.Entity<Actor>()
            .HasGlobalSecondaryIndexPartitionKey("GSI1PK", a => a.Name, "ACTOR");

        // Example Global Secondary Index Sort Key: ACTOR#Keanu Reeves
        modelBuilder.Entity<Actor>()
            .HasGlobalSecondaryIndexSortKey("GSI1SK", a => a.Name, "ACTOR");
    }

    private static void PerformanceModelBuilder(IModelBuilder modelBuilder)
    {
        // Example Partition Key: MOVIE#The Matrix
        modelBuilder.Entity<Performance>()
            .HasPartitionKey(p => p.MovieTitle, "MOVIE");

        // Example Sort Key: ACTOR#Keanu Reeves
        modelBuilder.Entity<Performance>()
            .HasSortKey(p => p.ActorName, "ACTOR");

        // Example Global Secondary Index Partition Key: ACTOR#Keanu Reeves
        modelBuilder.Entity<Performance>()
            .HasGlobalSecondaryIndexPartitionKey("GSI1PK", p => p.ActorName, "ACTOR");

        // Example Global Secondary Index Sort Key: MOVIE#The Matrix
        modelBuilder.Entity<Performance>()
            .HasGlobalSecondaryIndexSortKey("GSI1SK", p => p.MovieTitle, "MOVIE");
    }
}
