using MediatR;
using Recycler.API.Commands;
using Recycler.API.Models;
using Microsoft.Extensions.Configuration; 
using Npgsql; 
using Dapper; 
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Recycler.API.Services;

namespace Recycler.API
{
    public class ReceiveLogisticsItemsCommandHandler : IRequestHandler<ReceiveLogisticsItemsCommand, Unit>
    {
        private readonly IConfiguration _configuration;

        public ReceiveLogisticsItemsCommandHandler(IConfiguration configuration, IRecyclingService recyclingService)
        {
            _configuration = configuration;
        }

        public async Task<Unit> Handle(ReceiveLogisticsItemsCommand request, CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);

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

                }
                else
                {
                    Console.WriteLine($"Info: General logistics item received (not a phone): {itemToReceive.Name}, Quantity: {itemToReceive.Quantity}.");
                }
            }

            return Unit.Value;

        }    
                  
        }
    }

 