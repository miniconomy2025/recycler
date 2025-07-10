using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Recycler.API.Dto;
using Recycler.API.Services;

namespace Recycler.API.Queries.GetAvailablePhones
{
    public class GetAvailablePhonesQueryHandler : IRequestHandler<GetAvailablePhonesQuery, List<PhoneInventoryDto>>
    {
        private readonly IConfiguration _configuration;
        private readonly IRecyclingService _recyclingService;

        public GetAvailablePhonesQueryHandler(IConfiguration configuration, IRecyclingService recyclingService)
        {
            _configuration = configuration;
            _recyclingService = recyclingService;
        }

        public async Task<List<PhoneInventoryDto>> Handle(GetAvailablePhonesQuery request, CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT
                    pi.phone_id as PhoneId,
                    p.model as Model,
                    pb.brand_name as BrandName,
                    pi.quantity as AvailableQuantity
                FROM phoneinventory pi
                JOIN phone p ON pi.phone_id = p.id
                JOIN phonebrand pb ON p.phone_brand_id = pb.id
                WHERE pi.quantity > 0";

            var parameters = new DynamicParameters();

            if (request.PhoneBrandId.HasValue)
            {
                sql += " AND p.phone_brand_id = @PhoneBrandId";
                parameters.Add("PhoneBrandId", request.PhoneBrandId.Value);
            }

            if (!string.IsNullOrEmpty(request.ModelFilter))
            {
                sql += " AND p.model ILIKE @ModelFilter";
                parameters.Add("ModelFilter", $"%{request.ModelFilter}%");
            }

            var phoneInventories = await connection.QueryAsync<PhoneInventoryDto>(sql, parameters);

            var result = new List<PhoneInventoryDto>();
            foreach (var inventory in phoneInventories)
            {
                var estimate = await _recyclingService.EstimateRecyclingYieldAsync(inventory.PhoneId, inventory.AvailableQuantity);
                inventory.EstimatedYield = estimate;
                result.Add(inventory);
            }

            return result;
        }
    }
}
