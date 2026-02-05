using Ardalis.SmartEnum;
using DynamoSharp.Converters.Jsons;
using DynamoSharp.Converters.Objects;
using EfficientDynamoDb.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace DynamoSharp.Tests.Converters.Objects;

public class ObjectConverterTests
{
    [Fact]
    public void CloneList_ShouldCloneDocuments()
    {
        // Arrange
        var originalDocuments = new List<Document>
        {
            new Document { ["Id"] = new StringAttributeValue("1") },
            new Document { ["Id"] = new StringAttributeValue("2") }
        };

        // Act
        var clonedDocuments = ObjectConverter.CloneList(originalDocuments);

        // Assert
        Assert.Equal(originalDocuments.Count, clonedDocuments.Count);
        for (int i = 0; i < originalDocuments.Count; i++)
        {
            Assert.Equal(originalDocuments[i]["Id"].AsString(), clonedDocuments[i]["Id"].AsString());
        }
    }

    [Fact]
    public void DeepCopy_ShouldCreateDeepCopyOfObject()
    {
        // Arrange
        var originalObject = new TestEntity
        {
            Id = 1,
            Name = "Test",
            NestedEntity = new NestedEntity { Value = "Nested" }
        };
        var jsonSerializer = JsonSerializerBuilder.Build();

        // Act
        var copiedObject = (TestEntity?)ObjectConverter.Instance.DeepCopy(originalObject, jsonSerializer);

        // Assert
        Assert.NotSame(originalObject, copiedObject);
        Assert.Equal(originalObject.Id, copiedObject?.Id);
        Assert.Equal(originalObject.Name, copiedObject?.Name);
        Assert.NotSame(originalObject.NestedEntity, copiedObject?.NestedEntity);
        Assert.Equal(originalObject.NestedEntity.Value, copiedObject?.NestedEntity?.Value);
        Assert.Equal(EntityType.Parent, copiedObject?.EntityType);
    }

    [Fact]
    public void ConvertDocumentToObject_ShouldConvertDocumentToObject()
    {
        // Arrange
        var document = new Document
        {
            ["Id"] = new NumberAttributeValue("1"),
            ["Name"] = new StringAttributeValue("Test"),
            ["NestedEntity"] = new MapAttributeValue(new Document { ["Value"] = new StringAttributeValue("Nested") })
        };

        // Act
        var result = (TestEntity)ObjectConverter.Instance.ConvertDocumentToObject(document, typeof(TestEntity));

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.NotNull(result.NestedEntity);
        Assert.Equal("Nested", result.NestedEntity.Value);
    }

