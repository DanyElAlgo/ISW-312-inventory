using InventoryAPI.DTOs;
using InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Services;

public class PosService
{
    private const int OutOfStockStatusId = 4;
    private readonly InventoryDbContext _context;

    public PosService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<GlobalTaxConfigDto> GetGlobalTaxAsync()
    {
        var config = await GetOrCreateTaxConfigAsync();
        return new GlobalTaxConfigDto { TaxRate = config.TaxRate };
    }

    public async Task<GlobalTaxConfigDto> UpdateGlobalTaxAsync(GlobalTaxConfigDto dto)
    {
        if (dto.TaxRate < 0)
            throw new ArgumentException("Tax rate cannot be negative.");

        var config = await GetOrCreateTaxConfigAsync();
        config.TaxRate = dto.TaxRate;
        await _context.SaveChangesAsync();

        return new GlobalTaxConfigDto { TaxRate = config.TaxRate };
    }

    public async Task<PosAccountDto> CreateAccountAsync(OpenAccountCreateDto dto)
    {
        var openStatusId = await GetOrCreateOpenStatusIdAsync();
        var taxConfig = await GetOrCreateTaxConfigAsync();

        var ticket = new OrderTicket
        {
            CustomerId = dto.CustomerId,
            StatusId = openStatusId,
            TaxRateSnapshot = taxConfig.TaxRate
        };

        _context.OrderTickets.Add(ticket);
        await _context.SaveChangesAsync();

        return await GetAccountAsync(ticket.Id) ?? throw new InvalidOperationException("Could not create account.");
    }

    public async Task<List<PosAccountDto>> GetOpenAccountsAsync()
    {
        var openStatusId = await GetOrCreateOpenStatusIdAsync();

        var ids = await _context.OrderTickets
            .Where(t => t.StatusId == openStatusId)
            .Select(t => t.Id)
            .ToListAsync();

        var results = new List<PosAccountDto>();
        foreach (var id in ids)
        {
            var account = await GetAccountAsync(id);
            if (account != null)
                results.Add(account);
        }

        return results;
    }

    public async Task<PosAccountDto?> GetAccountAsync(int ticketId)
    {
        var ticket = await _context.OrderTickets
            .Include(t => t.Status)
            .Include(t => t.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return null;

        var waiter = await GetAssignedWaiterAsync(ticket.Id);

        var items = ticket.OrderItems
            .Where(i => i.ProductId.HasValue && i.Product != null)
            .Select(i =>
            {
                var qty = i.Qty ?? 0;
                var unitPrice = i.Product?.Price ?? 0;
                return new PosOrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId!.Value,
                    ProductName = i.Product?.Name ?? string.Empty,
                    UnitPrice = unitPrice,
                    Quantity = qty,
                    Note = i.AdditionalNote,
                    LineTotal = unitPrice * (decimal)qty
                };
            })
            .ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var taxRate = ticket.TaxRateSnapshot ?? 0;
        var tax = subtotal * taxRate;

        return new PosAccountDto
        {
            TicketId = ticket.Id,
            AccountNumber = BuildAccountNumber(ticket.Id),
            Status = ticket.Status?.Name ?? "Open",
            WaiterId = waiter?.Id,
            WaiterName = waiter?.Name,
            TaxRate = taxRate,
            Subtotal = subtotal,
            Tax = tax,
            Total = subtotal + tax,
            Items = items
        };
    }

    public async Task<PosAccountDto?> AssignWaiterAsync(int ticketId, int waiterId)
    {
        var ticket = await _context.OrderTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
            return null;

        var waiter = await _context.Waiters.FirstOrDefaultAsync(w => w.Id == waiterId);
        if (waiter == null)
            throw new InvalidOperationException("Waiter not found.");

        var latestOrderCommand = await _context.OrderCommands
            .Where(c => c.OrderId == ticketId)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        if (latestOrderCommand == null)
        {
            latestOrderCommand = new OrderCommand
            {
                OrderId = ticketId,
                WaiterId = waiterId
            };
            _context.OrderCommands.Add(latestOrderCommand);
        }
        else
        {
            latestOrderCommand.WaiterId = waiterId;
        }

        await _context.SaveChangesAsync();
        return await GetAccountAsync(ticketId);
    }

