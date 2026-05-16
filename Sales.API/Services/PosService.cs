using Sales.API.DTOs;
using Sales.API.HttpClients;
using Sales.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Sales.API.Services;

public class PosService
{
    private readonly SalesDbContext _context;
    private readonly InventoryClient _inventoryClient;
    private readonly InventoryIntegrationOptions _integrationOptions;

    public PosService(
        SalesDbContext context,
        InventoryClient inventoryClient,
        IOptions<InventoryIntegrationOptions> integrationOptions)
    {
        _context = context;
        _inventoryClient = inventoryClient;
        _integrationOptions = integrationOptions.Value;
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

        var items = ticket.OrderItems
            .Where(i => !string.IsNullOrWhiteSpace(i.ProductCen) || i.ProductId.HasValue)
            .Select(i =>
            {
                var qty = i.Qty ?? 0;
                var unitPrice = i.UnitPrice ?? 0;
                return new PosOrderItemDto
                {
                    Id = i.Id,
                    ProductCen = i.ProductCen ?? i.ProductId?.ToString() ?? string.Empty,
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

        if (string.IsNullOrWhiteSpace(dto.ProductCen))
            throw new InvalidOperationException("Product CEN is required.");

        var product = await _inventoryClient.GetProductAsync(_integrationOptions.CompanyCen, dto.ProductCen);
        if (product == null)
            throw new InvalidOperationException("Product not found.");

        var status = product.Status.Trim().ToUpperInvariant();
        if (status is "INACTIVE")
            throw new InvalidOperationException("Product is inactive and cannot be added to the ticket.");

        if (status is "OUT_OF_STOCK")
            throw new InvalidOperationException("Product is out of stock and cannot be added to the ticket.");

        var stock = await _inventoryClient.GetStockAsync(
            _integrationOptions.CompanyCen,
            product.ProductCen,
            _integrationOptions.WarehouseCen);

        var stockItem = stock?.FirstOrDefault();
        if (stockItem == null || stockItem.AvailableQuantity <= 0)
            throw new InvalidOperationException("Product is out of stock and cannot be added to the ticket.");

        var pendingStatusId = await GetOrCreatePendingStatusIdAsync();

        var item = new OrderItem
        {
            OrderId = ticketId,
            ProductCen = product.ProductCen,
            ProductName = product.Name,
            UnitPrice = product.SalePrice,
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
            if (string.IsNullOrWhiteSpace(item.ProductCen))
                continue;

            var stationId = await ResolveStationForProductAsync(item.ProductCen);
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
                Status = ci.OrderItem.Status!.Name ?? "Pending"
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

        var items = ticket.OrderItems
            .Where(i => !string.IsNullOrWhiteSpace(i.ProductCen))
            .ToList();
        if (!items.Any())
            throw new InvalidOperationException("Account has no items.");

        var paymentType = await _context.PaymentTypes.FindAsync(dto.PaymentTypeId);
        if (paymentType == null)
            throw new InvalidOperationException("Payment type not found.");

        var checkDto = new StockValidationRequest
        {
            WarehouseCen = _integrationOptions.WarehouseCen,
            Source = _integrationOptions.Source,
            ReferenceCen = BuildAccountNumber(ticketId),
            Items = items.Select(i => new StockValidationItemDto
            {
                ProductCen = i.ProductCen ?? string.Empty,
                Quantity = (decimal)(i.Qty ?? 0)
            }).ToList()
        };

        var validation = await _inventoryClient.ValidateStockAsync(_integrationOptions.CompanyCen, checkDto);
        if (validation == null)
            throw new InvalidOperationException("Could not validate stock. Inventory service unavailable.");

        if (!validation.IsValid)
        {
            var shortages = validation.Requirements
                .Select(l => $"{l.ProductName} (required: {l.RequestedQuantity}, available: {l.AvailableQuantity})");
            throw new InvalidOperationException(
                $"Insufficient stock for: {string.Join(", ", shortages)}");
        }

        var deductDto = new StockConsumeRequest
        {
            WarehouseCen = _integrationOptions.WarehouseCen,
            Source = _integrationOptions.Source,
            ReferenceCen = BuildAccountNumber(ticketId),
            Reason = $"Sale — ticket #{ticketId}",
            Items = items.Select(i => new StockConsumeItemDto
            {
                ProductCen = i.ProductCen ?? string.Empty,
                Quantity = (decimal)(i.Qty ?? 0)
            }).ToList()
        };

        var deduction = await _inventoryClient.ConsumeStockAsync(_integrationOptions.CompanyCen, deductDto);
        if (deduction == null)
            throw new InvalidOperationException("Stock deduction failed.");

        if (!deduction.Success)
            throw new InvalidOperationException(deduction.Message ?? "Stock deduction failed.");

        var payment = new Payment
        {
            OrderId = ticketId,
            PaymentTypeId = dto.PaymentTypeId,
            PaidAt = DateTime.UtcNow
        };
        _context.Payments.Add(payment);

        ticket.StatusId = await GetOrCreatePaidStatusIdAsync();
        await _context.SaveChangesAsync();

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

    private async Task<int?> ResolveStationForProductAsync(string productCen)
    {
        var product = await _inventoryClient.GetProductAsync(_integrationOptions.CompanyCen, productCen);
        if (product == null || string.IsNullOrWhiteSpace(product.StationCode))
            return null;

        var normalized = product.StationCode.Trim().ToLower();

        var stationTypeId = await _context.StationTypes
            .Where(s => s.Name != null && s.Name.ToLower() == normalized)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        if (stationTypeId != 0)
        {
            return await _context.Stations
                .Where(s => s.TypeId == stationTypeId)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();
        }

        return await _context.Stations
            .Where(s => s.Name != null && s.Name.ToLower() == normalized)
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

    // ── Contract-compliant methods ─────────────────────────────────────────────

    public async Task<List<WaiterContractResponse>> GetWaitersAsync()
    {
        return await _context.Waiters
            .OrderBy(w => w.Name)
            .Select(w => new WaiterContractResponse
            {
                WaiterCen = w.Id.ToString(),
                Name = w.Name ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task<List<TicketContractResponse>> GetTicketsAsync(string companyCen)
    {
        var openStatusId = await GetOrCreateOpenStatusIdAsync();

        var tickets = await _context.OrderTickets
            .Include(t => t.Status)
            .Include(t => t.OrderItems)
            .Include(t => t.OrderCommands)
            .Where(t => t.StatusId == openStatusId)
            .ToListAsync();

        var result = new List<TicketContractResponse>();
        foreach (var t in tickets)
            result.Add(await MapTicketToContractAsync(t, companyCen));

        return result;
    }

    public async Task<TicketContractResponse?> CreateTicketContractAsync(string companyCen, CreateTicketContractRequest request)
    {
        var openStatusId = await GetOrCreateOpenStatusIdAsync();
        var taxConfig = await GetOrCreateTaxConfigAsync();

        var today = DateTime.UtcNow.Date;
        var dailyCount = await _context.OrderTickets
            .CountAsync(t => t.CreatedAt >= today && t.CreatedAt < today.AddDays(1));

        var ticket = new OrderTicket
        {
            StatusId = openStatusId,
            TaxRateSnapshot = taxConfig.TaxRate,
            CreatedAt = DateTime.UtcNow,
            DailyNumber = dailyCount + 1
        };

        _context.OrderTickets.Add(ticket);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.WaiterCen) && int.TryParse(request.WaiterCen, out var waiterId))
        {
            var waiterExists = await _context.Waiters.AnyAsync(w => w.Id == waiterId);
            if (waiterExists)
            {
                _context.OrderCommands.Add(new OrderCommand { OrderId = ticket.Id, WaiterId = waiterId });
                await _context.SaveChangesAsync();
            }
        }

        var created = await _context.OrderTickets
            .Include(t => t.Status)
            .Include(t => t.OrderItems)
            .Include(t => t.OrderCommands)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);

        return created == null ? null : await MapTicketToContractAsync(created, companyCen);
    }

    public async Task<List<TicketItemContractResponse>?> GetTicketItemsContractAsync(string ticketCen)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            return null;

        var exists = await _context.OrderTickets.AnyAsync(t => t.Id == ticketId);
        if (!exists)
            return null;

        var items = await _context.OrderItems
            .Include(i => i.Status)
            .Where(i => i.OrderId == ticketId)
            .ToListAsync();

        return items.Select(MapItemToContract).ToList();
    }

    public async Task<TicketItemContractResponse?> AddTicketItemContractAsync(string ticketCen, CreateTicketItemContractRequest request)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            return null;

        var ticket = await _context.OrderTickets
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return null;

        var statusName = ticket.Status?.Name?.ToLower() ?? "";
        if (statusName is not ("open" or "abierto"))
            throw new InvalidOperationException("Ticket is not open.");

        var product = await _inventoryClient.GetProductAsync(_integrationOptions.CompanyCen, request.ProductCen);
        if (product == null)
            throw new InvalidOperationException("Product not found.");

        var status = product.Status.Trim().ToUpperInvariant();
        if (status is "INACTIVE")
            throw new InvalidOperationException("Product is inactive.");
        if (status is "OUT_OF_STOCK")
            throw new InvalidOperationException("Product is out of stock.");

        var pendingStatusId = await GetOrCreatePendingStatusIdAsync();

        var item = new OrderItem
        {
            OrderId = ticketId,
            ProductCen = product.ProductCen,
            ProductName = product.Name,
            UnitPrice = product.SalePrice,
            Qty = request.Quantity,
            AdditionalNote = request.Note,
            StatusId = pendingStatusId,
            ResendCount = 0
        };

        _context.OrderItems.Add(item);
        await _context.SaveChangesAsync();

        var saved = await _context.OrderItems
            .Include(i => i.Status)
            .FirstOrDefaultAsync(i => i.Id == item.Id);

        return saved == null ? null : MapItemToContract(saved);
    }

    public async Task<TicketItemContractResponse?> UpdateTicketItemContractAsync(string ticketCen, string ticketItemCen, UpdateTicketItemContractRequest request)
    {
        if (!int.TryParse(ticketCen, out var ticketId) || !int.TryParse(ticketItemCen, out var itemId))
            return null;

        var item = await _context.OrderItems
            .Include(i => i.Status)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.OrderId == ticketId);

        if (item == null)
            return null;

        if (request.Quantity.HasValue)
            item.Qty = request.Quantity.Value;

        if (request.Note != null)
            item.AdditionalNote = request.Note;

        await _context.SaveChangesAsync();
        return MapItemToContract(item);
    }

    public async Task<TicketItemContractResponse?> ResendTicketItemAsync(string ticketCen, string ticketItemCen)
    {
        if (!int.TryParse(ticketCen, out var ticketId) || !int.TryParse(ticketItemCen, out var itemId))
            return null;

        var item = await _context.OrderItems
            .Include(i => i.Status)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.OrderId == ticketId);

        if (item == null)
            return null;

        var pendingStatusId = await GetOrCreatePendingStatusIdAsync();
        item.StatusId = pendingStatusId;
        item.ResendCount++;
        item.SentAt = null;

        // Remove old CommandItems so it reappears in KDS as unsent
        var oldCommandItems = await _context.CommandItems
            .Where(ci => ci.OrderItemId == itemId)
            .ToListAsync();
        _context.CommandItems.RemoveRange(oldCommandItems);

        await _context.SaveChangesAsync();

        var updated = await _context.OrderItems
            .Include(i => i.Status)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        return updated == null ? null : MapItemToContract(updated);
    }

    public async Task<List<TicketItemContractResponse>?> SendTicketToKitchenContractAsync(string companyCen, string ticketCen)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            return null;

        var ticket = await _context.OrderTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
            return null;

        var waiter = await GetAssignedWaiterAsync(ticketId);
        if (waiter == null)
            throw new InvalidOperationException("A waiter must be assigned before sending the ticket to kitchen.");

        var unsentItems = await _context.OrderItems
            .Include(i => i.Status)
            .Where(i => i.OrderId == ticketId && !i.CommandItems.Any())
            .ToListAsync();

        if (!unsentItems.Any())
            throw new InvalidOperationException("There are no new items to send.");

        var command = new OrderCommand { OrderId = ticketId, WaiterId = waiter.Id };
        _context.OrderCommands.Add(command);
        await _context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        foreach (var item in unsentItems)
        {
            if (string.IsNullOrWhiteSpace(item.ProductCen))
                continue;

            var stationId = await ResolveStationForProductAsync(item.ProductCen);
            if (!stationId.HasValue)
                continue;

            _context.CommandItems.Add(new CommandItem
            {
                CommandId = command.Id,
                OrderItemId = item.Id,
                StationId = stationId
            });

            item.SentAt = now;
        }

        await _context.SaveChangesAsync();

        var allItems = await _context.OrderItems
            .Include(i => i.Status)
            .Where(i => i.OrderId == ticketId)
            .ToListAsync();

        return allItems.Select(MapItemToContract).ToList();
    }

    public async Task<AssignTicketWaiterContractResponse?> AssignTicketWaiterContractAsync(string ticketCen, AssignTicketWaiterContractRequest request)
    {
        if (!int.TryParse(ticketCen, out var ticketId) || !int.TryParse(request.WaiterCen, out var waiterId))
            return null;

        var account = await AssignWaiterAsync(ticketId, waiterId);
        if (account == null)
            return null;

        return new AssignTicketWaiterContractResponse
        {
            TicketCen = ticketId.ToString(),
            WaiterCen = request.WaiterCen,
            WaiterName = account.WaiterName ?? string.Empty
        };
    }

    public async Task<CancelTicketContractResponse?> CancelTicketContractAsync(string ticketCen, string? reason)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            return null;

        var ticket = await _context.OrderTickets
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return null;

        var statusName = ticket.Status?.Name?.ToLower() ?? "";
        if (statusName is "paid" or "pagado")
            throw new InvalidOperationException("Cannot cancel a paid ticket.");

        if (statusName is "cancelled" or "cancelado")
            throw new InvalidOperationException("Ticket is already cancelled.");

        ticket.StatusId = await GetOrCreateCancelledStatusIdAsync();
        ticket.CancellationReason = reason;
        await _context.SaveChangesAsync();

        var updatedTicket = await _context.OrderTickets
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        return new CancelTicketContractResponse
        {
            TicketCen = ticketId.ToString(),
            Status = updatedTicket?.Status?.Name ?? "Cancelado"
        };
    }

