namespace Sales.API.DTOs;

public class SalesDashboardDto
{
    public decimal TotalSoldToday { get; set; }
    public int PaidTicketsToday { get; set; }
    public decimal AvgTicketToday { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public double TotalQtySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class KdsStatusSummaryDto
{
    public int PendingCount { get; set; }
    public int InPreparationCount { get; set; }
    public int ReadyCount { get; set; }
}

public class StockAlertDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int StockLeft { get; set; }
    public int LowStockQty { get; set; }
    public bool IsOutOfStock { get; set; }
}

public class StockAlertsDashboardDto
{
    public List<StockAlertDto> OutOfStock { get; set; } = new();
    public List<StockAlertDto> LowStock { get; set; } = new();
}
