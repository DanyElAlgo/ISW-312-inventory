namespace Sales.API.Helpers;

public static class SalesCenBuilder
{
    public static string BuildTicketCen(int id) => $"TKT-{id:D6}";
    public static string BuildItemCen(int id) => $"ITEM-{id:D6}";
}