    public async Task<PosAccountDto?> AddItemAsync(int ticketId, AddOrderItemDto dto)
    {
        var ticket = await _context.OrderTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
            return null;

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
        if (product == null)
            throw new InvalidOperationException("Product not found.");

        if (product.IsActive == false)
            throw new InvalidOperationException("Product is inactive and cannot be added to the ticket.");

        var availableStockExists = await _context.WarehouseProducts.AnyAsync(wp =>
            wp.ProductId == dto.ProductId &&
            wp.StatusId != OutOfStockStatusId &&
            (wp.StockLeft ?? 0) > 0);

        if (!availableStockExists)
            throw new InvalidOperationException("Product is out of stock and cannot be added to the ticket.");

        var pendingStatusId = await GetOrCreatePendingStatusIdAsync();

        var item = new OrderItem
        {
            OrderId = ticketId,
            ProductId = dto.ProductId,
            Qty = dto.Quantity,
            AdditionalNote = dto.Note,
            StatusId = pendingStatusId
        };

        _context.OrderItems.Add(item);
        await _context.SaveChangesAsync();

        return await GetAccountAsync(ticketId);
    }

    public async Task<PosAccountDto?> UpdateItemAsync(int ticketId, int orderItemId, UpdateOrderItemDto dto)
    {
        var ticketExists = await _context.OrderTickets.AnyAsync(t => t.Id == ticketId);
        if (!ticketExists)
            return null;

        var item = await _context.OrderItems.FirstOrDefaultAsync(i => i.Id == orderItemId && i.OrderId == ticketId);
        if (item == null)
            throw new InvalidOperationException("Order item not found.");

        if (dto.Quantity.HasValue)
            item.Qty = dto.Quantity.Value;

        if (dto.Note != null)
            item.AdditionalNote = dto.Note;

        await _context.SaveChangesAsync();
        return await GetAccountAsync(ticketId);
    }

    public async Task ValidateCheckoutAsync(int ticketId)
    {
        var ticketExists = await _context.OrderTickets.AnyAsync(t => t.Id == ticketId);
        if (!ticketExists)
            throw new InvalidOperationException("Ticket not found.");

        var waiter = await GetAssignedWaiterAsync(ticketId);
        if (waiter == null)
            throw new InvalidOperationException("Waiter is required before checkout.");
    }

    public async Task<CommandSendResultDto> SendCommandAsync(int ticketId)
    {
        var ticket = await _context.OrderTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new CommandSendResultDto
            {
                Success = false,
                Message = "Ticket not found."
            };
        }

        var waiter = await GetAssignedWaiterAsync(ticketId);
        if (waiter == null)
        {
            return new CommandSendResultDto
            {
                Success = false,
                Message = "A waiter must be assigned before sending command."
            };
        }

        var unsentItems = await _context.OrderItems
            .Include(i => i.Product)
            .Where(i => i.OrderId == ticketId && !i.CommandItems.Any())
            .ToListAsync();

        if (!unsentItems.Any())
        {
            return new CommandSendResultDto
            {
                Success = false,
                Message = "There are no new items to send.",
                ItemsSent = 0
            };
        }

        var command = new OrderCommand
        {
            OrderId = ticketId,
            WaiterId = waiter.Id
        };

        _context.OrderCommands.Add(command);
        await _context.SaveChangesAsync();

        var sentCount = 0;
        foreach (var item in unsentItems)
        {
            if (!item.ProductId.HasValue)
                continue;

            var stationId = await ResolveStationForProductAsync(item.ProductId.Value);
            if (!stationId.HasValue)
                continue;

            _context.CommandItems.Add(new CommandItem
            {
                CommandId = command.Id,
                OrderItemId = item.Id,
                StationId = stationId
            });

            sentCount++;
        }

        if (sentCount == 0)
        {
            return new CommandSendResultDto
            {
                Success = false,
                Message = "No station coverage found for the new items.",
                CommandId = command.Id,
                ItemsSent = 0
            };
        }

        await _context.SaveChangesAsync();

