using MediatR;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Commands.CreatePickupRequest;
using Recycler.API.Models;
using Recycler.API.Services;
using Recycler.API.Utils;

namespace Recycler.API.Controllers;

[ApiController]
[Route("machine-payment-confirmation")]
public class MachinePaymentConfirmationController(
    IMediator mediator,
    MakePaymentService paymentService
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ConfirmMachinePayment([FromBody] MachinePaymentConfirmationDto body)
    {
        if (string.IsNullOrEmpty(body.OrderId))
        {
            return BadRequest(new { message = "Missing orderNumber" });
        }

        try
        {
            var recyclerCompany = "recycler";
            var thoHCompany = "thoh";

            var pickupCommand = new CreatePickupRequestCommand
            {
                originalExternalOrder = body.OrderId,
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

            var pickupResult = await RetryHelper.RetryAsync(
                () => mediator.Send(pickupCommand, cancellationToken),
                operationName: "Create logistics pickup request");

            if (!pickupResult.Success)
            {
                return StatusCode(500, new { message = $"Failed to create logistics pickup request: {pickupResult.Message}" });
            }

            Console.WriteLine($"Logistics pickup created: ID #{pickupResult.PickupRequestId}, Cost: {pickupResult.Cost}");

            if (!string.IsNullOrEmpty(pickupResult.BulkLogisticsBankAccount) && pickupResult.Cost > 0)
            {
                try
                {
                    var logisticsPaymentResult = await RetryHelper.RetryAsync(
                        () => paymentService.SendPaymentAsync(
                            toAccountNumber: pickupResult.BulkLogisticsBankAccount!,
                            amount: (decimal)pickupResult.Cost,
                            description: pickupResult.PickupRequestId.ToString() ?? "",
                            cancellationToken),
                        operationName: "Send logistics payment");

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
                    Console.WriteLine($"Logistics payment failed: {ex.Message}");
                    return StatusCode(500, new { message = $"Pickup created, but logistics payment failed: {ex.Message}" });
                }
            }
            else
            {
                return Ok(new
                {
                    message = "Pickup request created, but missing logistics payment details",
                    pickupRequestId = pickupResult.PickupRequestId
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error handling machine payment confirmation: {ex.Message}" });
        }
    }
}
