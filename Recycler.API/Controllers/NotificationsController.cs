using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Recycler.API.Models.ExternalApiRequests;
using Recycler.API.Models.Thoh;
using Recycler.API.Services;
using RecyclerApi.Commands;

namespace Recycler.API.Controllers;

[ApiController]
public class NotificationsController(
    ThohService thohPhoneService,
    ConsumerLogisticsService consumerLogisticsService,
    ILogService logService,
    MakePaymentService paymentService,
    IMediator mediator
) : ControllerBase
{
    [HttpPost]
    [Route("recycler/notify-me")]
    public async Task<IActionResult> NotifyAvailablePhones()
    {
        Console.WriteLine("THoH has notified Recycler about available phones.");

        var phones = await thohPhoneService.GetAvailableRecycledPhonesAsync();
        if (phones is null || phones.Count == 0)
        {
            return BadRequest(new { message = "No recycled phones received from THoH." });
        }

        var results = new List<object>();

        foreach (var phone in phones)
        {
            var deliveryOrder = new DeliveryOrderRequestDto
            {
                modelName = phone.ModelName,
                quantity = phone.Quantity
            };

            try
            {
                var response = await consumerLogisticsService.SendDeliveryOrderAsync(deliveryOrder);
                var responseBodyText = await response.Content.ReadAsStringAsync();
                var deliveryResponse = JsonSerializer.Deserialize<DeliveryOrderResponseDto>(responseBodyText);

                if (deliveryResponse == null)
                {
                    throw new Exception("Failed to parse delivery order response.");
                }

                var paymentResult = await paymentService.SendPaymentAsync(
                    toAccountNumber: deliveryResponse.accountNumber,
                    amount: decimal.Parse(deliveryResponse.amount),
                    description: deliveryResponse.referenceNo
                );
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    phone.ModelName,
                    Status = "Failed",
                    Error = ex.Message
                });
            }
        }

        results.Add(new
        {
            Status = "Success",
        });

        await logService.CreateLog(HttpContext, "", Ok(new
        {
            message = "Completed delivery orders for all phones.",
            results
        }));

        return Ok(new
        {
            message = "Completed delivery orders for all phones.",
            results
        });
    }

    [HttpPost]
    [Route("/machine-failure")]
    public async Task<IActionResult> GetNotificationOfMachineFailure([FromBody] GetNotificationOfMachineFailureCommand command)
    {
        await mediator.Send(command);

        await logService.CreateLog(HttpContext, command, Ok());

        return Ok();
    }
}