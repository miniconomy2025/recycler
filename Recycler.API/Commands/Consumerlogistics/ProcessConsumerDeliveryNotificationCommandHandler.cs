using MediatR;
using Recycler.API.Commands;
using Recycler.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Dapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Recycler.API
{
    public class ProcessConsumerDeliveryNotificationCommandHandler 
        : IRequestHandler<ProcessConsumerDeliveryNotificationCommand, ConsumerLogisticsDeliveryResponseDto>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProcessConsumerDeliveryNotificationCommandHandler> _logger;

        public ProcessConsumerDeliveryNotificationCommandHandler(
            IConfiguration configuration,
            ILogger<ProcessConsumerDeliveryNotificationCommandHandler> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ConsumerLogisticsDeliveryResponseDto> Handle(
            ProcessConsumerDeliveryNotificationCommand request,
            CancellationToken cancellationToken)
        {
            var response = new ConsumerLogisticsDeliveryResponseDto();

            _logger.LogInformation("Received delivery notification for model '{ModelName}' with status '{Status}' and quantity {Quantity}",
                request.ModelName, request.Status, request.Quantity);

            if (!request.Status.Equals("delivered", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Delivery status for model '{ModelName}' was '{Status}', skipping processing",
                    request.ModelName, request.Status);

                response.Message = "Delivery not processed";
                return response;
            }

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                _logger.LogInformation("Connected to database successfully");

                var phoneLookupSql = "SELECT id FROM Phone WHERE model LIKE @ModelName;";
                var phoneId = await connection.QueryFirstOrDefaultAsync<int?>(phoneLookupSql, new { ModelName = request.ModelName });

                if (!phoneId.HasValue)
                {
                    _logger.LogWarning("Phone model '{ModelName}' not found in database", request.ModelName);
                    response.Message = $"Phone model '{request.ModelName}' not found";
                    return response;
                }

                var upsertSql = @"
                    INSERT INTO PhoneInventory (phone_id, quantity)
                    VALUES (@PhoneId, @Quantity)
                    ON CONFLICT (phone_id) DO UPDATE
                    SET quantity = PhoneInventory.quantity + @Quantity;
                ";

                _logger.LogInformation("Updating inventory for Phone ID {PhoneId} with additional quantity {Quantity}", 
                    phoneId.Value, request.Quantity);

                await connection.ExecuteAsync(upsertSql, new { PhoneId = phoneId.Value, Quantity = request.Quantity });

                _logger.LogInformation("Inventory update completed for model '{ModelName}' (Phone ID {PhoneId})", 
                    request.ModelName, phoneId.Value);

                response.Message = "Phones received";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing delivery notification for model '{ModelName}'", request.ModelName);
                response.Message = "An error occurred while processing the delivery";
                return response;
            }
        }
    }
}
