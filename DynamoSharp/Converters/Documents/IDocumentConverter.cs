using DocumentEfficient = EfficientDynamoDb.DocumentModel.Document;

namespace DynamoSharp.Converters.Documents;

public interface IDocumentConverter
{
    List<DocumentEfficient> CloneList(IReadOnlyList<DocumentEfficient> documentsEfficient);
    object ConvertToObject(DocumentEfficient doc, Type propertyType);
}