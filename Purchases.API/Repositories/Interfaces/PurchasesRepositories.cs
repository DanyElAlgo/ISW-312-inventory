using Purchases.API.Models;

namespace Purchases.API.Repositories.Interfaces;

public interface IBusinessRepository
{
    Task<Business?> GetByCenAsync(string companyCen);
}

public interface ISupplierRepository
{
    Task<IReadOnlyList<Supplier>> GetActiveByBusinessIdAsync(int businessId);
    Task<Supplier?> GetByCenAsync(int businessId, string supplierCen);
    Task<Supplier?> GetByIdAsync(int id);
    Supplier Add(Supplier supplier);
    void Remove(Supplier supplier);
}

public interface IPurchaseStatusRepository
{
    Task<IReadOnlyList<PurchaseStatus>> GetAllAsync();
    Task<int?> FindIdByLowercaseNameAsync(string lowercaseName);
}

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByCenAsync(int businessId, string orderCen, bool includeItems = false);
    Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> SearchAsync(
        int businessId,
        int? statusId,
        int page,
        int pageSize,
        bool sortDescending);
    PurchaseOrder Add(PurchaseOrder order);
}

public interface IPurchaseOrderItemRepository
{
    PurchaseOrderItem Add(PurchaseOrderItem item);
}

public interface IPurchasesUnitOfWork
{
    Task<int> SaveChangesAsync();
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
}
