namespace Purchases.API.Exceptions;

/// <summary>
/// Thrown when a requested resource (company, order, supplier...) does not exist.
/// Mapped to HTTP 404 (Recurso no encontrado) by the global exception handler.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
