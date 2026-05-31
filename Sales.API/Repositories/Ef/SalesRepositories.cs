using Microsoft.EntityFrameworkCore;
using Sales.API.Models;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Repositories.Ef;

public sealed class OrderTicketRepository : IOrderTicketRepository
{
    private readonly SalesDbContext _context;

    public OrderTicketRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<OrderTicket?> GetByIdAsync(int id, bool includeItems = false, bool includeStatus = false)
    {
        IQueryable<OrderTicket> query = _context.OrderTickets;
        if (includeStatus)
            query = query.Include(t => t.Status);
        if (includeItems)
            query = query.Include(t => t.OrderItems);
        return await query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<OrderTicket?> GetByCenAsync(string cen, bool includeItems = false, bool includeStatus = false)
    {
        IQueryable<OrderTicket> query = _context.OrderTickets;
        if (includeStatus)
            query = query.Include(t => t.Status);
        if (includeItems)
            query = query.Include(t => t.OrderItems);

        var ticket = await query.FirstOrDefaultAsync(t => t.Cen == cen);
        if (ticket == null && int.TryParse(cen, out var id))
            ticket = await query.FirstOrDefaultAsync(t => t.Id == id);
        return ticket;
    }

    public async Task<IReadOnlyList<OrderTicket>> GetByStatusAsync(int statusId, bool includeItems = false, bool includeStatus = false)
    {
        IQueryable<OrderTicket> query = _context.OrderTickets.Where(t => t.StatusId == statusId);
        if (includeStatus)
            query = query.Include(t => t.Status);
        if (includeItems)
            query = query.Include(t => t.OrderItems);
        return await query.ToListAsync();
    }

    public async Task<int> CountByCreatedAtRangeAsync(DateTime fromInclusive, DateTime toExclusive)
    {
        return await _context.OrderTickets
            .CountAsync(t => t.CreatedAt >= fromInclusive && t.CreatedAt < toExclusive);
    }

    public OrderTicket Add(OrderTicket ticket) => _context.OrderTickets.Add(ticket).Entity;
}

public sealed class OrderItemRepository : IOrderItemRepository
{
    private readonly SalesDbContext _context;

    public OrderItemRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<OrderItem?> GetByIdAsync(int id, bool includeStatus = false)
    {
        IQueryable<OrderItem> query = _context.OrderItems;
        if (includeStatus)
            query = query.Include(i => i.Status);
        return await query.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<OrderItem?> GetByCenAsync(string cen, bool includeStatus = false)
    {
        IQueryable<OrderItem> query = _context.OrderItems;
        if (includeStatus)
            query = query.Include(i => i.Status);

        var item = await query.FirstOrDefaultAsync(i => i.Cen == cen);
        if (item == null && int.TryParse(cen, out var id))
            item = await query.FirstOrDefaultAsync(i => i.Id == id);
        return item;
    }

    public async Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(int orderId, bool includeStatus = false)
    {
        IQueryable<OrderItem> query = _context.OrderItems.Where(i => i.OrderId == orderId);
        if (includeStatus)
            query = query.Include(i => i.Status);
        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<OrderItem>> GetUnsentByOrderIdAsync(int orderId, bool includeStatus = false)
    {
        IQueryable<OrderItem> query = _context.OrderItems
            .Where(i => i.OrderId == orderId && !i.CommandItems.Any());
        if (includeStatus)
            query = query.Include(i => i.Status);
        return await query.ToListAsync();
    }

    public OrderItem Add(OrderItem item) => _context.OrderItems.Add(item).Entity;
}

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly SalesDbContext _context;

    public PaymentRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<int>> GetPaidOrderIdsAsync(DateTime? fromInclusive = null, DateTime? toExclusive = null)
    {
        var query = _context.Payments.Where(p => p.OrderId.HasValue);
        if (fromInclusive.HasValue)
            query = query.Where(p => p.PaidAt >= fromInclusive.Value);
        if (toExclusive.HasValue)
            query = query.Where(p => p.PaidAt < toExclusive.Value);

        return await query.Select(p => p.OrderId!.Value).Distinct().ToListAsync();
    }

    public Payment Add(Payment payment) => _context.Payments.Add(payment).Entity;
}

public sealed class PaymentTypeRepository : IPaymentTypeRepository
{
    private readonly SalesDbContext _context;

    public PaymentTypeRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PaymentType>> GetAllAsync()
    {
        return await _context.PaymentTypes.OrderBy(pt => pt.Name).ToListAsync();
    }

    public async Task<PaymentType?> FindByCodeOrNameAsync(string codeOrName)
    {
        var lower = codeOrName.Trim().ToLower();
        return await _context.PaymentTypes
            .FirstOrDefaultAsync(pt =>
                (pt.Code != null && pt.Code.ToLower() == lower) ||
                (pt.Name != null && pt.Name.ToLower() == lower));
    }

    public async Task<PaymentType?> GetByIdAsync(int id)
    {
        return await _context.PaymentTypes.FindAsync(id);
    }
}

public sealed class OrderStatusRepository : IOrderStatusRepository
{
    private readonly SalesDbContext _context;

