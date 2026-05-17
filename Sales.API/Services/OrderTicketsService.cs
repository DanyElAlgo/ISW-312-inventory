using System.Text;
using Sales.API.DTOs;
using Sales.API.Models;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class OrderTicketsService
{
    private readonly IOrderTicketRepository _tickets;
    private readonly IOrderItemRepository _items;
    private readonly IOrderCommandRepository _commands;
    private readonly IWaiterRepository _waiters;
    private readonly IGlobalTaxConfigRepository _taxConfig;
    private readonly OrderStatusesService _statuses;
    private readonly ISalesUnitOfWork _uow;

    public OrderTicketsService(
        IOrderTicketRepository tickets,
        IOrderItemRepository items,
        IOrderCommandRepository commands,
        IWaiterRepository waiters,
        IGlobalTaxConfigRepository taxConfig,
        OrderStatusesService statuses,
        ISalesUnitOfWork uow)
    {
        _tickets = tickets;
        _items = items;
        _commands = commands;
        _waiters = waiters;
        _taxConfig = taxConfig;
        _statuses = statuses;
        _uow = uow;
    }

    public async Task<IReadOnlyList<TicketContractResponse>> GetTicketsAsync(string companyCen)
    {
        var openStatusId = await _statuses.GetOpenStatusIdAsync();
        var tickets = await _tickets.GetByStatusAsync(openStatusId, includeItems: true, includeStatus: true);

        var result = new List<TicketContractResponse>();
        foreach (var t in tickets)
            result.Add(await MapToContractAsync(t, companyCen));

        return result;
    }

    public async Task<TicketContractResponse?> CreateTicketAsync(string companyCen, CreateTicketContractRequest request)
    {
        var openStatusId = await _statuses.GetOpenStatusIdAsync();
        var taxConfig = await _taxConfig.GetOrCreateAsync();

        var today = DateTime.UtcNow.Date;
        var dailyCount = await _tickets.CountByCreatedAtRangeAsync(today, today.AddDays(1));

        var ticket = _tickets.Add(new OrderTicket
        {
            StatusId = openStatusId,
            TaxRateSnapshot = taxConfig.TaxRate,
            CreatedAt = DateTime.UtcNow,
            DailyNumber = dailyCount + 1
        });
        await _uow.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.WaiterCen) && int.TryParse(request.WaiterCen, out var waiterId)
            && await _waiters.ExistsAsync(waiterId))
        {
            _commands.Add(new OrderCommand { OrderId = ticket.Id, WaiterId = waiterId });
            await _uow.SaveChangesAsync();
        }

        var created = await _tickets.GetByIdAsync(ticket.Id, includeItems: true, includeStatus: true);
        return created == null ? null : await MapToContractAsync(created, companyCen);
    }

    public async Task<AssignTicketWaiterContractResponse?> AssignWaiterAsync(string ticketCen, AssignTicketWaiterContractRequest request)
    {
        if (!int.TryParse(ticketCen, out var ticketId) || !int.TryParse(request.WaiterCen, out var waiterId))
            return null;

        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return null;

        var waiter = await _waiters.GetByIdAsync(waiterId);
        if (waiter == null)
            throw new InvalidOperationException("Waiter not found.");

        var latest = await _commands.GetLatestByOrderIdAsync(ticketId);
        if (latest == null)
            _commands.Add(new OrderCommand { OrderId = ticketId, WaiterId = waiterId });
        else
            latest.WaiterId = waiterId;

        await _uow.SaveChangesAsync();

        return new AssignTicketWaiterContractResponse
        {
            TicketCen = ticketId.ToString(),
            WaiterCen = request.WaiterCen,
            WaiterName = waiter.Name ?? string.Empty
        };
    }

    public async Task<CancelTicketContractResponse?> CancelTicketAsync(string ticketCen, string? reason)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            return null;

        var ticket = await _tickets.GetByIdAsync(ticketId, includeStatus: true);
        if (ticket == null)
            return null;

        var statusName = ticket.Status?.Name?.ToLower() ?? "";
        if (statusName is "paid" or "pagado")
            throw new InvalidOperationException("Cannot cancel a paid ticket.");

        if (statusName is "cancelled" or "cancelado")
            throw new InvalidOperationException("Ticket is already cancelled.");

        ticket.StatusId = await _statuses.GetCancelledStatusIdAsync();
        ticket.CancellationReason = reason;
        await _uow.SaveChangesAsync();

        var refreshed = await _tickets.GetByIdAsync(ticketId, includeStatus: true);
        return new CancelTicketContractResponse
        {
            TicketCen = ticketId.ToString(),
            Status = refreshed?.Status?.Name ?? "Cancelado"
        };
    }

    public async Task<TicketTotalsContractResponse?> GetTicketTotalsAsync(string ticketCen)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            return null;

        var ticket = await _tickets.GetByIdAsync(ticketId, includeItems: true);
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

    public async Task<byte[]> PrintTicketAsync(string ticketCen)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            throw new InvalidOperationException("Invalid ticketCen.");

        var ticket = await _tickets.GetByIdAsync(ticketId, includeItems: true, includeStatus: true);
        if (ticket == null)
            throw new InvalidOperationException("Ticket not found.");

        var waiter = await GetAssignedWaiterAsync(ticketId);
        var subtotal = ticket.OrderItems.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
        var taxRate = ticket.TaxRateSnapshot ?? 0;
        var taxAmount = subtotal * taxRate;
        var total = subtotal + taxAmount;

        var sb = new StringBuilder();
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

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<IReadOnlyList<TicketItemContractResponse>?> SendTicketToKitchenAsync(
        string ticketCen,
        Func<string, Task<int?>> resolveStationForProductAsync)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            return null;

        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return null;

        var waiter = await GetAssignedWaiterAsync(ticketId);
        if (waiter == null)
            throw new InvalidOperationException("A waiter must be assigned before sending the ticket to kitchen.");

        var unsentItems = await _items.GetUnsentByOrderIdAsync(ticketId, includeStatus: true);
        if (!unsentItems.Any())
            throw new InvalidOperationException("There are no new items to send.");

        var command = _commands.Add(new OrderCommand { OrderId = ticketId, WaiterId = waiter.Id });
        await _uow.SaveChangesAsync();

        var now = DateTime.UtcNow;
        foreach (var item in unsentItems)
        {
            if (string.IsNullOrWhiteSpace(item.ProductCen))
                continue;

            var stationId = await resolveStationForProductAsync(item.ProductCen);
            if (!stationId.HasValue)
                continue;

            item.SentAt = now;
        }

        await _uow.SaveChangesAsync();

        var allItems = await _items.GetByOrderIdAsync(ticketId, includeStatus: true);
        return allItems.Select(OrderItemMapping.MapToContract).ToList();
    }

    public async Task<Waiter?> GetAssignedWaiterAsync(int ticketId)
    {
        var latest = await _commands.GetLatestByOrderIdAsync(ticketId);
        if (latest?.WaiterId == null)
            return null;
        return await _waiters.GetByIdAsync(latest.WaiterId.Value);
    }

    private async Task<TicketContractResponse> MapToContractAsync(OrderTicket ticket, string companyCen)
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
}

internal static class OrderItemMapping
{
    public static TicketItemContractResponse MapToContract(OrderItem item) => new()
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
