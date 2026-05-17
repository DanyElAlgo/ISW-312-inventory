using Sales.API.Models;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class OrderStatusesService
{
    private readonly IOrderStatusRepository _statuses;
    private readonly ISalesUnitOfWork _uow;

    public OrderStatusesService(IOrderStatusRepository statuses, ISalesUnitOfWork uow)
    {
        _statuses = statuses;
        _uow = uow;
    }

    public Task<int> GetOpenStatusIdAsync() =>
        GetOrCreateAsync(new[] { "open", "abierto", "pending" }, "Open", "Open account");

    public Task<int> GetPendingStatusIdAsync() =>
        GetOrCreateAsync(new[] { "pending", "pendiente" }, "Pending", "Pending item");

    public Task<int> GetInPreparationStatusIdAsync() =>
        GetOrCreateAsync(new[] { "en preparacion", "en preparación", "in preparation" }, "En Preparación", "Item being prepared");

    public Task<int> GetReadyStatusIdAsync() =>
        GetOrCreateAsync(new[] { "listo", "ready" }, "Listo", "Item ready");

    public Task<int> GetPaidStatusIdAsync() =>
        GetOrCreateAsync(new[] { "paid", "pagado" }, "Pagado", "Paid ticket");

    public Task<int> GetCancelledStatusIdAsync() =>
        GetOrCreateAsync(new[] { "cancelled", "cancelado" }, "Cancelado", "Cancelled account");

    private async Task<int> GetOrCreateAsync(string[] aliases, string defaultName, string defaultDescription)
    {
        var existing = await _statuses.FindByAnyNameAsync(aliases);
        if (existing != null)
            return existing.Id;

        var created = _statuses.Add(new OrderStatus { Name = defaultName, Description = defaultDescription });
        await _uow.SaveChangesAsync();
        return created.Id;
    }
}
