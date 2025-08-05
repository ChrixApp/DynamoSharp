namespace SparseIndex.Models;

public class User
{
    public string Name { get; private set; }
    public SubscriptionLevel SubscriptionLevel { get; private set; }
    public string OrganizationName { get; private set; }

    public User(string name, SubscriptionLevel subscriptionLevel, string organizationName)
    {
        Name = name;
        SubscriptionLevel = subscriptionLevel;
        OrganizationName = organizationName;
    }
}
