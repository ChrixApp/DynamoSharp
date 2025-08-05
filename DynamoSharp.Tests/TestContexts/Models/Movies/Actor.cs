﻿namespace DynamoSharp.Tests.Contexts.Models.Movies;

public partial class Actor
{
    public Guid Id { get; private set; }
    public string Name { get; set; }
    public DateTime BirthDate { get; set; }

    private readonly List<Performance> _movies;

    public IReadOnlyList<Performance> Movies => _movies.AsReadOnly();

    public Actor(string name, DateTime birthDate)
    {
        Id = Guid.NewGuid();
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
        _movies.Add(new Performance(movie.Id, Id, movie.Title, Name, roleName));
    }
}
