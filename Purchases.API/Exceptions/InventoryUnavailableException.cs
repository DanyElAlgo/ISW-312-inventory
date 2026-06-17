namespace Purchases.API.Exceptions;

/// <summary>
/// Thrown when the Inventory module cannot be reached (transport failure, timeout, open circuit)
/// or returns a server-side error. Mapped to HTTP 503 (Servicio no disponible) by the global
/// exception handler so the shared frontend shows a clear toast instead of a raw 500.
/// </summary>
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
