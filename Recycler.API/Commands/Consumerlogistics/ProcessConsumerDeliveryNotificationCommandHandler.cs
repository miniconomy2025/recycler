using MediatR;
using Recycler.API.Commands;
using Recycler.API.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recycler.API
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
            var response = new ConsumerLogisticsDeliveryResponseDto();

            if (!request.Status.Equals("delivered", StringComparison.OrdinalIgnoreCase))
            {
                response.Message = "Delivery not processed";
                return response;
            }

            try
            {
                await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync(cancellationToken);

                var phoneLookupSql = "SELECT id FROM Phone WHERE model LIKE @ModelName;";
                var phoneId = await connection.QueryFirstOrDefaultAsync<int?>(phoneLookupSql, new { ModelName = request.ModelName });

                if (!phoneId.HasValue)
                {
                    response.Message = $"Phone model '{request.ModelName}' not found";
                    return response;
                }

                var upsertSql = @"
                    INSERT INTO PhoneInventory (phone_id, quantity)
                    VALUES (@PhoneId, @Quantity)
                    ON CONFLICT (phone_id) DO UPDATE
                    SET quantity = PhoneInventory.quantity + @Quantity;
                ";

                await connection.ExecuteAsync(upsertSql, new { PhoneId = phoneId.Value, Quantity = request.Quantity });

                response.Message = "Phones received";
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                response.Message = "An error occurred while processing the delivery";
                return response;
            }
        }
    }
}
