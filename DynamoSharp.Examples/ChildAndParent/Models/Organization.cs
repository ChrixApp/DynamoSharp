namespace ChildAndParent.Models;

public class Organization
{
    public string Name { get; private set; }
    public SubscriptionLevel SubscriptionLevel { get; private set; }

    public Organization(string name, SubscriptionLevel subscriptionLevel)
    {
        Name = name;
        SubscriptionLevel = subscriptionLevel;
    }
}
