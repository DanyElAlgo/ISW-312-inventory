using Purchases.API.Repositories.Interfaces;

namespace Purchases.API.Services;

// Maps the contract's integer PurchaseStatus enum (0=Pending, 1=Confirmed, 2=Cancelled)
// to the DB lookup table's primary keys. The mapping is resolved once and cached for
// the request lifetime, so we do not hammer the status table on every call.
public class PurchaseStatusesService
{
    public const int Pending = 1;
    public const int Confirmed = 2;
    public const int Cancelled = 3;

    private readonly IPurchaseStatusRepository _statuses;

    private int? _pendingId;
    private int? _confirmedId;
    private int? _cancelledId;

    public PurchaseStatusesService(IPurchaseStatusRepository statuses)
    {
        _statuses = statuses;
    }

    public async Task<int> GetPendingIdAsync()
        => _pendingId ??= await ResolveAsync("pending", Pending);

    public async Task<int> GetConfirmedIdAsync()
        => _confirmedId ??= await ResolveAsync("confirmed", Confirmed);

    public async Task<int> GetCancelledIdAsync()
        => _cancelledId ??= await ResolveAsync("cancelled", Cancelled);

    // The contract's integer PurchaseStatus is 0/1/2 (Pending/Confirmed/Cancelled).
    // The DB IDs may differ if the seed runs against a non-empty table, so we never
    // assume id == externalCode.
    public async Task<int> ToExternalAsync(int statusId)
    {
        if (statusId == await GetPendingIdAsync()) return Pending;
        if (statusId == await GetConfirmedIdAsync()) return Confirmed;
        if (statusId == await GetCancelledIdAsync()) return Cancelled;
        return -1;
    }

    public async Task<int?> FromExternalAsync(int external) => external switch
    {
        Pending => await GetPendingIdAsync(),
        Confirmed => await GetConfirmedIdAsync(),
        Cancelled => await GetCancelledIdAsync(),
        _ => null,
    };

    private async Task<int> ResolveAsync(string lowercaseName, int externalCode)
    {
        var id = await _statuses.FindIdByLowercaseNameAsync(lowercaseName);
        if (id == null)
            throw new InvalidOperationException(
                $"purchases.purchase_status row '{lowercaseName}' (code {externalCode}) is missing. " +
                "Did you run backend/database/seed_data.sql?");
        return id.Value;
    }
}
