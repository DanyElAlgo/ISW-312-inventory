using System.Collections.Concurrent;
using System.Threading.Channels;
using Inventory.API.DTOs.Contract;

namespace Inventory.API.Services;

public sealed class RestockNotifier
{
    private readonly ConcurrentDictionary<Guid, Channel<RestockEvent>> _subscribers = new();

    public (Guid Id, ChannelReader<RestockEvent> Reader) Subscribe()
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<RestockEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
        _subscribers[id] = channel;
        return (id, channel.Reader);
    }

    public void Unsubscribe(Guid id)
    {
        if (_subscribers.TryRemove(id, out var channel))
            channel.Writer.TryComplete();
    }

    public void Publish(RestockEvent evt)
    {
        foreach (var channel in _subscribers.Values)
            channel.Writer.TryWrite(evt);
    }
}
