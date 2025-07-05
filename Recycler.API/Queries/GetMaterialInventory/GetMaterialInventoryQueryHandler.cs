using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Npgsql;

namespace Recycler.API.Queries.GetMaterialInventory
{
    public class GetMaterialInventoryQueryHandler : IRequestHandler<GetMaterialInventoryQuery, Dictionary<string, int>>
    {
        private readonly IConfiguration _configuration;

        public GetMaterialInventoryQueryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Dictionary<string, int>> Handle(GetMaterialInventoryQuery request, CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            
            var sql = @"
                SELECT rm.name as MaterialName, COALESCE(mi.available_quantity_in_kg, 0) as Quantity
                FROM rawmaterial rm
                LEFT JOIN materialinventory mi ON rm.id = mi.material_id";

            var results = await connection.QueryAsync<dynamic>(sql);
            
            return results.ToDictionary(
                r => (string)r.materialname, 
                r => (int)r.quantity
            );
        }
    }
}