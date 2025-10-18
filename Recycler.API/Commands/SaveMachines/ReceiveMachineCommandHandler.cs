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

            // var existingMachineSql = "SELECT id, machine_id, received_at, is_operational FROM Machines WHERE machine_id = @MachineId;";
            // var existingMachine = await connection.QueryFirstOrDefaultAsync<ReceivedMachineDto>(existingMachineSql, new { MachineId = request.ModelName });

            // if (existingMachine != null)
            // {
            //     Console.WriteLine($"Warning: Machine with ThoH ID {request.ModelName} already exists in Machines table (Recycler ID: {existingMachine.Id}). Not adding duplicate.");
            //     return existingMachine;
            // }
            // else
            // {
            var insertSql = @"
                    INSERT INTO Machines (machine_id, received_at, is_operational)
                    VALUES (@MachineId, @ReceivedAt, @Status)
                    RETURNING id, machine_id, received_at, is_operational;";
            for (int i = 0; i <= request.Quantity; i++)
            {

                var newReceivedMachine = await connection.QuerySingleAsync<ReceivedMachineDto>(insertSql, new
                {
                    MachineId = 1,
                    ReceivedAt = DateTime.UtcNow,
                    Status = true
                });
            }

            return new ReceivedMachineDto
            {
                Id = 1,
                IsOperational = true,
                MachineId = 1,
                ReceivedAt = DateTime.UtcNow
            };
            // }
        }
    }
}
