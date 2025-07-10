using MediatR;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace RecyclerApi.Handlers
{
    public class ProcessConsumerDropOffCommandHandler : IRequestHandler<ProcessConsumerDropOffCommand, ConsumerLogisticsDropOffResponseDto>
    {
        private readonly IConfiguration _configuration;

        public ProcessConsumerDropOffCommandHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ConsumerLogisticsDropOffResponseDto> Handle(ProcessConsumerDropOffCommand request, CancellationToken cancellationToken)
        {
            if (request.Status.Equals("success", StringComparison.OrdinalIgnoreCase))
            {
                await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync(cancellationToken);

                var phoneLookupSql = "SELECT id FROM Phone WHERE model ILIKE @ModelName;";
                var phoneId = await connection.QueryFirstOrDefaultAsync<int?>(phoneLookupSql, new { ModelName = request.ModelName });

                if (phoneId.HasValue)
                {
                    var upsertPhoneInventorySql = @"
                        INSERT INTO PhoneInventory (phone_id, quantity)
                        VALUES (@PhoneId, @Quantity)
                        ON CONFLICT (phone_id) DO UPDATE
                        SET quantity = PhoneInventory.quantity + @Quantity;
                    ";
                    await connection.ExecuteAsync(upsertPhoneInventorySql, new { PhoneId = phoneId.Value, Quantity = request.Quantity });

                    return new ConsumerLogisticsDropOffResponseDto { Message = $"Successfully processed drop-off for {request.Quantity} x {request.ModelName}. Inventory updated." };
                }
                else
                {
                    Console.WriteLine($"Warning: Drop-off received for unknown phone model: {request.ModelName}. Quantity: {request.Quantity}");
                    return new ConsumerLogisticsDropOffResponseDto { Message = $"Drop-off processed, but phone model '{request.ModelName}' not recognized. Inventory not updated for this item." };
                }
            }
            else
            {
                Console.WriteLine($"Info: Drop-off event received with non-success status: {request.Status} for {request.ModelName}. Inventory not updated.");
                return new ConsumerLogisticsDropOffResponseDto { Message = $"Drop-off event received with status '{request.Status}'. Inventory not updated." };
            }
        }
    }
}