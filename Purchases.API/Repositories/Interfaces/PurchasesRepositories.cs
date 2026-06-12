using Purchases.API.Models;

namespace Purchases.API.Repositories.Interfaces;

public interface ISupplierRepository
{
    Task<IReadOnlyList<Supplier>> GetActiveByCompanyCenAsync(string companyCen);
    Task<Supplier?> GetByCenAsync(string companyCen, string supplierCen);
}

public interface IPurchaseStatusRepository
{
    Task<IReadOnlyList<PurchaseStatus>> GetAllAsync();
    Task<int?> FindIdByLowercaseNameAsync(string lowercaseName);
}

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByCenAsync(string companyCen, string orderCen, bool includeItems = false);
    Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> SearchAsync(
        string companyCen,
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
