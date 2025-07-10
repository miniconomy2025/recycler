using MediatR;
using Recycler.API.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Recycler.API.Queries
{
    public class GetMachinesQueryHandler : IRequestHandler<GetMachinesQuery, List<ReceivedMachineDto>>
    {
        private readonly IConfiguration _configuration;

        public GetMachinesQueryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<ReceivedMachineDto>> Handle(GetMachinesQuery request, CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT
                    id,
                    machine_id as MachineId,
                    received_at as ReceivedAt,
                    status
                FROM Machines
                ORDER BY received_at DESC;";

            var receivedMachines = await connection.QueryAsync<ReceivedMachineDto>(sql);

            return receivedMachines.ToList();
        }
    }
}
