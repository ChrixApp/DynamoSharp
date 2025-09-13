using DynamoSharp.Converters.Objects;

namespace DynamoSharp.Tests.Converters.Objects;

public class PropertyInspectorTests
{
    private class TestClass
    {
        // Auto-property (compiler-generated backing field expected)
        public int AutoProp { get; set; }

        // Manual property (no compiler-generated backing field named '<ManualProp>k__BackingField')
        private int _manual;
        public int ManualProp
        {
            get => _manual;
            set => _manual = value;
        }

        // Auto-calculated property (no backing field)
        public int ComputedProp => AutoProp + 1; // Read-only auto-calculated property

        // Various collections / non-collections for IsCollectionProperty tests
        public List<int> ListProp { get; set; } = new();
        public IReadOnlyList<string> ReadOnlyListProp { get; set; } = new List<string>();
        public IReadOnlyCollection<int> ReadOnlyCollectionProp { get; set; } = new List<int>();
        public Dictionary<int, string> DictionaryProp { get; set; } = new();
        public IReadOnlyDictionary<int, string> ReadOnlyDictionaryProp { get; set; } = new Dictionary<int, string>();

        // Unsupported/other types
        public int[] ArrayProp { get; set; } = new int[0];
        public HashSet<int> HashSetProp { get; set; } = new();
        public IEnumerable<int> EnumerableProp { get; set; } = new List<int>();
        public int IntProp { get; set; }
    }

    [Fact]
    public void IsComputedProperty_ReturnsTrue_ForComputedProperty()
    {
        var type = typeof(TestClass);
        var prop = type.GetProperty(nameof(TestClass.AutoProp))!;
        Assert.False(PropertyInspector.IsComputedProperty(type, prop));
    }

    [Fact]
    public void IsComputedProperty_ReturnsFalse_ForManualProperty()
    {
        var type = typeof(TestClass);
        var prop = type.GetProperty(nameof(TestClass.ManualProp))!;
        Assert.True(PropertyInspector.IsComputedProperty(type, prop));
    }

    [Fact]
    public void IsComputedProperty_ReturnsFalse_ForComputedProperty()
    {
        var type = typeof(TestClass);
        var prop = type.GetProperty(nameof(TestClass.ComputedProp))!;
        Assert.True(PropertyInspector.IsComputedProperty(type, prop));
    }

    [Fact]
    public void IsCollectionProperty_ReturnsTrue_ForSupportedGenericCollections()
    {
        var type = typeof(TestClass);

        Assert.True(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.ListProp))!));
        Assert.True(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.ReadOnlyListProp))!));
        Assert.True(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.ReadOnlyCollectionProp))!));
        Assert.True(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.DictionaryProp))!));
        Assert.True(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.ReadOnlyDictionaryProp))!));
    }

    [Fact]
    public void IsCollectionProperty_ReturnsFalse_ForUnsupportedOrNonGenericCollections()
    {
        var type = typeof(TestClass);

        Assert.False(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.ArrayProp))!));
        Assert.False(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.HashSetProp))!));
        Assert.False(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.EnumerableProp))!));
        Assert.False(PropertyInspector.IsCollectionProperty(type, type.GetProperty(nameof(TestClass.IntProp))!));
    }
}
