using Purchases.API.Repositories.Interfaces;
using Purchases.Domain.Enums;

namespace Purchases.API.Services;

public class PurchaseStatusesService
{
    private readonly IPurchaseStatusRepository _statuses;
    private readonly Dictionary<PurchaseStatusEnum, int> _idCache = new();

    public PurchaseStatusesService(IPurchaseStatusRepository statuses)
    {
        _statuses = statuses;
    }

    public Task<int> GetPendingIdAsync()   => GetIdAsync(PurchaseStatusEnum.Pending);
    public Task<int> GetConfirmedIdAsync() => GetIdAsync(PurchaseStatusEnum.Confirmed);
    public Task<int> GetCancelledIdAsync() => GetIdAsync(PurchaseStatusEnum.Cancelled);

    public async Task<int?> FromExternalAsync(int external)
        => Enum.IsDefined(typeof(PurchaseStatusEnum), external)
            ? await GetIdAsync((PurchaseStatusEnum)external)
            : null;

    public async Task<int> ToExternalAsync(int statusId)
    {
        foreach (var status in Enum.GetValues<PurchaseStatusEnum>())
            if (statusId == await GetIdAsync(status))
                return (int)status;
        return -1;
    }

    private async Task<int> GetIdAsync(PurchaseStatusEnum status)
    {
        if (_idCache.TryGetValue(status, out var cached))
            return cached;

        var name = status.ToString().ToLowerInvariant();
        var id = await _statuses.FindIdByLowercaseNameAsync(name)
            ?? throw new InvalidOperationException(
                $"purchases.purchase_status row '{name}' (code {(int)status}) is missing. ");

        return _idCache[status] = id;
    }
}
