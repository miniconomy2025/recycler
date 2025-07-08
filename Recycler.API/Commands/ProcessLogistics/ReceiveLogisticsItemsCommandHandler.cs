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

namespace RecyclerApi.Handlers
{
    public class ReceiveLogisticsItemsCommandHandler : IRequestHandler<ReceiveLogisticsItemsCommand, Unit>
    {
        private readonly IConfiguration _configuration;

        public ReceiveLogisticsItemsCommandHandler(IConfiguration configuration)
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
                    Console.WriteLine($"Info: General logistics item received (not a phone): {itemToReceive.Name}, Quantity: {itemToReceive.Quantity}. Update logic for general inventory would go here.");
                }
            }

            return Unit.Value;
        }
    }
}
