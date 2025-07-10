using MediatR;
using Recycler.API.Commands;
using Recycler.API.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System;
using System.Text.Json;

namespace Recycler.API.ProcessLogistics
{
    public class ProcessLogisticsCommandHandler : IRequestHandler<ProcessLogisticsCommand, LogisticsResponseDto>
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public ProcessLogisticsCommandHandler(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        public async Task<LogisticsResponseDto> Handle(ProcessLogisticsCommand request, CancellationToken cancellationToken)
        {
            var newInternalRecordId = Guid.NewGuid();

            string message = $"Logistics event '{request.Type}' with ID '{request.Id}' processed successfully.";

            if (request.Type == "DELIVERY")
            {
                if (request.Items != null && request.Items.Any())
                {
                    var receiveCommand = new ReceiveLogisticsItemsCommand { ItemsToReceive = request.Items };
                    await _mediator.Send(receiveCommand, cancellationToken);

                    message += " Inventory updated for delivered items (including phones).";
                }
                else
                {
                    message += " No items specified for delivery.";
                }
            }
            else if (request.Type == "PICKUP")
            {
                if (request.Items != null && request.Items.Any())
                {
                    await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                    await connection.OpenAsync(cancellationToken);

                    foreach (var itemToPickup in request.Items) 
                    {
                     
                        var materialLookupSql = "SELECT id FROM Material WHERE name ILIKE @ItemName;";
                        var materialId = await connection.QueryFirstOrDefaultAsync<int?>(materialLookupSql, new { ItemName = itemToPickup.Name });

                        if (!materialId.HasValue)
                        {
                            Console.WriteLine($"Error: Material '{itemToPickup.Name}' not found for pickup. No quantity removed.");
                            message += $" Warning: Material '{itemToPickup.Name}' not found.";
                            continue; 
                        }

                        
                        var checkInventorySql = "SELECT reserved_quantity_in_kg FROM MaterialInventory WHERE material_id = @MaterialId;";
                        var currentAvailableQuantity = await connection.QueryFirstOrDefaultAsync<double?>(checkInventorySql, new { MaterialId = materialId.Value });

                        if (!currentAvailableQuantity.HasValue)
                        {
                            Console.WriteLine($"Warning: Material '{itemToPickup.Name}' found but no inventory record exists for pickup. No quantity removed.");
                            message += $" Warning: No inventory record for '{itemToPickup.Name}'.";
                            continue; 
                        }

                        
                        if (itemToPickup.Quantity <= 0)
                        {
                             Console.WriteLine($"Warning: Invalid pickup quantity for '{itemToPickup.Name}'. Quantity must be positive.");
                             message += $" Warning: Invalid quantity for '{itemToPickup.Name}'.";
                             continue;
                        }

                        if (currentAvailableQuantity.Value < itemToPickup.Quantity)
                        {
                            Console.WriteLine($"Warning: Insufficient quantity for pickup of '{itemToPickup.Name}'. Requested: {itemToPickup.Quantity}kg, Available: {currentAvailableQuantity.Value}kg. No quantity removed for this item.");
                            message += $" Warning: Insufficient quantity for '{itemToPickup.Name}'.";
                            continue; 
                        }

                        
                        var updateInventorySql = @"
                            UPDATE MaterialInventory
                            SET  reserved_quantity_in_kg =  reserved_quantity_in_kg - @Quantity
                            WHERE material_id = @MaterialId;";
                        await connection.ExecuteAsync(updateInventorySql, new { MaterialId = materialId.Value, Quantity = itemToPickup.Quantity });

                        Console.WriteLine($"Info: Inventory updated for pickup: {itemToPickup.Quantity} x {itemToPickup.Name} removed.");
                        message += $" Removed {itemToPickup.Quantity}kg of '{itemToPickup.Name}'.";
                    }
                    message = message.Trim(); 
                }
                else
                {
                    message += " No items specified for pickup.";
                }
            }

            var response = new LogisticsResponseDto
            {
                Message = message,
                LogisticsRecordId = newInternalRecordId.ToString()
            };

            return response;
        }
    }
}