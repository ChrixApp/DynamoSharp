namespace DynamoSharp.Tests.Contexts.Models.Movies;

public class Performance
{
    public Guid MovieId { get; private set; }
    public Guid ActorId { get; private set; }
    public string MovieTitle { get; private set; }
    public string ActorName { get; private set; }
    public string RoleName { get; private set; }

    public Performance(Guid movieId, Guid actorId, string movieTitle, string actorName, string roleName)
    {
        MovieId = movieId;
        ActorId = actorId;
        MovieTitle = movieTitle;
        ActorName = actorName;
        RoleName = roleName;
    }
}
