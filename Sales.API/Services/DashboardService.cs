using Sales.API.DTOs;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class DashboardService
{
    private readonly IPaymentRepository _payments;
    private readonly IOrderTicketRepository _tickets;
    private readonly IOrderItemRepository _orderItems;
    private readonly ICommandItemRepository _commandItems;

    public DashboardService(
        IPaymentRepository payments,
        IOrderTicketRepository tickets,
        IOrderItemRepository orderItems,
        ICommandItemRepository commandItems)
    {
        _payments = payments;
        _tickets = tickets;
        _orderItems = orderItems;
        _commandItems = commandItems;
    }

    public async Task<DailySalesDashboardDto> GetDailySalesDashboardAsync()
    {
        var todayUtc = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);
        var tomorrowUtc = todayUtc.AddDays(1);

        var paidTicketIds = await _payments.GetPaidOrderIdsAsync(todayUtc, tomorrowUtc);
        if (paidTicketIds.Count == 0)
            return new DailySalesDashboardDto();

        var totals = new List<decimal>();
        foreach (var id in paidTicketIds)
        {
            var ticket = await _tickets.GetByIdAsync(id, includeItems: true);
            if (ticket == null)
                continue;

            var sub = ticket.OrderItems.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
            totals.Add(sub + sub * (ticket.TaxRateSnapshot ?? 0));
        }

        var totalSales = totals.Sum();
        var count = totals.Count;

        return new DailySalesDashboardDto
        {
            TotalSales = totalSales,
            TicketsCount = count,
            AverageTicket = count > 0 ? totalSales / count : 0
        };
    }

    public async Task<IReadOnlyList<TopProductDashboardContractResponse>> GetTopProductsDashboardAsync(int topN = 10)
    {
        var paidTicketIds = await _payments.GetPaidOrderIdsAsync();
        if (paidTicketIds.Count == 0)
            return Array.Empty<TopProductDashboardContractResponse>();

        var lines = new List<(string? Cen, string Name, decimal Price, int Qty)>();
        foreach (var id in paidTicketIds)
        {
            var items = await _orderItems.GetByOrderIdAsync(id);
            foreach (var i in items)
            {
                if (string.IsNullOrWhiteSpace(i.ProductCen))
                    continue;
                lines.Add((i.ProductCen, i.ProductName ?? string.Empty, i.UnitPrice ?? 0, (int)(i.Qty ?? 0)));
            }
        }

        return lines
            .GroupBy(l => new { l.Cen, l.Name, l.Price })
            .Select(g => new TopProductDashboardContractResponse
            {
                ProductCen = g.Key.Cen,
                ProductName = g.Key.Name,
                TotalQuantity = g.Sum(l => l.Qty),
                SalePrice = g.Key.Price
            })
            .OrderByDescending(p => p.TotalQuantity)
            .Take(topN)
            .ToList();
    }

    public async Task<KdsStatusDashboardDto> GetKdsStatusDashboardAsync()
    {
        var items = await _commandItems.GetAllWithOrderItemStatusAsync();

        var dto = new KdsStatusDashboardDto();
        foreach (var ci in items)
        {
            var statusName = ci.OrderItem?.Status?.Name?.ToLower() ?? "pending";
            if (statusName is "pending" or "pendiente")
                dto.PendingCount++;
            else if (statusName is "en preparacion" or "en preparación" or "in preparation")
                dto.PreparingCount++;
            else if (statusName is "listo" or "ready")
                dto.ReadyCount++;
        }
        return dto;
    }
}
