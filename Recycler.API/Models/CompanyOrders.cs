namespace Recycler.API.Queries.GetRevenueReport;

public class RevenueReportDto
{
    public required string CompanyName { get; set; }
    public required string OrderNumber { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public required List<RevenueOrderItemDto> Items { get; set; }
    public decimal CompanyTotalOrders { get; set; }
}

public class RevenueOrderItemDto
{
    public required string MaterialName { get; set; }
    public int QuantityKg { get; set; }
    public decimal? TotalPrice { get; set; }
}
