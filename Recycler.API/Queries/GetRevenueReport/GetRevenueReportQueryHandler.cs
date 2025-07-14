using Dapper;
using MediatR;
using Npgsql;

namespace Recycler.API.Queries.GetRevenueReport;

public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, List<RevenueReportDto>>
{
    private readonly IConfiguration _configuration;

    public GetRevenueReportQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<RevenueReportDto>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using var connection = new NpgsqlConnection(connectionString);

        //TODO test when db is up
        var sql = @"
            SELECT
                c.name AS company_name,
                o.order_number,
                os.name AS status,
                o.created_at,
                rm.name AS material_name,
                oi.quantity_in_kg,
                oi.price_per_kg
            FROM Orders o
            JOIN Companies c ON o.company_id = c.id
            JOIN OrderStatus os ON o.order_status_id = os.id
            JOIN OrderItems oi ON oi.order_id = o.id
            JOIN RawMaterial rm ON oi.material_id = rm.id
            ORDER BY c.name, o.created_at;
        ";

        var result = await connection.QueryAsync<dynamic>(sql);

        var grouped = result
            .GroupBy(r => new { r.company_name, r.order_number, r.status, r.created_at })
            .Select(g =>
            {
                var items = g.Select(i => new RevenueOrderItemDto
                {
                    MaterialName = i.material_name,
                    QuantityKg = i.quantity_in_kg,
                    TotalPrice = i.quantity_in_kg * i.price_per_kg  
                }).ToList();

                var total = items.Sum(x => x.TotalPrice ?? 0m);

                return new RevenueReportDto
                {
                    CompanyName = g.Key.company_name,
                    OrderNumber = g.Key.order_number.ToString(),
                    Status = g.Key.status,
                    CreatedAt = g.Key.created_at,
                    Items = items,
                    CompanyTotalOrders = total
                };
            })
            .ToList();

        return grouped;

    }
}