    [Fact]
    public void ConvertDocumentToObject_ShouldConvertComplexDocumentToObject()
    {
        // Arrange
        var document = new Document
        {
            ["Id"] = new NumberAttributeValue("1"),
            ["Name"] = new StringAttributeValue("Test"),
            ["Integer"] = new NumberAttributeValue("26"),
            ["UInteger"] = new NumberAttributeValue("26"),
            ["Short"] = new NumberAttributeValue("26"),
            ["UShort"] = new NumberAttributeValue("26"),
            ["Long"] = new NumberAttributeValue("26"),
            ["ULong"] = new NumberAttributeValue("26"),
            ["Byte"] = new NumberAttributeValue("26"),
            ["Char"] = new StringAttributeValue("C"),
            ["Guid"] = new StringAttributeValue("61ACC105-CC28-4FFA-B0EA-E6E4F1502A8D"),
            ["Bool"] = new BoolAttributeValue(true),
            ["Double"] = new NumberAttributeValue("26.05"),
            ["Float"] = new NumberAttributeValue("26.05"),
            ["Decimal"] = new NumberAttributeValue("26.05"),
            ["Enum"] = new StringAttributeValue("Value2"),
            ["NestedEntity"] = new MapAttributeValue(new Document { ["Value"] = new StringAttributeValue("Nested") }),
            ["NestedEntityList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new MapAttributeValue(new Document { ["Value"] = new StringAttributeValue("Nested1") }),
                    new MapAttributeValue(new Document { ["Value"] = new StringAttributeValue("Nested2") })
                }),
            ["StringList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new StringAttributeValue("String1"),
                    new StringAttributeValue("String2")
                }),
            ["UIntList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new NumberAttributeValue("1"),
                    new NumberAttributeValue("2")
                }),
            ["IntList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new NumberAttributeValue("1"),
                    new NumberAttributeValue("2")
                }),
            ["DateTimeList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new StringAttributeValue("2022-12-31T18:00:00.0000000-06:00"),
                    new StringAttributeValue("2022-12-31T18:00:00.0000000-06:00")
                }),
            ["GuidList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new StringAttributeValue(Guid.NewGuid().ToString()),
                    new StringAttributeValue(Guid.NewGuid().ToString())
                }),
            ["BoolList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new BoolAttributeValue(true),
                    new BoolAttributeValue(false)
                }),
            ["DoubleList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new NumberAttributeValue("1.1"),
                    new NumberAttributeValue("2.2")
                }),
            ["FloatList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new NumberAttributeValue("1.1"),
                    new NumberAttributeValue("2.2")
                }),
            ["DecimalList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new NumberAttributeValue("1.1"),
                    new NumberAttributeValue("2.2")
                }),
            ["EnumList"] = new ListAttributeValue(new List<AttributeValue>
                {
                    new StringAttributeValue("Value1"),
                    new StringAttributeValue("Value2")
                }),
            ["NestedEntityDictionary"] = new MapAttributeValue(new Document
            {
                ["Key1"] = new MapAttributeValue(new Document { ["Value"] = new StringAttributeValue("Nested1") }),
                ["Key2"] = new MapAttributeValue(new Document { ["Value"] = new StringAttributeValue("Nested2") })
            }),
            ["StringDictionary"] = new MapAttributeValue(new Document
            {
                ["Key1"] = new StringAttributeValue("Value1"),
                ["Key2"] = new StringAttributeValue("Value2")
            }),
            ["IntDictionary"] = new MapAttributeValue(new Document
            {
                ["1"] = new StringAttributeValue("Value1"),
                ["2"] = new StringAttributeValue("Value2")
            }),
            ["DateTimeDictionary"] = new MapAttributeValue(new Document
            {
                ["2022-12-31T18:00:00.0000000-06:00"] = new StringAttributeValue("Value1"),
                ["2022-12-31T19:00:00.0000000-06:00"] = new StringAttributeValue("Value2")
            }),
            ["GuidDictionary"] = new MapAttributeValue(new Document
            {
                [Guid.NewGuid().ToString()] = new StringAttributeValue("Value1"),
                [Guid.NewGuid().ToString()] = new StringAttributeValue("Value2")
            }),
            ["BoolDictionary"] = new MapAttributeValue(new Document
            {
                ["true"] = new StringAttributeValue("Value1"),
                ["false"] = new StringAttributeValue("Value2")
            }),
            ["DoubleDictionary"] = new MapAttributeValue(new Document
            {
                ["1.1"] = new StringAttributeValue("Value1"),
                ["2.2"] = new StringAttributeValue("Value2")
            }),
            ["FloatDictionary"] = new MapAttributeValue(new Document
            {
                ["1.1"] = new StringAttributeValue("Value1"),
                ["2.2"] = new StringAttributeValue("Value2")
            }),
            ["DecimalDictionary"] = new MapAttributeValue(new Document
            {
                ["1.1"] = new StringAttributeValue("Value1"),
                ["2.2"] = new StringAttributeValue("Value2")
            }),
            ["EnumDictionary"] = new MapAttributeValue(new Document
            {
                ["Value1"] = new StringAttributeValue("Value1"),
                ["Value2"] = new StringAttributeValue("Value2")
            }),
            ["EntityType"] = new StringAttributeValue("Parent")
        };

        // Act
        var result = (TestEntity)ObjectConverter.Instance.ConvertDocumentToObject(document, typeof(TestEntity));

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal(26, result.Integer);
        Assert.Equal(26u, result.UInteger);
        Assert.Equal(short.Parse("26"), result.Short);
        Assert.Equal(ushort.Parse("26"), result.UShort);
        Assert.Equal(26, result.Long);
        Assert.Equal(26u, result.ULong);
        Assert.Equal(byte.Parse("26"), result.Byte);
        Assert.Equal('C', result.Char);
        Assert.Equal(Guid.Parse("61ACC105-CC28-4FFA-B0EA-E6E4F1502A8D"), result.Guid);
        Assert.True(result.Bool);
        Assert.Equal(26.05, result.Double);
        Assert.Equal(26.05f, result.Float);
        Assert.Equal(26.05m, result.Decimal);
        Assert.Equal(EnumTest.Value2, result.Enum);
        Assert.NotNull(result.NestedEntity);
        Assert.Equal("Nested", result.NestedEntity.Value);

        Assert.NotNull(result.NestedEntityList);
        Assert.Equal(2, result.NestedEntityList.Count);
        Assert.Equal("Nested1", result.NestedEntityList[0].Value);
        Assert.Equal("Nested2", result.NestedEntityList[1].Value);

        Assert.NotNull(result.StringList);
        Assert.Equal(2, result.StringList.Count);
        Assert.Equal("String1", result.StringList[0]);
        Assert.Equal("String2", result.StringList[1]);

        Assert.NotNull(result.IntList);
        Assert.Equal(2, result.IntList.Count);
        Assert.Equal(1, result.IntList[0]);
        Assert.Equal(2, result.IntList[1]);

        Assert.NotNull(result.DateTimeList);
        Assert.Equal(2, result.DateTimeList.Count);
        Assert.Equal(DateTime.Parse("2022-12-31T18:00:00.0000000-06:00", CultureInfo.InvariantCulture), result.DateTimeList[0]);
        Assert.Equal(DateTime.Parse("2022-12-31T18:00:00.0000000-06:00", CultureInfo.InvariantCulture), result.DateTimeList[1]);

        Assert.NotNull(result.GuidList);
        Assert.Equal(2, result.GuidList.Count);

        Assert.NotNull(result.BoolList);
        Assert.Equal(2, result.BoolList.Count);
        Assert.True(result.BoolList[0]);
        Assert.False(result.BoolList[1]);

        Assert.NotNull(result.DoubleList);
        Assert.Equal(2, result.DoubleList.Count);
        Assert.Equal(1.1, result.DoubleList[0]);
        Assert.Equal(2.2, result.DoubleList[1]);

        Assert.NotNull(result.FloatList);
        Assert.Equal(2, result.FloatList.Count);
        Assert.Equal(1.1f, result.FloatList[0]);
        Assert.Equal(2.2f, result.FloatList[1]);

        Assert.NotNull(result.DecimalList);
        Assert.Equal(2, result.DecimalList.Count);
        Assert.Equal(1.1m, result.DecimalList[0]);
        Assert.Equal(2.2m, result.DecimalList[1]);

        Assert.NotNull(result.EnumList);
        Assert.Equal(2, result.EnumList.Count);
        Assert.Equal(EnumTest.Value1, result.EnumList[0]);
        Assert.Equal(EnumTest.Value2, result.EnumList[1]);

        Assert.NotNull(result.NestedEntityDictionary);
        Assert.Equal(2, result.NestedEntityDictionary.Count);
        Assert.Equal("Nested1", result.NestedEntityDictionary["Key1"].Value);
        Assert.Equal("Nested2", result.NestedEntityDictionary["Key2"].Value);

        Assert.NotNull(result.StringDictionary);
        Assert.Equal(2, result.StringDictionary.Count);
        Assert.Equal("Value1", result.StringDictionary["Key1"]);
        Assert.Equal("Value2", result.StringDictionary["Key2"]);

        Assert.NotNull(result.IntDictionary);
        Assert.Equal(2, result.IntDictionary.Count);
        Assert.Equal("Value1", result.IntDictionary[1]);
        Assert.Equal("Value2", result.IntDictionary[2]);

        Assert.NotNull(result.DateTimeDictionary);
        Assert.Equal(2, result.DateTimeDictionary.Count);
        Assert.Equal("Value1", result.DateTimeDictionary[DateTime.Parse("2022-12-31T18:00:00.0000000-06:00", CultureInfo.InvariantCulture)]);
        Assert.Equal("Value2", result.DateTimeDictionary[DateTime.Parse("2022-12-31T19:00:00.0000000-06:00", CultureInfo.InvariantCulture)]);

        Assert.NotNull(result.GuidDictionary);
        Assert.Equal(2, result.GuidDictionary.Count);

        Assert.NotNull(result.BoolDictionary);
        Assert.Equal(2, result.BoolDictionary.Count);
        Assert.Equal("Value1", result.BoolDictionary[true]);
        Assert.Equal("Value2", result.BoolDictionary[false]);

        Assert.NotNull(result.DoubleDictionary);
        Assert.Equal(2, result.DoubleDictionary.Count);
        Assert.Equal("Value1", result.DoubleDictionary[1.1]);
        Assert.Equal("Value2", result.DoubleDictionary[2.2]);

        Assert.NotNull(result.FloatDictionary);
        Assert.Equal(2, result.FloatDictionary.Count);
        Assert.Equal("Value1", result.FloatDictionary[1.1f]);
        Assert.Equal("Value2", result.FloatDictionary[2.2f]);

        Assert.NotNull(result.DecimalDictionary);
        Assert.Equal(2, result.DecimalDictionary.Count);
        Assert.Equal("Value1", result.DecimalDictionary[1.1m]);
        Assert.Equal("Value2", result.DecimalDictionary[2.2m]);

        Assert.NotNull(result.EnumDictionary);
        Assert.Equal(2, result.EnumDictionary.Count);
        Assert.Equal("Value1", result.EnumDictionary[EnumTest.Value1]);
        Assert.Equal("Value2", result.EnumDictionary[EnumTest.Value2]);

        Assert.Equal(EntityType.Parent, result.EntityType);
    }

    private class TestEntity
    {
        public List<int> UIntList { get; set; } = new List<int>();

        public int Id { get; set; } = 26;
        public string Name { get; set; } = string.Empty;
        public int Integer { get; set; } = 26;
        public uint UInteger { get; set; } = 26;
        public short Short { get; set; } = 26;
        public ushort?UShort { get; set; } = 26;
        public long Long { get; set; } = 26;
        public ulong ULong { get; set; } = 26;
        public byte Byte { get; set; } = 26;
        public char Char { get; set; } = 'C';
        public Guid Guid { get; set; } = Guid.Parse("61ACC105-CC28-4FFA-B0EA-E6E4F1502A8D");
        public bool Bool { get; set; } = true;
        public double Double { get; set; } = 26;
        public float Float { get; set; } = 26;
        public decimal Decimal { get; set; } = 26;
        public EnumTest Enum { get; set; } = EnumTest.Value2;
        public DateTime DateTime { get; set; } = DateTime.Parse("2022-12-31T18:00:00.0000000-06:00", CultureInfo.InvariantCulture);
        public NestedEntity NestedEntity { get; set; } = new NestedEntity();

        public List<NestedEntity>? NestedEntityList { get; set; } = new List<NestedEntity>();
        public List<string>? StringList { get; set; } = new List<string>();
        public List<int>? IntList { get; set; } = new List<int>();
        public List<DateTime>? DateTimeList { get; set; } = new List<DateTime>();
        public List<Guid>? GuidList { get; set; } = new List<Guid>();
        public List<bool>? BoolList { get; set; } = new List<bool>();
        public List<double>? DoubleList { get; set; } = new List<double>();
        public List<float>? FloatList { get; set; } = new List<float>();
        public List<decimal>? DecimalList { get; set; } = new List<decimal>();
        public List<EnumTest>? EnumList { get; set; } = new List<EnumTest>();

        public Dictionary<string, NestedEntity>? NestedEntityDictionary { get; set; } = new Dictionary<string, NestedEntity>();
        public Dictionary<string, string>? StringDictionary { get; set; } = new Dictionary<string, string>();
        public Dictionary<int, string>? IntDictionary { get; set; } = new Dictionary<int, string>();
        public Dictionary<DateTime, string>? DateTimeDictionary { get; set; } = new Dictionary<DateTime, string>();
        public Dictionary<Guid, string>? GuidDictionary { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<bool, string>? BoolDictionary { get; set; } = new Dictionary<bool, string>();
        public Dictionary<double, string>? DoubleDictionary { get; set; } = new Dictionary<double, string>();
        public Dictionary<float, string>? FloatDictionary { get; set; } = new Dictionary<float, string>();
        public Dictionary<decimal, string>? DecimalDictionary { get; set; } = new Dictionary<decimal, string>();
        public Dictionary<EnumTest, string>? EnumDictionary { get; set; } = new Dictionary<EnumTest, string>();

        public EntityType EntityType { get; set; } = EntityType.Parent;
    }

    private class NestedEntity
    {
        public string? Value { get; set; }
    }

    public enum EnumTest
    {
        Value1,
        Value2
    }

    public abstract class EntityType : SmartEnum<EntityType>
    {
        public static readonly EntityType Parent = new ParentType();
        public static readonly EntityType Child = new ChildType();

        private EntityType(string name, int value) : base(name, value)
        {
        }

        private sealed class ParentType : EntityType
        {
            public ParentType() : base("Parent", 0)
            {
            }
        }

        private sealed class ChildType : EntityType
        {
            public ChildType() : base("Child", 1)
            {
            }
        }
    }
}