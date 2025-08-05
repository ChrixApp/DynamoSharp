namespace OptimisticLockingSave.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Order does not contain an item with product id {productId}")
    {
    }
}

