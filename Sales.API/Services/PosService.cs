using Sales.API.DTOs;
using Sales.API.HttpClients;
using Sales.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Sales.API.Services;

public class PosService
{
    private readonly SalesDbContext _context;
    private readonly InventoryClient _inventoryClient;

    public PosService(SalesDbContext context, InventoryClient inventoryClient)
    {
        _context = context;
        _inventoryClient = inventoryClient;
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
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return null;

        var waiter = await GetAssignedWaiterAsync(ticket.Id);

        // Product name and price come from snapshot columns — no HTTP call needed here
        var items = ticket.OrderItems
            .Where(i => i.ProductId.HasValue)
            .Select(i =>
            {
                var qty = i.Qty ?? 0;
                var unitPrice = i.UnitPrice ?? 0;
                return new PosOrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId!.Value,
                    ProductName = i.ProductName ?? string.Empty,
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

        // Validate product via Inventory.API HTTP call
        var productRef = await _inventoryClient.GetProductReferenceAsync(dto.ProductId);
        if (productRef == null)
            throw new InvalidOperationException("Product not found.");

        if (!productRef.IsActive)
            throw new InvalidOperationException("Product is inactive and cannot be added to the ticket.");

        if (!productRef.HasAvailableStock)
            throw new InvalidOperationException("Product is out of stock and cannot be added to the ticket.");

        var pendingStatusId = await GetOrCreatePendingStatusIdAsync();

        var item = new OrderItem
        {
            OrderId = ticketId,
            ProductId = dto.ProductId,
            // Snapshot product data at order time so history remains stable
            ProductName = productRef.Name,
            UnitPrice = productRef.Price,
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
            return new CommandSendResultDto { Success = false, Message = "Ticket not found." };

        var waiter = await GetAssignedWaiterAsync(ticketId);
        if (waiter == null)
            return new CommandSendResultDto
            {
                Success = false,
                Message = "A waiter must be assigned before sending command."
            };

        var unsentItems = await _context.OrderItems
            .Where(i => i.OrderId == ticketId && !i.CommandItems.Any())
            .ToListAsync();

        if (!unsentItems.Any())
            return new CommandSendResultDto
            {
                Success = false,
                Message = "There are no new items to send.",
                ItemsSent = 0
            };

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
            return new CommandSendResultDto
            {
                Success = false,
                Message = "No station coverage found for the new items.",
                CommandId = command.Id,
                ItemsSent = 0
            };

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

        var activeStatuses = new[] { "pending", "pendiente", "en preparacion", "en preparación", "in preparation" };

        return await _context.CommandItems
            .Include(ci => ci.Station)
                .ThenInclude(s => s!.Type)
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
                 activeStatuses.Contains(ci.OrderItem.Status.Name.ToLower())))
            .OrderBy(ci => ci.CommandId)
            .Select(ci => new KdsItemDto
            {
                CommandId = ci.CommandId ?? 0,
                TicketId = ci.OrderItem!.OrderId ?? 0,
                OrderItemId = ci.OrderItemId ?? 0,
                StationName = ci.Station!.Name ?? string.Empty,
                StationType = ci.Station.Type!.Name ?? string.Empty,
                ProductName = ci.OrderItem.ProductName ?? string.Empty,
                Quantity = ci.OrderItem.Qty ?? 0,
                Note = ci.OrderItem.AdditionalNote,
                Status = ci.OrderItem.Status?.Name ?? "Pending"
            })
            .ToListAsync();
    }

    public async Task<KdsItemDto?> AdvanceKdsItemStatusAsync(int orderItemId)
    {
        var item = await _context.OrderItems
            .Include(i => i.Status)
            .FirstOrDefaultAsync(i => i.Id == orderItemId);

        if (item == null)
            return null;

        var currentStatus = item.Status?.Name?.ToLower() ?? "pending";
        int targetStatusId;

        if (currentStatus is "pending" or "pendiente")
            targetStatusId = await GetOrCreateInPreparationStatusIdAsync();
        else if (currentStatus is "en preparacion" or "en preparación" or "in preparation")
            targetStatusId = await GetOrCreateReadyStatusIdAsync();
        else
            throw new InvalidOperationException("Item is already in final state (Listo/Ready).");

        item.StatusId = targetStatusId;
        await _context.SaveChangesAsync();

        var commandItem = await _context.CommandItems
            .Include(ci => ci.Station)
                .ThenInclude(s => s!.Type)
            .FirstOrDefaultAsync(ci => ci.OrderItemId == orderItemId);

        var newStatus = await _context.OrderStatuses.FindAsync(targetStatusId);
        return new KdsItemDto
        {
            CommandId = commandItem?.CommandId ?? 0,
            TicketId = item.OrderId ?? 0,
            OrderItemId = item.Id,
            StationName = commandItem?.Station?.Name ?? string.Empty,
            StationType = commandItem?.Station?.Type?.Name ?? string.Empty,
            ProductName = item.ProductName ?? string.Empty,
            Quantity = item.Qty ?? 0,
            Note = item.AdditionalNote,
            Status = newStatus?.Name ?? string.Empty
        };
    }

    public async Task<CommandReprintDto?> GetCommandReprintAsync(int commandId)
    {
        var command = await _context.OrderCommands
            .Include(c => c.Waiter)
            .FirstOrDefaultAsync(c => c.Id == commandId);

        if (command == null)
            return null;

        var commandItems = await _context.CommandItems
            .Include(ci => ci.OrderItem)
            .Include(ci => ci.Station)
            .Where(ci => ci.CommandId == commandId)
            .ToListAsync();

        return new CommandReprintDto
        {
            CommandId = commandId,
            TicketId = command.OrderId ?? 0,
            WaiterName = command.Waiter?.Name ?? string.Empty,
            PrintedAt = DateTime.UtcNow,
            Items = commandItems.Select(ci => new CommandReprintItemDto
            {
                ProductName = ci.OrderItem?.ProductName ?? string.Empty,
                Quantity = ci.OrderItem?.Qty ?? 0,
                Note = ci.OrderItem?.AdditionalNote,
                StationName = ci.Station?.Name ?? string.Empty
            }).ToList()
        };
    }

    public async Task<CheckoutResultDto> CheckoutAsync(int ticketId, CheckoutDto dto)
    {
        var ticket = await _context.OrderTickets
            .Include(t => t.Status)
            .Include(t => t.OrderItems)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new InvalidOperationException("Account not found.");

        var statusName = ticket.Status?.Name?.ToLower() ?? "";
        if (statusName is not ("open" or "abierto"))
            throw new InvalidOperationException("Account is not open and cannot be checked out.");

        var waiter = await GetAssignedWaiterAsync(ticketId);
        if (waiter == null)
            throw new InvalidOperationException("A waiter must be assigned before checkout.");

        var items = ticket.OrderItems.Where(i => i.ProductId.HasValue).ToList();
        if (!items.Any())
            throw new InvalidOperationException("Account has no items.");

        var paymentType = await _context.PaymentTypes.FindAsync(dto.PaymentTypeId);
        if (paymentType == null)
            throw new InvalidOperationException("Payment type not found.");

        // Validate stock for all items
        var checkDto = new Shared.Contracts.DTOs.BulkStockCheckDto
        {
            Items = items.Select(i => new Shared.Contracts.DTOs.BulkStockCheckItemDto
            {
                ProductId = i.ProductId!.Value,
                Quantity = i.Qty ?? 0
            }).ToList()
        };

        var validation = await _inventoryClient.ValidateBulkStockAsync(checkDto);
        if (validation == null)
            throw new InvalidOperationException("Could not validate stock. Inventory service unavailable.");

        if (!validation.AllAvailable)
        {
            var shortages = validation.Lines
                .Where(l => !l.Sufficient)
                .Select(l => $"{l.ProductName} (required: {l.Required}, available: {l.Available})");
            throw new InvalidOperationException(
                $"Insufficient stock for: {string.Join(", ", shortages)}");
        }

        // Deduct stock
        var deductDto = new Shared.Contracts.DTOs.BulkStockDeductDto
        {
            Items = items.Select(i => new Shared.Contracts.DTOs.BulkStockCheckItemDto
            {
                ProductId = i.ProductId!.Value,
                Quantity = i.Qty ?? 0
            }).ToList(),
            Reason = $"Sale — ticket #{ticketId}"
        };

        var deduction = await _inventoryClient.DeductBulkStockAsync(deductDto);
        if (deduction == null || !deduction.Success)
            throw new InvalidOperationException(deduction?.Message ?? "Stock deduction failed.");

        // Create payment
        var payment = new Payment
        {
            OrderId = ticketId,
            PaymentTypeId = dto.PaymentTypeId,
            PaidAt = DateTime.UtcNow
        };
        _context.Payments.Add(payment);

        // Mark ticket as paid
        ticket.StatusId = await GetOrCreatePaidStatusIdAsync();
        await _context.SaveChangesAsync();

        // Compute total for result
        var subtotal = items.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
        var tax = subtotal * (ticket.TaxRateSnapshot ?? 0);

        return new CheckoutResultDto
        {
            Success = true,
            Message = "Payment confirmed.",
            PaymentId = payment.Id,
            Total = subtotal + tax
        };
    }

    public async Task<PosAccountDto?> CancelAccountAsync(int ticketId)
    {
        var ticket = await _context.OrderTickets
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return null;

        var statusName = ticket.Status?.Name?.ToLower() ?? "";
        if (statusName is not ("open" or "abierto"))
            throw new InvalidOperationException("Only open accounts can be cancelled.");

        ticket.StatusId = await GetOrCreateCancelledStatusIdAsync();
        await _context.SaveChangesAsync();

        return await GetAccountAsync(ticketId);
    }

    private async Task<GlobalTaxConfig> GetOrCreateTaxConfigAsync()
    {
        var config = await _context.GlobalTaxConfigs.FirstOrDefaultAsync(c => c.Id == 1);
        if (config != null)
            return config;

        config = new GlobalTaxConfig { Id = 1, TaxRate = 0 };
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
        var productRef = await _inventoryClient.GetProductReferenceAsync(productId);
        if (productRef?.CategoryId == null)
            return null;

        var categoryId = productRef.CategoryId.Value;

        // station_coverage stores (station_type_id, category_id) — category_id is a logical ref
        var stationTypeId = await _context.Database
            .SqlQueryRaw<int>(
                "SELECT station_type_id AS \"Value\" FROM sales.station_coverage WHERE category_id = {0} LIMIT 1",
                categoryId)
            .FirstOrDefaultAsync();

        if (stationTypeId == 0)
            return null;

        return await _context.Stations
            .Where(s => s.TypeId == stationTypeId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<PosAccountDto?> RemoveItemAsync(int ticketId, int orderItemId)
    {
        var ticketExists = await _context.OrderTickets.AnyAsync(t => t.Id == ticketId);
        if (!ticketExists)
            return null;

        var item = await _context.OrderItems
            .FirstOrDefaultAsync(i => i.Id == orderItemId && i.OrderId == ticketId);
        if (item == null)
            throw new InvalidOperationException("Order item not found.");

        _context.OrderItems.Remove(item);
        await _context.SaveChangesAsync();
        return await GetAccountAsync(ticketId);
    }

    private async Task<int> GetOrCreateInPreparationStatusIdAsync()
    {
        var status = await _context.OrderStatuses.FirstOrDefaultAsync(s =>
            s.Name != null && (s.Name.ToLower() == "en preparacion" || s.Name.ToLower() == "en preparación" || s.Name.ToLower() == "in preparation"));

        if (status != null)
            return status.Id;

        var created = new OrderStatus { Name = "En Preparación", Description = "Item being prepared" };
        _context.OrderStatuses.Add(created);
        await _context.SaveChangesAsync();
        return created.Id;
    }

    private async Task<int> GetOrCreateReadyStatusIdAsync()
    {
        var status = await _context.OrderStatuses.FirstOrDefaultAsync(s =>
            s.Name != null && (s.Name.ToLower() == "listo" || s.Name.ToLower() == "ready"));

        if (status != null)
            return status.Id;

        var created = new OrderStatus { Name = "Listo", Description = "Item ready" };
        _context.OrderStatuses.Add(created);
        await _context.SaveChangesAsync();
        return created.Id;
    }

    private async Task<int> GetOrCreatePaidStatusIdAsync()
    {
        var status = await _context.OrderStatuses.FirstOrDefaultAsync(s =>
            s.Name != null && (s.Name.ToLower() == "paid" || s.Name.ToLower() == "pagado"));

        if (status != null)
            return status.Id;

        var created = new OrderStatus { Name = "Pagado", Description = "Paid ticket" };
        _context.OrderStatuses.Add(created);
        await _context.SaveChangesAsync();
        return created.Id;
    }

    private async Task<int> GetOrCreateCancelledStatusIdAsync()
    {
        var status = await _context.OrderStatuses.FirstOrDefaultAsync(s =>
            s.Name != null && (s.Name.ToLower() == "cancelled" || s.Name.ToLower() == "cancelado"));

        if (status != null)
            return status.Id;

        var created = new OrderStatus { Name = "Cancelado", Description = "Cancelled account" };
        _context.OrderStatuses.Add(created);
        await _context.SaveChangesAsync();
        return created.Id;
    }

    private static string BuildAccountNumber(int ticketId) => $"ACC-{ticketId:D6}";
}
