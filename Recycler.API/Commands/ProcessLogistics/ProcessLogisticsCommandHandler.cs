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
using System.Text;

namespace Recycler.API
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
            var messageBuilder = new StringBuilder();

            messageBuilder.Append($"Logistics event '{request.Type}' with ID '{request.Id}' processed successfully.");

            try
            {
                if (request.Type == "DELIVERY")
                {
                    if (request.Items != null && request.Items.Any())
                    {
                        foreach (var item in request.Items)
                        {
                            if (string.IsNullOrWhiteSpace(item.Name) || item.Quantity <= 0)
                            {
                                messageBuilder.Append($" Warning: Invalid item entry: '{item.Name}' with quantity {item.Quantity}.");
                                continue;
                            }

                            var receiveMachineCommand = new ReceiveMachineCommand
                            {
                                ModelName = item.Name,
                                Quantity = item.Quantity
                            };

                            await _mediator.Send(receiveMachineCommand, cancellationToken);

                            messageBuilder.Append($" Machine received: '{item.Name}', Quantity: {item.Quantity}. ");
                        }
                    }
                    else
                    {
                        messageBuilder.Append(" No items specified for delivery.");
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
                            var materialLookupSql = "SELECT id FROM RawMaterial WHERE name ILIKE @ItemName;";
                            var materialId = await connection.QueryFirstOrDefaultAsync<int?>(materialLookupSql, new { ItemName = itemToPickup.Name });

                            if (!materialId.HasValue)
                            {
                                Console.WriteLine($"Error: Material '{itemToPickup.Name}' not found for pickup. No quantity removed.");
                                messageBuilder.Append($" Warning: Material '{itemToPickup.Name}' not found.");
                                continue;
                            }

                            var checkInventorySql = "SELECT reserved_quantity_in_kg FROM MaterialInventory WHERE material_id = @MaterialId;";
                            var currentAvailableQuantity = await connection.QueryFirstOrDefaultAsync<double?>(checkInventorySql, new { MaterialId = materialId.Value });

                            if (!currentAvailableQuantity.HasValue)
                            {
                                Console.WriteLine($"Warning: Material '{itemToPickup.Name}' found but no inventory record exists for pickup. No quantity removed.");
                                messageBuilder.Append($" Warning: No inventory record for '{itemToPickup.Name}'.");
                                continue;
                            }

                            if (itemToPickup.Quantity <= 0)
                            {
                                Console.WriteLine($"Warning: Invalid pickup quantity for '{itemToPickup.Name}'. Quantity must be positive.");
                                messageBuilder.Append($" Warning: Invalid quantity for '{itemToPickup.Name}'.");
                                continue;
                            }

                            if (currentAvailableQuantity.Value < itemToPickup.Quantity)
                            {
                                Console.WriteLine($"Warning: Insufficient quantity for pickup of '{itemToPickup.Name}'. Requested: {itemToPickup.Quantity}kg, Available: {currentAvailableQuantity.Value}kg. No quantity removed for this item.");
                                messageBuilder.Append($" Warning: Insufficient quantity for '{itemToPickup.Name}'.");
                                continue;
                            }

                            var updateInventorySql = @"
                                UPDATE MaterialInventory
                                SET reserved_quantity_in_kg = reserved_quantity_in_kg - @Quantity
                                WHERE material_id = @MaterialId;";
                            await connection.ExecuteAsync(updateInventorySql, new { MaterialId = materialId.Value, Quantity = itemToPickup.Quantity });

                            Console.WriteLine($"Info: Inventory updated for pickup: {itemToPickup.Quantity} x {itemToPickup.Name} removed.");
                            messageBuilder.Append($" Removed {itemToPickup.Quantity}kg of '{itemToPickup.Name}'.");
                        }
                    }
                    else
                    {
                        messageBuilder.Append(" No items specified for pickup.");
                    }
                }
            }
            catch (Exception ex)
            {
                messageBuilder.Append($" Error processing logistics event: {ex.Message}");
            }

            var response = new LogisticsResponseDto
            {
                Message = messageBuilder.ToString().Trim(),
                LogisticsRecordId = newInternalRecordId.ToString()
            };

            return response;
        }
    }
}
