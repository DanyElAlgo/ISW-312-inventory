using Polly.CircuitBreaker;
using Polly.Timeout;
using Purchases.API.Exceptions;

namespace Purchases.API.Http;

/// <summary>
/// Outermost handler on the Inventory typed client. Translates transport-level failures
/// (connection refused, timeout, open circuit) into a friendly <see cref="InventoryUnavailableException"/>
/// so the rest of the app — and the shared frontend — see a single, clear 503 contract
/// instead of leaking raw <see cref="HttpRequestException"/>/Polly exceptions.
/// </summary>
public sealed class InventoryFailureTranslatingHandler : DelegatingHandler
{
    private const string UnavailableMessage =
        "El módulo de Inventario no está disponible en este momento. Intenta nuevamente en unos segundos.";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or BrokenCircuitException
                                      or TimeoutRejectedException or TaskCanceledException)
        {
            throw new InventoryUnavailableException(UnavailableMessage, ex);
        }
    }
}
