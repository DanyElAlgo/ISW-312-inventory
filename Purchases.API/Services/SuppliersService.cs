using Purchases.API.DTOs;
using Purchases.API.Models;
using Purchases.API.Repositories.Interfaces;

namespace Purchases.API.Services;

public class SuppliersService
{
    private readonly IBusinessRepository _businesses;
    private readonly ISupplierRepository _suppliers;

    public SuppliersService(
        IBusinessRepository businesses,
        ISupplierRepository suppliers)
    {
        _businesses = businesses;
        _suppliers = suppliers;
    }

    public async Task<IReadOnlyList<SupplierDto>?> ListAsync(string companyCen)
    {
        var business = await _businesses.GetByCenAsync(companyCen);
        if (business == null) return null;

        var rows = await _suppliers.GetActiveByBusinessIdAsync(business.Id);
        return rows.Select(MapSummary).ToList();
    }

    public static SupplierDto MapSummary(Supplier s) => new()
    {
        SupplierCen = s.Cen ?? $"SUP-{s.Id:D6}",
        Name = s.Name,
    };
}
