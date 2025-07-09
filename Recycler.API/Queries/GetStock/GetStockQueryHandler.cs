using Dapper;
using MediatR;
using Npgsql;
using Recycler.API.Models;

namespace Recycler.API.Queries.GetRevenueReport;

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
            select r.name , m.available_quantity_in_kg as quantity, 'kg' as unit from materialinventory m
            join rawmaterial r on r.id = m.material_id;
        ";
        var materials = await connection.QueryAsync<StockItem>(sqlMaterials);

        var sqlPhones = @"
            select CONCAT(p3.brand_name, ' ', p2.model) as name, p.quantity, 'unit(s)' as unit from phoneinventory p
            join phone p2 on p.phone_id =p2.id
            join phonebrand p3 on p2.phone_brand_id =p3.id
        ";
        var phones = await connection.QueryAsync<StockItem>(sqlPhones);
        Console.WriteLine(phones);
        var result = new StockSet();
        result.Phones = (List<StockItem>)phones;
        result.RawMaterials = (List<StockItem>)materials;
        return result;
    }
}