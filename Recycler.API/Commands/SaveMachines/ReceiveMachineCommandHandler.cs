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

namespace Recycler.API.Handlers
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

            var existingMachineSql = "SELECT id, machine_id, received_at, status FROM Machines WHERE machine_id = @MachineId;";
            var existingMachine = await connection.QueryFirstOrDefaultAsync<ReceivedMachineDto>(existingMachineSql, new { MachineId = request.MachineId });

            if (existingMachine != null)
            {
                Console.WriteLine($"Warning: Machine with ThoH ID {request.MachineId} already exists in Machines table (Recycler ID: {existingMachine.Id}). Not adding duplicate.");
                return existingMachine;
            }
            else
            {
                var insertSql = @"
                    INSERT INTO Machines (machine_id, received_at, status)
                    VALUES (@MachineId, @ReceivedAt, @Status)
                    RETURNING id, machine_id, received_at, status;";

                var newReceivedMachine = await connection.QuerySingleAsync<ReceivedMachineDto>(insertSql, new
                {
                    MachineId = request.MachineId,
                    ReceivedAt = DateTime.UtcNow,
                    Status = "Received"
                });

                return newReceivedMachine;
            }
        }
    }
}
