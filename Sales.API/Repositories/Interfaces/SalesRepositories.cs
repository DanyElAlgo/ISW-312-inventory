using Sales.API.Models;

namespace Sales.API.Repositories.Interfaces;

public interface IOrderTicketRepository
{
    Task<OrderTicket?> GetByIdAsync(int id, bool includeItems = false, bool includeStatus = false);
    Task<OrderTicket?> GetByCenAsync(string cen, bool includeItems = false, bool includeStatus = false);
    Task<IReadOnlyList<OrderTicket>> GetByStatusAsync(int statusId, bool includeItems = false, bool includeStatus = false);
    Task<int> CountByCreatedAtRangeAsync(DateTime fromInclusive, DateTime toExclusive);
    OrderTicket Add(OrderTicket ticket);
}

public interface IOrderItemRepository
{
    Task<OrderItem?> GetByIdAsync(int id, bool includeStatus = false);
    Task<OrderItem?> GetByCenAsync(string cen, bool includeStatus = false);
    Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(int orderId, bool includeStatus = false);
    Task<IReadOnlyList<OrderItem>> GetUnsentByOrderIdAsync(int orderId, bool includeStatus = false);
    OrderItem Add(OrderItem item);
}

public interface IPaymentRepository
{
    Task<IReadOnlyList<int>> GetPaidOrderIdsAsync(DateTime? fromInclusive = null, DateTime? toExclusive = null);
    Payment Add(Payment payment);
}

public interface IPaymentTypeRepository
{
    Task<IReadOnlyList<PaymentType>> GetAllAsync();
    Task<PaymentType?> FindByCodeOrNameAsync(string codeOrName);
    Task<PaymentType?> GetByIdAsync(int id);
}

public interface IOrderStatusRepository
{
    Task<OrderStatus?> FindByAnyNameAsync(IReadOnlyList<string> lowercaseNames);
    OrderStatus Add(OrderStatus status);
}

public interface IStationRepository
{
    Task<IReadOnlyList<Station>> GetByTypeIdAsync(int typeId);
    Task<Station?> FindByLowercaseNameAsync(string lowercaseName);
}

public interface IStationTypeRepository
{
    Task<IReadOnlyList<StationType>> GetAllWithStationsAsync();
    Task<StationType?> GetByIdAsync(int id);
    Task<int?> FindIdByLowercaseNameAsync(string lowercaseName);
    Task<IReadOnlyList<int>> GetCategoryIdsAsync(int stationTypeId);
}

public interface IWaiterRepository
{
    Task<IReadOnlyList<Waiter>> GetAllAsync();
    Task<Waiter?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IGlobalTaxConfigRepository
{
    Task<GlobalTaxConfig> GetOrCreateAsync();
}

public interface IOrderCommandRepository
{
    Task<OrderCommand?> GetLatestByOrderIdAsync(int orderId);
    OrderCommand Add(OrderCommand command);
}

public interface ICommandItemRepository
{
    Task<IReadOnlyList<CommandItem>> GetByCommandIdAsync(int commandId, bool includeOrderItem = false, bool includeStation = false);
    Task<IReadOnlyList<CommandItem>> GetByStationTypeIdAsync(int stationTypeId);
    Task<IReadOnlyList<CommandItem>> GetAllWithOrderItemStatusAsync();
    Task RemoveByOrderItemIdAsync(int orderItemId);
    CommandItem Add(CommandItem commandItem);
}

public interface ISalesUnitOfWork
{
    Task<int> SaveChangesAsync();
}
