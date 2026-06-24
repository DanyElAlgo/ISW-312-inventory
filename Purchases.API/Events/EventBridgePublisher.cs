using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;

namespace Purchases.API.Events;

public sealed class EventBridgePublisher : IEventPublisher, IDisposable
{
    private const string EventSource = "erp.purchases";
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IAmazonEventBridge _client;
    private readonly string _busName;
    private readonly ILogger<EventBridgePublisher> _logger;

    public EventBridgePublisher(string busName, ILogger<EventBridgePublisher> logger)
    {
        _busName = busName;
        _logger = logger;
        _client = new AmazonEventBridgeClient();
    }

    public async Task PublishPurchaseOrderConfirmedAsync(PurchaseOrderConfirmedEvent evt, CancellationToken ct = default)
    {
        try
        {
            var request = new PutEventsRequest
            {
                Entries =
                [
                    new PutEventsRequestEntry
                    {
                        EventBusName = _busName,
                        Source = EventSource,
                        DetailType = "PurchaseOrderConfirmed",
                        Detail = JsonSerializer.Serialize(evt, JsonOpts),
                    },
                ],
            };

            var response = await _client.PutEventsAsync(request, ct);
            if (response.FailedEntryCount > 0)
            {
                _logger.LogWarning(
                    "EventBridge rejected PurchaseOrderConfirmed for {OrderCen}: {Reason}",
                    evt.OrderCen, response.Entries.FirstOrDefault()?.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish PurchaseOrderConfirmed event for {OrderCen}", evt.OrderCen);
        }
    }

    public void Dispose() => _client.Dispose();
}
