namespace DynamoSharp.Tests.Contexts.Models.Movies;

public class Movie
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public int Year { get; private set; }
    public string Genre { get; private set; }
    public float IMDBScore { get; private set; }

    private readonly List<Performance> _actors;

    public IReadOnlyList<Performance> Actors => _actors.AsReadOnly();

    public Movie(string title, int year, string genre, float imdbScore)
    {
        Id = Guid.NewGuid();
        Title = title;
        Year = year;
        Genre = genre;
        IMDBScore = imdbScore;
        _actors = new();
    }

    public void AddActor(Actor actor, string roleName)
    {
        _actors.Add(new Performance(Id, actor.Id, Title, actor.Name, roleName));
    }

    public void RemoveActor(Actor actor)
    {
        _actors.RemoveAll(p => p.ActorName == actor.Name);
    }
}