        return new CommandSendResultDto
        {
            Success = true,
            Message = "Command sent successfully.",
            CommandId = command.Id,
            ItemsSent = sentCount
        };
    }

    public async Task<List<KdsItemDto>> GetKdsPendingByStationTypeAsync(string stationType)
    {
        var normalized = stationType.Trim().ToLower();
        var search = normalized switch
        {
            "cocina" => new[] { "kitchen", "cocina" },
            "bar" => new[] { "bar" },
            _ => new[] { normalized }
        };

        return await _context.CommandItems
            .Include(ci => ci.Station)
                .ThenInclude(s => s!.Type)
            .Include(ci => ci.OrderItem)
                .ThenInclude(oi => oi!.Product)
            .Include(ci => ci.OrderItem)
                .ThenInclude(oi => oi!.Status)
            .Where(ci =>
                ci.Station != null &&
                ci.Station.Type != null &&
                ci.Station.Type.Name != null &&
                search.Contains(ci.Station.Type.Name.ToLower()) &&
                ci.OrderItem != null &&
                (ci.OrderItem.Status == null ||
                 ci.OrderItem.Status.Name == null ||
                 ci.OrderItem.Status.Name.ToLower() == "pending" ||
                 ci.OrderItem.Status.Name.ToLower() == "pendiente"))
            .OrderBy(ci => ci.CommandId)
            .Select(ci => new KdsItemDto
            {
                CommandId = ci.CommandId ?? 0,
                TicketId = ci.OrderItem!.OrderId ?? 0,
                OrderItemId = ci.OrderItemId ?? 0,
                StationName = ci.Station!.Name ?? string.Empty,
                StationType = ci.Station.Type!.Name ?? string.Empty,
                ProductName = ci.OrderItem.Product!.Name ?? string.Empty,
                Quantity = ci.OrderItem.Qty ?? 0,
                Note = ci.OrderItem.AdditionalNote
            })
            .ToListAsync();
    }

    private async Task<GlobalTaxConfig> GetOrCreateTaxConfigAsync()
    {
        var config = await _context.GlobalTaxConfigs.FirstOrDefaultAsync(c => c.Id == 1);
        if (config != null)
            return config;

        config = new GlobalTaxConfig
        {
            Id = 1,
            TaxRate = 0
        };

        _context.GlobalTaxConfigs.Add(config);
        await _context.SaveChangesAsync();
        return config;
    }

    private async Task<int> GetOrCreateOpenStatusIdAsync()
    {
        var status = await _context.OrderStatuses.FirstOrDefaultAsync(s =>
            s.Name != null && (s.Name.ToLower() == "open" || s.Name.ToLower() == "abierto" || s.Name.ToLower() == "pending"));

        if (status != null)
            return status.Id;

        var created = new OrderStatus { Name = "Open", Description = "Open account" };
        _context.OrderStatuses.Add(created);
        await _context.SaveChangesAsync();
        return created.Id;
    }

    private async Task<int> GetOrCreatePendingStatusIdAsync()
    {
        var status = await _context.OrderStatuses.FirstOrDefaultAsync(s =>
            s.Name != null && (s.Name.ToLower() == "pending" || s.Name.ToLower() == "pendiente"));

        if (status != null)
            return status.Id;

        var created = new OrderStatus { Name = "Pending", Description = "Pending item" };
        _context.OrderStatuses.Add(created);
        await _context.SaveChangesAsync();
        return created.Id;
    }

    private async Task<Waiter?> GetAssignedWaiterAsync(int ticketId)
    {
        var waiterId = await _context.OrderCommands
            .Where(c => c.OrderId == ticketId && c.WaiterId.HasValue)
            .OrderByDescending(c => c.Id)
            .Select(c => c.WaiterId)
            .FirstOrDefaultAsync();

        if (!waiterId.HasValue)
            return null;

        return await _context.Waiters.FirstOrDefaultAsync(w => w.Id == waiterId.Value);
    }

    private async Task<int?> ResolveStationForProductAsync(int productId)
    {
        var categoryId = await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => p.CategoryId)
            .FirstOrDefaultAsync();

        if (!categoryId.HasValue)
            return null;

        var stationTypeId = await _context.StationTypes
            .Where(st => st.Categories.Any(c => c.Id == categoryId.Value))
            .Select(st => (int?)st.Id)
            .FirstOrDefaultAsync();

        if (!stationTypeId.HasValue)
            return null;

        return await _context.Stations
            .Where(s => s.TypeId == stationTypeId.Value)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();
    }

    private static string BuildAccountNumber(int ticketId)
    {
        return $"ACC-{ticketId:D6}";
    }
}
