namespace SparseIndex.Models;

public class Organization
{
    public string Name { get; private set; }
    public SubscriptionLevel SubscriptionLevel { get; private set; }

    private readonly List<User> _users = new List<User>();
    public IReadOnlyCollection<User> Users => _users;

    public Organization(string name, SubscriptionLevel subscriptionLevel)
    {
        Name = name;
        SubscriptionLevel = subscriptionLevel;
    }

    public void AddUser(string name, SubscriptionLevel subscriptionLevel)
    {
        _users.Add(new User(name, subscriptionLevel, Name));
    }

    public void RemoveUser(string name)
    {
        var user = _users.FirstOrDefault(u => u.Name == name);
        if (user != null)
        {
            _users.Remove(user);
        }
    }
}
