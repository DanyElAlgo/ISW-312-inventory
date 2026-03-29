using Sales.API.DTOs;
using Sales.API.HttpClients;
using Microsoft.EntityFrameworkCore;
using Sales.API.Models;

namespace Sales.API.Services;

public class DashboardService
{
    private readonly SalesDbContext _context;
    private readonly InventoryClient _inventoryClient;

    public DashboardService(SalesDbContext context, InventoryClient inventoryClient)
    {
        _context = context;
        _inventoryClient = inventoryClient;
    }

    // HU-23: ventas del día
    public async Task<SalesDashboardDto> GetSalesDashboardAsync()
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        var paidTicketIds = await _context.Payments
            .Where(p => p.PaidAt >= todayUtc && p.PaidAt < tomorrowUtc && p.OrderId.HasValue)
            .Select(p => p.OrderId!.Value)
            .Distinct()
            .ToListAsync();

        if (paidTicketIds.Count == 0)
            return new SalesDashboardDto();

        var tickets = await _context.OrderTickets
            .Include(t => t.OrderItems)
            .Where(t => paidTicketIds.Contains(t.Id))
            .ToListAsync();

        var ticketTotals = tickets.Select(t =>
        {
            var subtotal = t.OrderItems
                .Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
            return subtotal + subtotal * (t.TaxRateSnapshot ?? 0);
        }).ToList();

        var totalSold = ticketTotals.Sum();
        var count = ticketTotals.Count;

        return new SalesDashboardDto
        {
            TotalSoldToday = totalSold,
            PaidTicketsToday = count,
            AvgTicketToday = count > 0 ? totalSold / count : 0
        };
    }

    // HU-24: top productos
    public async Task<List<TopProductDto>> GetTopProductsAsync(int limit = 10)
    {
        var paidTicketIds = await _context.Payments
            .Where(p => p.OrderId.HasValue)
            .Select(p => p.OrderId!.Value)
            .Distinct()
            .ToListAsync();

        if (paidTicketIds.Count == 0)
            return new List<TopProductDto>();

        return await _context.OrderItems
            .Where(i => i.OrderId.HasValue && paidTicketIds.Contains(i.OrderId.Value) && i.ProductId.HasValue)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId ?? 0,
                ProductName = g.Key.ProductName ?? string.Empty,
                TotalQtySold = g.Sum(i => i.Qty ?? 0),
                TotalRevenue = g.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0))
            })
            .OrderByDescending(p => p.TotalQtySold)
            .Take(limit)
            .ToListAsync();
    }

    // HU-25: alertas de stock (via Inventory.API)
    public async Task<StockAlertsDashboardDto> GetStockAlertsDashboardAsync()
    {
        var result = new StockAlertsDashboardDto();

        var lowStockItems = await _inventoryClient.GetLowStockItemsAsync();
        if (lowStockItems == null)
            return result;

        foreach (var item in lowStockItems)
        {
            var alert = new StockAlertDto
            {
                ProductId = item.ProductId ?? 0,
                ProductName = item.ProductName ?? string.Empty,
                WarehouseName = item.WarehouseName ?? string.Empty,
                StockLeft = item.StockLeft ?? 0,
                LowStockQty = item.LowStockQty ?? 0,
                IsOutOfStock = item.StatusName?.ToLower() is "out of stock" or "agotado" || item.StockLeft == 0
            };

            if (alert.IsOutOfStock)
                result.OutOfStock.Add(alert);
            else
                result.LowStock.Add(alert);
        }

        return result;
    }

    // HU-26: estado de comandas KDS
    public async Task<KdsStatusSummaryDto> GetKdsStatusSummaryAsync()
    {
        var items = await _context.CommandItems
            .Include(ci => ci.OrderItem)
                .ThenInclude(oi => oi!.Status)
            .Where(ci => ci.OrderItem != null)
            .ToListAsync();

        var summary = new KdsStatusSummaryDto();
        foreach (var ci in items)
        {
            var statusName = ci.OrderItem?.Status?.Name?.ToLower() ?? "pending";
            if (statusName is "pending" or "pendiente")
                summary.PendingCount++;
            else if (statusName is "en preparacion" or "en preparación" or "in preparation")
                summary.InPreparationCount++;
            else if (statusName is "listo" or "ready")
                summary.ReadyCount++;
        }

        return summary;
    }
}
