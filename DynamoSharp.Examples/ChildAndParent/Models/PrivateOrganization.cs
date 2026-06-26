namespace ChildAndParent.Models;

public class PrivateOrganization : Organization
{
    public string CEO { get; private set; }
    public string CTO { get; private set; }

    private readonly List<User> _users = new List<User>();
    public IReadOnlyCollection<User> Users => _users;

    public PrivateOrganization(string name, SubscriptionLevel subscriptionLevel, string ceo, string cto)
        : base(name, subscriptionLevel)
    {
        CEO = ceo;
        CTO = cto;
    }

    public void AddUser(string name, SubscriptionLevel subscriptionLevel)
    {
        _users.Add(new User(name, subscriptionLevel, Name));
    }
}
