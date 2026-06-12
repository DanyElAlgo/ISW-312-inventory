using Purchases.API.DTOs;
using Purchases.API.Models;
using Purchases.API.Repositories.Interfaces;

namespace Purchases.API.Services;

public class SuppliersService
{
    private readonly ISupplierRepository _suppliers;

    public SuppliersService(ISupplierRepository suppliers)
    {
        _suppliers = suppliers;
    }

    public async Task<IReadOnlyList<SupplierDto>> ListAsync(string companyCen)
    {
        var rows = await _suppliers.GetActiveByCompanyCenAsync(companyCen);
        return rows.Select(MapSummary).ToList();
    }

    public static SupplierDto MapSummary(Supplier s) => new()
    {
        SupplierCen = s.Cen ?? $"SUP-{s.Id:D6}",
        Name = s.Name,
    };
}
