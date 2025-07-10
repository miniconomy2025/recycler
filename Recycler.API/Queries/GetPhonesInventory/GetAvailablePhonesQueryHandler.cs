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
using Recycler.API.Queries;
using Recycler.API.Models;

namespace Recycler.API.Queries.GetPhonesInventory
{
    public class GetAvailablePhonesQueryHandler : IRequestHandler<GetPhonesInventoryQuery, List<PhoneInventoryItemDto>>
    {
        private readonly IConfiguration _configuration;
        private readonly IRecyclingService _recyclingService;

        public GetAvailablePhonesQueryHandler(IConfiguration configuration, IRecyclingService recyclingService)
        {
            _configuration = configuration;
            _recyclingService = recyclingService;
        }

        public async Task<List<PhoneInventoryItemDto>> Handle(GetPhonesInventoryQuery request, CancellationToken cancellationToken)
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

            if (!string.IsNullOrEmpty(request.PhoneBrandId))
            {
                sql += " AND p.phone_brand_id = @PhoneBrandId";
                parameters.Add("PhoneBrandId", request.PhoneBrandId);
            }


            var phoneInventories = await connection.QueryAsync<PhoneInventoryItemDto>(sql, parameters);

            var result = new List<PhoneInventoryItemDto>();
            foreach (var inventory in phoneInventories)
            {
                var estimate = await _recyclingService.EstimateRecyclingYieldAsync(inventory.PhoneId, inventory.AvailableQuantity);
                result.Add(inventory);
            }

            return result;
        }
    }
}

