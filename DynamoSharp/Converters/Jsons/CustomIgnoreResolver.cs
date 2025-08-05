using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DynamoSharp.Converters.Jsons;

public class CustomIgnoreResolver : DefaultContractResolver
{
    private readonly HashSet<string> _propertiesToIgnore;

    public CustomIgnoreResolver(IEnumerable<string> propertiesToIgnore)
    {
        _propertiesToIgnore = new HashSet<string>(propertiesToIgnore);
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = base.CreateProperties(type, memberSerialization);

        // Include all List<T> properties except those in _propertiesToIgnore.
        return props
            .Where(p => p.PropertyName is not null && !_propertiesToIgnore.Contains(p.PropertyName))
            .ToList();
    }

}
