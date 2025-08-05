namespace ManyToManyWithCustomPrimaryKeyQuery.Models;

public class Performance
{
    public string MovieTitle { get; private set; }
    public string ActorName { get; private set; }
    public string RoleName { get; private set; }

    public Performance(string movieTitle, string actorName, string roleName)
    {
        MovieTitle = movieTitle;
        ActorName = actorName;
        RoleName = roleName;
    }

    public void ChangeRole(string roleName)
    {
        RoleName = roleName;
    }
}
