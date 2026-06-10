using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Purchases.API.Models;
using Purchases.API.Repositories.Interfaces;

namespace Purchases.API.Repositories.Ef;

public sealed class BusinessRepository : IBusinessRepository
{
    private readonly PurchasesDbContext _context;

    public BusinessRepository(PurchasesDbContext context)
    {
        _context = context;
    }

    public async Task<Business?> GetByCenAsync(string companyCen)
    {
        return await _context.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Cen == companyCen);
    }
}

public sealed class SupplierRepository : ISupplierRepository
{
    private readonly PurchasesDbContext _context;

    public SupplierRepository(PurchasesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Supplier>> GetActiveByBusinessIdAsync(int businessId)
    {
        return await _context.Suppliers
            .Where(s => s.BusinessId == businessId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Supplier?> GetByCenAsync(int businessId, string supplierCen)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(s => s.BusinessId == businessId && s.Cen == supplierCen);
    }
}

public sealed class PurchaseStatusRepository : IPurchaseStatusRepository
{
    private readonly PurchasesDbContext _context;

    public PurchaseStatusRepository(PurchasesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PurchaseStatus>> GetAllAsync()
    {
        return await _context.PurchaseStatuses.OrderBy(s => s.Id).ToListAsync();
    }

    public async Task<int?> FindIdByLowercaseNameAsync(string lowercaseName)
    {
        var id = await _context.PurchaseStatuses
            .Where(s => s.Name != null && s.Name.ToLower() == lowercaseName)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();
        return id;
    }
}

public sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly PurchasesDbContext _context;

    public PurchaseOrderRepository(PurchasesDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder?> GetByCenAsync(int businessId, string orderCen, bool includeItems = false)
    {
        IQueryable<PurchaseOrder> query = _context.PurchaseOrders.Include(o => o.Supplier);
        if (includeItems)
            query = query.Include(o => o.Items);
        return await query.FirstOrDefaultAsync(o => o.BusinessId == businessId && o.Cen == orderCen);
    }

    public async Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> SearchAsync(
        int businessId,
        int? statusId,
        int page,
        int pageSize,
        bool sortDescending)
    {
        IQueryable<PurchaseOrder> query = _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .Where(o => o.BusinessId == businessId);

        if (statusId.HasValue)
            query = query.Where(o => o.StatusId == statusId.Value);

        query = sortDescending
            ? query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
            : query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public PurchaseOrder Add(PurchaseOrder order) => _context.PurchaseOrders.Add(order).Entity;
}

public sealed class PurchaseOrderItemRepository : IPurchaseOrderItemRepository
{
    private readonly PurchasesDbContext _context;

    public PurchaseOrderItemRepository(PurchasesDbContext context)
    {
        _context = context;
    }

    public PurchaseOrderItem Add(PurchaseOrderItem item) => _context.PurchaseOrderItems.Add(item).Entity;
}

public sealed class PurchasesUnitOfWork : IPurchasesUnitOfWork
{
    private readonly PurchasesDbContext _context;

    public PurchasesUnitOfWork(PurchasesDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();
}
