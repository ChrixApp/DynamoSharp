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

    private class TestChildClass : TestClass
    {
        // Auto-property (compiler-generated backing field expected)
        public int AutoPropChild { get; set; }
    }

    [Fact]
    public void IsComputedProperty_ReturnsFalse_ForNotComputedProperty()
    {
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.AutoProp))!;
        Assert.False(PropertyInspector.IsComputedProperty(prop));
    }

    [Fact]
    public void IsComputedProperty_ReturnsTrue_ForManualProperty()
    {
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.ManualProp))!;
        Assert.True(PropertyInspector.IsComputedProperty(prop));
    }

    [Fact]
    public void IsComputedProperty_ReturnsTrue_ForComputedProperty()
    {
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.ComputedProp))!;
        Assert.True(PropertyInspector.IsComputedProperty(prop));
    }

    [Theory]
    [InlineData(nameof(TestClass.ListProp))]
    [InlineData(nameof(TestClass.ReadOnlyListProp))]
    [InlineData(nameof(TestClass.ReadOnlyCollectionProp))]
    [InlineData(nameof(TestClass.DictionaryProp))]
    [InlineData(nameof(TestClass.ReadOnlyDictionaryProp))]
    public void IsCollectionProperty_ReturnsTrue_ForSupportedGenericCollections(string propName)
    {
        var prop = typeof(TestClass).GetProperty(propName)!;
        Assert.True(PropertyInspector.IsCollectionProperty(prop));
    }

    [Theory]
    [InlineData(nameof(TestClass.ArrayProp))]
    [InlineData(nameof(TestClass.HashSetProp))]
    [InlineData(nameof(TestClass.EnumerableProp))]
    [InlineData(nameof(TestClass.IntProp))]
    public void IsCollectionProperty_ReturnsFalse_ForUnsupportedOrNonGenericCollections(string propName)
    {
        var prop = typeof(TestClass).GetProperty(propName)!;
        Assert.False(PropertyInspector.IsCollectionProperty(prop));
    }

    [Fact]
    public void IsComputedProperty_ReturnsFalse_ForNotComputedPropertyInChildClass()
    {
        var prop = typeof(TestChildClass).GetProperty(nameof(TestChildClass.AutoPropChild))!;
        Assert.False(PropertyInspector.IsComputedProperty(prop));
    }

    [Fact]
    public void IsComputedProperty_ReturnsFalse_ForNotComputedPropertyInBaseClass()
    {
        var prop = typeof(TestChildClass).GetProperty(nameof(TestClass.AutoProp))!;
        Assert.False(PropertyInspector.IsComputedProperty(prop));
    }

    [Fact]
    public void IsComputedProperty_ReturnsTrue_ForManualProperty_UsingChildClass()
    {
        var prop = typeof(TestChildClass).GetProperty(nameof(TestClass.ManualProp))!;
        Assert.True(PropertyInspector.IsComputedProperty(prop));
    }

    [Fact]
    public void IsComputedProperty_ReturnsTrue_ForComputedProperty_UsingChildClass()
    {
        var prop = typeof(TestChildClass).GetProperty(nameof(TestClass.ComputedProp))!;
        Assert.True(PropertyInspector.IsComputedProperty(prop));
    }

    [Fact]
    public void IsCollectionProperty_ReturnsTrue_ForInheritedCollectionProperty()
    {
        var prop = typeof(TestChildClass).GetProperty(nameof(TestClass.ListProp))!;
        Assert.True(PropertyInspector.IsCollectionProperty(prop));
    }

    [Fact]
    public void IsCollectionProperty_ReturnsFalse_ForUnsupportedOrNonGenericCollections_UsingChildClass()
    {
        var prop = typeof(TestChildClass).GetProperty(nameof(TestClass.ArrayProp))!;
        Assert.False(PropertyInspector.IsCollectionProperty(prop));
    }
}