namespace ManyToManyWithCustomPrimaryKeyQuery.Models;

public partial class Actor
{
    public string Name { get; set; }
    public DateTime BirthDate { get; set; }

    private List<Performance> _movies;

    public IReadOnlyList<Performance> Movies
    {
        get => _movies.AsReadOnly();
        private set
        {
            if (value is not null) _movies = value.ToList();
        }
    }

    public Actor(string name, DateTime birthDate)
    {
        Name = name;
        BirthDate = birthDate;
        _movies = new();
    }

    public void Rename(string name)
    {
        Name = name;
    }

    public void AddMovie(Movie movie, string roleName)
    {
        ArgumentNullException.ThrowIfNull(movie, nameof(movie));
        _movies.Add(new Performance(movie.Title, Name, roleName));
    }

    public void ChangeRole(string movieTitle, string roleName)
    {
        ArgumentNullException.ThrowIfNull(movieTitle, nameof(movieTitle));
        ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));
        var performance = _movies.FirstOrDefault(p => p.MovieTitle == movieTitle);
        if (performance is not null) performance.ChangeRole(roleName);
    }
}
