using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Operations.TransactWriteItems;

namespace DynamoSharp.Converters.Entities;

public interface IConverter
{
    TransactWriteItem Convert(Document document);
}
