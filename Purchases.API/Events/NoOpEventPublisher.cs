namespace Purchases.API.Events;

public sealed class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;

    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger) => _logger = logger;

    public Task PublishPurchaseOrderConfirmedAsync(PurchaseOrderConfirmedEvent evt, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[NoOp] PurchaseOrderConfirmed {OrderCen} not published (EVENT_BUS_NAME not set).",
            evt.OrderCen);
        return Task.CompletedTask;
    }
}
