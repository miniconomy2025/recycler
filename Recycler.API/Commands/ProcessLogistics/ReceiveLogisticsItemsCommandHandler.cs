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

namespace RecyclerApi.Handlers
{
    public class ReceiveLogisticsItemsCommandHandler : IRequestHandler<ReceiveLogisticsItemsCommand, Unit>
    {
        private readonly IConfiguration _configuration;
        private readonly IRecyclingService _recyclingService;

        public ReceiveLogisticsItemsCommandHandler(IConfiguration configuration, IRecyclingService recyclingService)
        {
            _configuration = configuration;
            _recyclingService = recyclingService;
        }

        public async Task<Unit> Handle(ReceiveLogisticsItemsCommand request, CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);

            bool phonesReceived = false;

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

                }
                else
                {
                    Console.WriteLine($"Info: General logistics item received (not a phone): {itemToReceive.Name}, Quantity: {itemToReceive.Quantity}.");
                }
            }

            if (phonesReceived)

            {
                var recyclingResult =  _recyclingService.StartRecyclingAsync();
            }

            return Unit.Value;

        }    
                  
        }
    }

 