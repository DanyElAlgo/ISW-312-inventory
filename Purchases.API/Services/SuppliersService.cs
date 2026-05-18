using Purchases.API.DTOs;
using Purchases.API.Models;
using Purchases.API.Repositories.Interfaces;

namespace Purchases.API.Services;

public class SuppliersService
{
    private readonly IBusinessRepository _businesses;
    private readonly ISupplierRepository _suppliers;
    private readonly IPurchasesUnitOfWork _uow;

    public SuppliersService(
        IBusinessRepository businesses,
        ISupplierRepository suppliers,
        IPurchasesUnitOfWork uow)
    {
        _businesses = businesses;
        _suppliers = suppliers;
        _uow = uow;
    }

    public async Task<IReadOnlyList<SupplierDto>?> ListAsync(string companyCen)
    {
        var business = await _businesses.GetByCenAsync(companyCen);
        if (business == null) return null;

        var rows = await _suppliers.GetActiveByBusinessIdAsync(business.Id);
        return rows.Select(MapSummary).ToList();
    }

    public async Task<SupplierDetailDto?> CreateAsync(string companyCen, CreateSupplierDto request)
    {
        var business = await _businesses.GetByCenAsync(companyCen);
        if (business == null) return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("name is required.");

        var supplier = _suppliers.Add(new Supplier
        {
            BusinessId = business.Id,
            Name = request.Name.Trim(),
            ContactEmail = request.ContactEmail?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            IsActive = true,
        });
        await _uow.SaveChangesAsync();

        supplier.Cen = $"SUP-{supplier.Id:D6}";
        await _uow.SaveChangesAsync();

        return MapDetail(supplier);
    }

    public async Task<SupplierDetailDto?> UpdateAsync(string companyCen, string supplierCen, UpdateSupplierDto request)
    {
        var business = await _businesses.GetByCenAsync(companyCen);
        if (business == null) return null;

        var supplier = await _suppliers.GetByCenAsync(business.Id, supplierCen);
        if (supplier == null) return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("name is required.");

        supplier.Name = request.Name.Trim();
        supplier.ContactEmail = request.ContactEmail?.Trim();
        supplier.ContactPhone = request.ContactPhone?.Trim();
        supplier.IsActive = request.IsActive;

        await _uow.SaveChangesAsync();
        return MapDetail(supplier);
    }

    // Soft delete — flips is_active to false. Real removal would cascade-fail any historical PO.
    public async Task<bool?> DeleteAsync(string companyCen, string supplierCen)
    {
        var business = await _businesses.GetByCenAsync(companyCen);
        if (business == null) return null;

        var supplier = await _suppliers.GetByCenAsync(business.Id, supplierCen);
        if (supplier == null) return null;

        supplier.IsActive = false;
        await _uow.SaveChangesAsync();
        return true;
    }

    public static SupplierDto MapSummary(Supplier s) => new()
    {
        SupplierCen = s.Cen ?? $"SUP-{s.Id:D6}",
        Name = s.Name,
    };

    public static SupplierDetailDto MapDetail(Supplier s) => new()
    {
        SupplierCen = s.Cen ?? $"SUP-{s.Id:D6}",
        Name = s.Name,
        ContactEmail = s.ContactEmail,
        ContactPhone = s.ContactPhone,
        IsActive = s.IsActive,
    };
}
