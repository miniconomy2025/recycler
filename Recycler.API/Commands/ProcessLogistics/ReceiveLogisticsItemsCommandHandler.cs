using MediatR;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using Microsoft.Extensions.Configuration; 
using Npgsql; 
using Dapper; 
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Recycler.API.Services;
using Recycler.API.Models;

namespace RecyclerApi.Handlers
{
    public class ReceiveLogisticsItemsCommandHandler : IRequestHandler<ReceiveLogisticsItemsCommand, LogisticsReceiptResult>
    {
        private readonly IConfiguration _configuration;
        private readonly IRecyclingService _recyclingService;

        public ReceiveLogisticsItemsCommandHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<LogisticsReceiptResult> Handle(ReceiveLogisticsItemsCommand request, CancellationToken cancellationToken)
        {

            var result = new LogisticsReceiptResult { Success = true };

            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);

            bool phonesReceived = false;

            try
            {

                foreach (var itemToReceive in request.ItemsToReceive)
                {
                    var phoneLookupSql = "SELECT id FROM Phone WHERE model ILIKE @ModelName;";
                    var phoneId = await connection.QueryFirstOrDefaultAsync<int?>(phoneLookupSql, new { ModelName = itemToReceive.Name });

                    if (phoneId.HasValue)
                    {
                        var upsertPhoneInventorySql = @"
                            INSERT INTO PhoneInventory (phone_id, quantity)
                            VALUES (@PhoneId, @Quantity)
                            ON CONFLICT (phone_id) DO UPDATE
                            SET quantity = PhoneInventory.quantity + @Quantity;
                        ";
                        await connection.ExecuteAsync(upsertPhoneInventorySql, new { PhoneId = phoneId.Value, Quantity = itemToReceive.Quantity });
                        phonesReceived = true;
                        result.PhonesReceived += itemToReceive.Quantity;
                        result.ReceivedPhoneModels.Add($"{itemToReceive.Quantity} {itemToReceive.Name}");
                    }

                    if (phonesReceived)
                    {
                        try
                        {
                            result.RecyclingResult = await _recyclingService.StartRecyclingAsync();
                            result.Message = $"Received {result.PhonesReceived} phones and completed recycling";
                        }
                        catch (Exception)
                        {
                            result.Message = $"Received {result.PhonesReceived} phones but recycling failed";

                        }
                    }
                    else
                    {
                        result.Message = "Items received but no phones found";
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error receiving logistics items: {ex.Message}";
                return result;

            }
              
        }
    }
}