    public async Task<TicketTotalsContractResponse?> GetTicketTotalsAsync(string ticketCen)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            return null;

        var ticket = await _context.OrderTickets
            .Include(t => t.OrderItems)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return null;

        var subtotal = ticket.OrderItems.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
        var taxRate = ticket.TaxRateSnapshot ?? 0;
        var taxAmount = subtotal * taxRate;

        return new TicketTotalsContractResponse
        {
            TicketCen = ticketId.ToString(),
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            Total = subtotal + taxAmount
        };
    }

    public async Task<(PayTicketContractResponse? success, ProcessRestaurantOrderPaymentResultDto? conflict)>
        PayTicketContractAsync(string companyCen, string ticketCen, string paymentMethodCode)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            throw new InvalidOperationException("Invalid ticketCen.");

        var ticket = await _context.OrderTickets
            .Include(t => t.Status)
            .Include(t => t.OrderItems)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new InvalidOperationException("Ticket not found.");

        var statusName = ticket.Status?.Name?.ToLower() ?? "";
        if (statusName is not ("open" or "abierto"))
            throw new InvalidOperationException("Ticket is not open.");

        var waiter = await GetAssignedWaiterAsync(ticketId);
        if (waiter == null)
            throw new InvalidOperationException("A waiter must be assigned before payment.");

        var items = ticket.OrderItems.Where(i => !string.IsNullOrWhiteSpace(i.ProductCen)).ToList();
        if (!items.Any())
            throw new InvalidOperationException("Ticket has no items.");

        var paymentType = await _context.PaymentTypes
            .FirstOrDefaultAsync(pt => pt.Code != null &&
                pt.Code.ToLower() == paymentMethodCode.ToLower());
        paymentType ??= await _context.PaymentTypes
            .FirstOrDefaultAsync(pt => pt.Name != null &&
                pt.Name.ToLower() == paymentMethodCode.ToLower());

        if (paymentType == null)
            throw new InvalidOperationException($"Payment method '{paymentMethodCode}' not found.");

        var validateDto = new StockValidationRequest
        {
            WarehouseCen = _integrationOptions.WarehouseCen,
            Source = _integrationOptions.Source,
            ReferenceCen = BuildAccountNumber(ticketId),
            Items = items.Select(i => new StockValidationItemDto
            {
                ProductCen = i.ProductCen!,
                Quantity = (decimal)(i.Qty ?? 0)
            }).ToList()
        };

        var validation = await _inventoryClient.ValidateStockAsync(_integrationOptions.CompanyCen, validateDto);
        if (validation == null)
            throw new InvalidOperationException("Could not validate stock.");

        if (!validation.IsValid)
        {
            var subtotal = items.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
            var tax = subtotal * (ticket.TaxRateSnapshot ?? 0);
            var conflict = new ProcessRestaurantOrderPaymentResultDto
            {
                IsSuccess = false,
                Message = "Insufficient stock.",
                Subtotal = subtotal,
                TaxAmount = tax,
                Total = subtotal + tax,
                Insufficiencies = validation.Requirements.Select(r => new StockInsufficiencyResponseDto
                {
                    ProductCen = r.ProductCen,
                    ProductName = r.ProductName,
                    WarehouseCen = r.WarehouseCen,
                    RequestedQuantity = (int)r.RequestedQuantity,
                    AvailableQuantity = (int)r.AvailableQuantity,
                    MissingQuantity = (int)r.MissingQuantity
                }).ToList()
            };
            return (null, conflict);
        }

        var consumeDto = new StockConsumeRequest
        {
            WarehouseCen = _integrationOptions.WarehouseCen,
            Source = _integrationOptions.Source,
            ReferenceCen = BuildAccountNumber(ticketId),
            Reason = $"Sale — ticket #{ticketId}",
            Items = items.Select(i => new StockConsumeItemDto
            {
                ProductCen = i.ProductCen!,
                Quantity = (decimal)(i.Qty ?? 0)
            }).ToList()
        };

        var deduction = await _inventoryClient.ConsumeStockAsync(_integrationOptions.CompanyCen, consumeDto);
        if (deduction == null || !deduction.Success)
            throw new InvalidOperationException(deduction?.Message ?? "Stock deduction failed.");

        var payment = new Payment
        {
            OrderId = ticketId,
            PaymentTypeId = paymentType.Id,
            PaidAt = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        ticket.StatusId = await GetOrCreatePaidStatusIdAsync();
        await _context.SaveChangesAsync();

        var sub = items.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
        var taxAmount = sub * (ticket.TaxRateSnapshot ?? 0);

        return (new PayTicketContractResponse
        {
            SaleCen = $"SALE-{payment.Id}",
            TicketCen = ticketId.ToString(),
            Status = "Pagado",
            Subtotal = sub,
            TaxAmount = taxAmount,
            Total = sub + taxAmount,
            InventoryDocumentCen = deduction.DocumentCen
        }, null);
    }

    public async Task<List<KdsTeamContractResponse>> GetKdsTeamsAsync()
    {
        var types = await _context.StationTypes
            .Include(t => t.Stations)
            .ToListAsync();

        var result = new List<KdsTeamContractResponse>();
        foreach (var t in types)
        {
            var categoryIds = await GetCategoryIdsForStationTypeAsync(t.Id);
            result.Add(new KdsTeamContractResponse
            {
                TeamCen = t.Id.ToString(),
                Name = t.Name ?? string.Empty,
                CategoryCens = categoryIds.Select(id => id.ToString()).ToList()
            });
        }
        return result;
    }

    public async Task<List<KdsItemContractResponse>?> GetKdsItemsByTeamAsync(string teamCen)
    {
        if (!int.TryParse(teamCen, out var stationTypeId))
            return null;

        var typeExists = await _context.StationTypes.AnyAsync(t => t.Id == stationTypeId);
        if (!typeExists)
            return null;

        var cutoff = DateTime.UtcNow.AddHours(-48);

        return await _context.CommandItems
            .Include(ci => ci.Station).ThenInclude(s => s!.Type)
            .Include(ci => ci.OrderItem).ThenInclude(oi => oi!.Status)
            .Where(ci =>
                ci.Station != null &&
                ci.Station.TypeId == stationTypeId &&
                ci.OrderItem != null)
            .OrderBy(ci => ci.CommandId)
            .Select(ci => new KdsItemContractResponse
            {
                TicketItemCen = ci.OrderItem!.Id.ToString(),
                TicketCen = (ci.OrderItem.OrderId ?? 0).ToString(),
                ProductCen = ci.OrderItem.ProductCen ?? string.Empty,
                ProductName = ci.OrderItem.ProductName ?? string.Empty,
                Quantity = (int)(ci.OrderItem.Qty ?? 0),
                Status = ci.OrderItem.Status!.Name ?? "Pending",
                Note = ci.OrderItem.AdditionalNote,
                ResendCount = ci.OrderItem.ResendCount,
                CreatedAt = ci.OrderItem.SentAt.HasValue
                    ? ci.OrderItem.SentAt.Value.ToString("O")
                    : string.Empty
            })
            .ToListAsync();
    }

    public async Task<bool?> UpdateKdsItemStatusContractAsync(string ticketItemCen, string newStatus)
    {
        if (!int.TryParse(ticketItemCen, out var orderItemId))
            return null;

        var item = await _context.OrderItems
            .Include(i => i.Status)
            .FirstOrDefaultAsync(i => i.Id == orderItemId);

        if (item == null)
            return null;

        var normalized = newStatus.Trim().ToLower();
        int targetStatusId = normalized switch
        {
            "created" or "pending" or "pendiente" => await GetOrCreatePendingStatusIdAsync(),
            "preparing" or "en preparacion" or "en preparación" => await GetOrCreateInPreparationStatusIdAsync(),
            "delivered" or "ready" or "listo" => await GetOrCreateReadyStatusIdAsync(),
            "canceled" or "cancelado" => await GetOrCreateCancelledStatusIdAsync(),
            _ => throw new InvalidOperationException($"Unknown KDS status '{newStatus}'. Valid values: created, preparing, delivered, canceled.")
        };

        item.StatusId = targetStatusId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> PrintTicketAsync(string ticketCen)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            throw new InvalidOperationException("Invalid ticketCen.");

        var ticket = await _context.OrderTickets
            .Include(t => t.Status)
            .Include(t => t.OrderItems)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new InvalidOperationException("Ticket not found.");

        var waiter = await GetAssignedWaiterAsync(ticketId);
        var subtotal = ticket.OrderItems.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
        var taxRate = ticket.TaxRateSnapshot ?? 0;
        var taxAmount = subtotal * taxRate;
        var total = subtotal + taxAmount;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        sb.AppendLine("<style>body{font-family:monospace;font-size:12px;margin:20px}");
        sb.AppendLine("table{width:100%;border-collapse:collapse}td{padding:2px 4px}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h2>Ticket #{ticket.DailyNumber}</h2>");
        sb.AppendLine($"<p>Fecha: {ticket.CreatedAt:yyyy-MM-dd HH:mm}</p>");
        if (waiter != null) sb.AppendLine($"<p>Mesero: {waiter.Name}</p>");
        sb.AppendLine("<table><tr><th align='left'>Producto</th><th>Cant</th><th>Precio</th><th>Total</th></tr>");
        foreach (var item in ticket.OrderItems)
        {
            var lineTotal = (item.UnitPrice ?? 0) * (decimal)(item.Qty ?? 0);
            sb.AppendLine($"<tr><td>{item.ProductName}</td><td align='center'>{item.Qty}</td><td align='right'>{item.UnitPrice:F2}</td><td align='right'>{lineTotal:F2}</td></tr>");
            if (!string.IsNullOrWhiteSpace(item.AdditionalNote))
                sb.AppendLine($"<tr><td colspan='4' style='font-style:italic;color:#666'>  Nota: {item.AdditionalNote}</td></tr>");
        }
        sb.AppendLine("</table><hr/>");
        sb.AppendLine($"<p>Subtotal: {subtotal:F2}</p>");
        sb.AppendLine($"<p>Impuesto ({taxRate:P0}): {taxAmount:F2}</p>");
        sb.AppendLine($"<p><strong>Total: {total:F2}</strong></p>");
        sb.AppendLine("</body></html>");

        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    private async Task<TicketContractResponse> MapTicketToContractAsync(OrderTicket ticket, string companyCen)
    {
        var waiter = await GetAssignedWaiterAsync(ticket.Id);
        var subtotal = ticket.OrderItems.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
        var taxRate = ticket.TaxRateSnapshot ?? 0;
        var taxAmount = subtotal * taxRate;

        return new TicketContractResponse
        {
            TicketCen = ticket.Id.ToString(),
            DailyNumber = ticket.DailyNumber,
            Status = ticket.Status?.Name ?? "Open",
            CreatedAt = ticket.CreatedAt.ToString("O"),
            WaiterCen = waiter?.Id.ToString(),
            CompanyCen = companyCen,
            TaxAmount = taxAmount
        };
    }

    private static TicketItemContractResponse MapItemToContract(OrderItem item)
    {
        return new TicketItemContractResponse
        {
            TicketItemCen = item.Id.ToString(),
            ProductCen = item.ProductCen ?? string.Empty,
            ProductName = item.ProductName ?? string.Empty,
            Quantity = (int)(item.Qty ?? 0),
            UnitPrice = item.UnitPrice ?? 0,
            Note = item.AdditionalNote,
            Status = item.Status?.Name ?? "Pending",
            SentAt = item.SentAt?.ToString("O"),
            ResendCount = item.ResendCount
        };
    }

    private async Task<List<int>> GetCategoryIdsForStationTypeAsync(int stationTypeId)
    {
        try
        {
            var sql = $"SELECT category_id FROM sales.station_type_category WHERE station_type_id = {stationTypeId}";
            var result = new List<int>();
            await using var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(reader.GetInt32(0));
            return result;
        }
        catch
        {
            return new List<int>();
        }
    }
}
