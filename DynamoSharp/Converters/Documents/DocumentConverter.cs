using DynamoSharp.Converters.Objects;
using EfficientDynamoDb.DocumentModel;

namespace DynamoSharp.Converters.Documents;

public class DocumentConverter : IDocumentConverter
{
    public List<Document> CloneList(IReadOnlyList<Document> documentsEfficient)
    {
        var documentsCopied = new List<Document>();
        for (var i = 0; i < documentsEfficient.Count; i++)
        {
            documentsCopied.Add(documentsEfficient[i]);
        }
        return documentsCopied;
    }

    public object ConvertToObject(Document doc, Type propertyType)
    {
        return ObjectConverter.Instance.ConvertDocumentToObject(doc, propertyType);
    }
}
