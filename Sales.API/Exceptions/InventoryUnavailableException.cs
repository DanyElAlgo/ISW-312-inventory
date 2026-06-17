namespace Sales.API.Exceptions;

public sealed class InventoryUnavailableException : Exception
{
    public InventoryUnavailableException(string message) : base(message)
    {
    }

    public InventoryUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
