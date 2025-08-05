using DynamoSharp.DynamoDb;
using DynamoSharp.DynamoDb.Configs;
using DynamoSharp.DynamoDb.ModelsBuilder;
using DynamoSharp.Tests.Contexts.Models.Movies;

namespace DynamoSharp.Tests.TestContexts;
public class MovieContext : DynamoSharpContext
{
    public IDynamoDbSet<Movie> Movies { get; private set; } = null!;
    public IDynamoDbSet<Actor> Actors { get; private set; } = null!;

    public MovieContext(IDynamoDbContextAdapter dynamoDbContextAdapter, TableSchema tableSchema) : base(dynamoDbContextAdapter, tableSchema)
    {
    }

    public override void OnModelCreating(IModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>()
            .HasManyToMany(m => m.Actors);

        modelBuilder.Entity<Actor>()
            .HasManyToMany(a => a.Movies);
    }
}
