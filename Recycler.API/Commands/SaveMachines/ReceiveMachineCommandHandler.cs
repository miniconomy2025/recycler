using MediatR;
using Recycler.API.Commands;
using Recycler.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recycler.API
{
    public class ReceiveMachineCommandHandler : IRequestHandler<ReceiveMachineCommand, ReceivedMachineDto>
    {
        private readonly IConfiguration _configuration;

        public ReceiveMachineCommandHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ReceivedMachineDto> Handle(ReceiveMachineCommand request, CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);

            var insertSql = @"
                INSERT INTO Machines (machine_id, received_at, is_operational)
                VALUES (@MachineId, @ReceivedAt, @IsOperational)
                RETURNING id, machine_id, received_at, is_operational;";

            var newMachine = await connection.QuerySingleAsync<ReceivedMachineDto>(insertSql, new
            {
                MachineId = 4,
                ReceivedAt = DateTime.UtcNow,
                IsOperational = true
            });

            return newMachine;
        }
    }
}
