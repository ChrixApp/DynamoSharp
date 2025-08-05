namespace ManyToMany.Models;

public class Movie
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public int Year { get; private set; }
    public string Genre { get; private set; }
    public float IMDBScore { get; private set; }

    private List<Performance> _actors;

    public IReadOnlyList<Performance> Actors
    {
        get => _actors.AsReadOnly();
        private set
        {
            if (value is not null) _actors = value.ToList();
        }
    }

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
