using MediatR;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Commands.CreatePickupRequest;
using Recycler.API.Models;
using Recycler.API.Services;
using Recycler.API.Utils;
using Microsoft.Extensions.Logging;

namespace Recycler.API.Controllers;

[ApiController]
[Route("machine-payment-confirmation")]
public class MachinePaymentConfirmationController(
    IMediator mediator,
    MakePaymentService paymentService,
    ILogger<MachinePaymentConfirmationController> logger
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ConfirmMachinePayment([FromBody] MachinePaymentConfirmationDto body)
    {
        logger.LogInformation("Received machine payment confirmation request with OrderId: {OrderId}", body.OrderId);

        if (string.IsNullOrEmpty(body.OrderId))
        {
            logger.LogWarning("Bad request: Missing orderNumber in payment confirmation");
            return BadRequest(new { message = "Missing orderNumber" });
        }

        try
        {
            logger.LogInformation("Building pickup command for OrderId: {OrderId}", body.OrderId);
            var recyclerCompany = "recycler";
            var thoHCompany = "thoh";

            var pickupCommand = new CreatePickupRequestCommand
            {
                originalExternalOrderId = body.OrderId,
                originCompany = thoHCompany,
                destinationCompany = recyclerCompany,
                items = new List<PickupItem>
                {
                    new PickupItem
                    {
                        itemName = "recycling_machine",
                        quantity = body.TotalWeight,
                    }
                }
            };

            var cancellationToken = HttpContext.RequestAborted;

            logger.LogInformation("Sending CreatePickupRequestCommand via mediator");
            var pickupResult = await RetryHelper.RetryAsync(
                () => mediator.Send(pickupCommand, cancellationToken),
                operationName: "Create logistics pickup request");

            if (!pickupResult.Success)
            {
                logger.LogError("Failed to create logistics pickup request: {Message}", pickupResult.Message);
                return StatusCode(500, new { message = $"Failed to create logistics pickup request: {pickupResult.Message}" });
            }

            logger.LogInformation("Logistics pickup created successfully: ID #{PickupRequestId}, Cost: {Cost}", pickupResult.PickupRequestId, pickupResult.Cost);
            Console.WriteLine($"Logistics pickup created: ID #{pickupResult.PickupRequestId}, Cost: {pickupResult.Cost}");

            if (!string.IsNullOrEmpty(pickupResult.BulkLogisticsBankAccount) && pickupResult.Cost > 0)
            {
                try
                {
                    logger.LogInformation("Initiating logistics payment to account {BankAccount} for cost {Cost}",
                        pickupResult.BulkLogisticsBankAccount, pickupResult.Cost);

                    var logisticsPaymentResult = await RetryHelper.RetryAsync(
                        () => paymentService.SendPaymentAsync(
                            toAccountNumber: pickupResult.BulkLogisticsBankAccount!,
                            amount: (decimal)pickupResult.Cost,
                            description: pickupResult.PickupRequestId.ToString() ?? "",
                            cancellationToken),
                        operationName: "Send logistics payment");

                    logger.LogInformation("Logistics payment completed successfully: Tx#{TransactionNumber}", logisticsPaymentResult.transaction_number);
                    Console.WriteLine($"Logistics payment made: Tx#{logisticsPaymentResult.transaction_number}");

                    return Ok(new
                    {
                        message = "Pickup request created and logistics paid",
                        pickupRequestId = pickupResult.PickupRequestId,
                        logisticsTx = logisticsPaymentResult.transaction_number
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Logistics payment failed for pickup request {PickupRequestId}", pickupResult.PickupRequestId);
                    Console.WriteLine($"Logistics payment failed: {ex.Message}");
                    return StatusCode(500, new { message = $"Pickup created, but logistics payment failed: {ex.Message}" });
                }
            }
            else
            {
                logger.LogWarning("Pickup request created, but missing logistics payment details for PickupRequestId: {PickupRequestId}", pickupResult.PickupRequestId);
                return Ok(new
                {
                    message = "Pickup request created, but missing logistics payment details",
                    pickupRequestId = pickupResult.PickupRequestId
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling machine payment confirmation for OrderId: {OrderId}", body.OrderId);
            return StatusCode(500, new { message = $"Error handling machine payment confirmation: {ex.Message}" });
        }
    }
}
