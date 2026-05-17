using Sales.API.DTOs;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class WaitersService
{
    private readonly IWaiterRepository _waiters;

    public WaitersService(IWaiterRepository waiters)
    {
        _waiters = waiters;
    }

    public async Task<IReadOnlyList<WaiterContractResponse>> GetWaitersAsync()
    {
        var list = await _waiters.GetAllAsync();
        return list.Select(w => new WaiterContractResponse
        {
            WaiterCen = w.Id.ToString(),
            Name = w.Name ?? string.Empty
        }).ToList();
    }
}