    public OrderStatusRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<OrderStatus?> FindByAnyNameAsync(IReadOnlyList<string> lowercaseNames)
    {
        return await _context.OrderStatuses
            .FirstOrDefaultAsync(s => s.Name != null && lowercaseNames.Contains(s.Name.ToLower()));
    }

    public OrderStatus Add(OrderStatus status) => _context.OrderStatuses.Add(status).Entity;
}

public sealed class StationRepository : IStationRepository
{
    private readonly SalesDbContext _context;

    public StationRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Station>> GetByTypeIdAsync(int typeId)
    {
        return await _context.Stations.Where(s => s.TypeId == typeId).ToListAsync();
    }

    public async Task<Station?> FindByLowercaseNameAsync(string lowercaseName)
    {
        return await _context.Stations
            .FirstOrDefaultAsync(s => s.Name != null && s.Name.ToLower() == lowercaseName);
    }
}

public sealed class StationTypeRepository : IStationTypeRepository
{
    private readonly SalesDbContext _context;

    public StationTypeRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StationType>> GetAllWithStationsAsync()
    {
        return await _context.StationTypes.Include(t => t.Stations).ToListAsync();
    }

    public async Task<StationType?> GetByIdAsync(int id)
    {
        return await _context.StationTypes.Include(t => t.Stations).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<int?> FindIdByLowercaseNameAsync(string lowercaseName)
    {
        var id = await _context.StationTypes
            .Where(s => s.Name != null && s.Name.ToLower() == lowercaseName)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();
        return id;
    }

    public async Task<IReadOnlyList<int>> GetCategoryIdsAsync(int stationTypeId)
    {
        try
        {
            var result = new List<int>();
            await using var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT category_id FROM sales.station_coverage WHERE station_type_id = @id";
            var p = cmd.CreateParameter();
            p.ParameterName = "@id";
            p.Value = stationTypeId;
            cmd.Parameters.Add(p);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(reader.GetInt32(0));
            return result;
        }
        catch
        {
            return Array.Empty<int>();
        }
    }
}

public sealed class WaiterRepository : IWaiterRepository
{
    private readonly SalesDbContext _context;

    public WaiterRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Waiter>> GetAllAsync()
    {
        return await _context.Waiters.OrderBy(w => w.Name).ToListAsync();
    }

    public async Task<Waiter?> GetByIdAsync(int id) => await _context.Waiters.FindAsync(id);

    public async Task<bool> ExistsAsync(int id) => await _context.Waiters.AnyAsync(w => w.Id == id);
}

public sealed class GlobalTaxConfigRepository : IGlobalTaxConfigRepository
{
    private readonly SalesDbContext _context;

    public GlobalTaxConfigRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<GlobalTaxConfig> GetOrCreateAsync()
    {
        var config = await _context.GlobalTaxConfigs.FirstOrDefaultAsync(c => c.Id == 1);
        if (config != null)
            return config;

        config = new GlobalTaxConfig { Id = 1, TaxRate = 0 };
        _context.GlobalTaxConfigs.Add(config);
        await _context.SaveChangesAsync();
        return config;
    }
}

public sealed class OrderCommandRepository : IOrderCommandRepository
{
    private readonly SalesDbContext _context;

    public OrderCommandRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<OrderCommand?> GetLatestByOrderIdAsync(int orderId)
    {
        return await _context.OrderCommands
            .Where(c => c.OrderId == orderId)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();
    }

    public OrderCommand Add(OrderCommand command) => _context.OrderCommands.Add(command).Entity;
}

public sealed class CommandItemRepository : ICommandItemRepository
{
    private readonly SalesDbContext _context;

    public CommandItemRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CommandItem>> GetByCommandIdAsync(int commandId, bool includeOrderItem = false, bool includeStation = false)
    {
        IQueryable<CommandItem> query = _context.CommandItems.Where(ci => ci.CommandId == commandId);
        if (includeOrderItem)
            query = query.Include(ci => ci.OrderItem);
        if (includeStation)
            query = query.Include(ci => ci.Station);
        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<CommandItem>> GetByStationTypeIdAsync(int stationTypeId)
    {
        return await _context.CommandItems
            .Include(ci => ci.Station).ThenInclude(s => s!.Type)
            .Include(ci => ci.OrderItem).ThenInclude(oi => oi!.Status)
            .Include(ci => ci.OrderItem).ThenInclude(oi => oi!.Order)
            .Where(ci => ci.Station != null && ci.Station.TypeId == stationTypeId && ci.OrderItem != null)
            .OrderBy(ci => ci.CommandId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CommandItem>> GetAllWithOrderItemStatusAsync()
    {
        return await _context.CommandItems
            .Include(ci => ci.OrderItem).ThenInclude(oi => oi!.Status)
            .Where(ci => ci.OrderItem != null)
            .ToListAsync();
    }

    public async Task RemoveByOrderItemIdAsync(int orderItemId)
    {
        var items = await _context.CommandItems.Where(ci => ci.OrderItemId == orderItemId).ToListAsync();
        _context.CommandItems.RemoveRange(items);
    }

    public CommandItem Add(CommandItem commandItem) => _context.CommandItems.Add(commandItem).Entity;
}

public sealed class SalesUnitOfWork : ISalesUnitOfWork
{
    private readonly SalesDbContext _context;

    public SalesUnitOfWork(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
