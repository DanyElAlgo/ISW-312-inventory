namespace Purchases.API.Events;

public interface IEventPublisher
{
    Task PublishPurchaseOrderConfirmedAsync(PurchaseOrderConfirmedEvent evt, CancellationToken ct = default);
}
