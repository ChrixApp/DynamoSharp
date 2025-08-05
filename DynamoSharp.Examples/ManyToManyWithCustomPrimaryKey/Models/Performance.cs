namespace ManyToManyWithCustomPrimaryKey.Models;

public class Performance
{
    public string MovieTitle { get; set; }
    public string ActorName { get; set; }
    public string RoleName { get; set; }

    public Performance(string movieTitle, string actorName, string roleName)
    {
        MovieTitle = movieTitle;
        ActorName = actorName;
        RoleName = roleName;
    }
}
