using MediatR;
using Recycler.API.Commands;
using Recycler.API.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Recycler.API.Handlers
{
    public class ProcessConsumerDeliveryNotificationCommandHandler : IRequestHandler<ProcessConsumerDeliveryNotificationCommand, ConsumerLogisticsDeliveryResponseDto> 
    {
        private readonly IConfiguration _configuration;

        public ProcessConsumerDeliveryNotificationCommandHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ConsumerLogisticsDeliveryResponseDto> Handle(ProcessConsumerDeliveryNotificationCommand request, CancellationToken cancellationToken)
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

                   
                    return new ConsumerLogisticsDeliveryResponseDto
                    {
                        ReferenceNo = Guid.NewGuid(), 
                        Amount = 0.00M, 
                        AccountNumber = "N/A" 
                    };
                }
                else
                {
                    Console.WriteLine($"Warning: Consumer delivery notification received for unknown phone model: {request.ModelName}. Quantity: {request.Quantity}");
                  
                    return new ConsumerLogisticsDeliveryResponseDto
                    {
                        ReferenceNo = Guid.NewGuid(),
                        Amount = 0.00M,
                        AccountNumber = "N/A"
                    };
                }
            }
            else
            {
                Console.WriteLine($"Info: Consumer delivery notification received with non-success status: {request.Status} for {request.ModelName}. Inventory not updated.");
                
                return new ConsumerLogisticsDeliveryResponseDto
                {
                    ReferenceNo = Guid.NewGuid(),
                    Amount = 0.00M,
                    AccountNumber = "N/A"
                };
            }
        }
    }
}
