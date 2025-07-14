using Dapper;
using MediatR;
using Npgsql;
using Recycler.API.Models;

namespace Recycler.API.Queries;

public class GetStockQueryHandler : IRequestHandler<GetStockQuery, StockSet>
{
    private readonly IConfiguration _configuration;

    public GetStockQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<StockSet> Handle(GetStockQuery request, CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        using var connection = new NpgsqlConnection(connectionString);

        var sqlMaterials = @"
            SELECT r.name, m.available_quantity_in_kg AS quantity, 'kg' AS unit
            FROM materialinventory m
            JOIN rawmaterial r ON r.id = m.material_id;
        ";
        var materials = await connection.QueryAsync<StockItem>(sqlMaterials);

        var sqlPhones = @"
            SELECT CONCAT(p3.brand_name, ' ', p2.model) AS name, p.quantity, 'unit(s)' AS unit
            FROM phoneinventory p
            JOIN phone p2 ON p.phone_id = p2.id
            JOIN phonebrand p3 ON p2.phone_brand_id = p3.id;
        ";
        var phones = await connection.QueryAsync<StockItem>(sqlPhones);

        var sqlOrders = @"
            SELECT 
                COUNT(*) AS Total,
                COUNT(*) FILTER (
                    WHERE os.name ILIKE 'Delivered' OR os.name ILIKE 'Shipped'
                ) AS Completed,
                COUNT(*) FILTER (
                    WHERE os.name ILIKE 'Pending'
                ) AS Pending
            FROM Orders o
            JOIN OrderStatus os ON o.order_status_id = os.id;
        ";

        var orderCounts = await connection.QuerySingleAsync<OrderCounts>(sqlOrders);
        Console.WriteLine($"Total: {orderCounts.Total}, Completed: {orderCounts.Completed}, Pending: {orderCounts.Pending}");

        return new StockSet
        {
            RawMaterials = materials.ToList(),
            Phones = phones.ToList(),
            TotalOrders = orderCounts.Total,
            CompletedOrders = orderCounts.Completed,
            PendingOrders = orderCounts.Pending
        };
    }
}

public class OrderCounts
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
}
