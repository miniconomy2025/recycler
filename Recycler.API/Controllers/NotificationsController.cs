using Microsoft.AspNetCore.Mvc;
using Recycler.API.Models.ExternalApiRequests;
using Recycler.API.Services;

namespace Recycler.API.Controllers;

[ApiController]
[Route("recycler/notifyme")]
public class PhonesNotificationsController(
    ThohService thohPhoneService,
    ConsumerLogisticsService consumerLogisticsService,
    ILogService logService
) : ControllerBase
{
    [HttpPost]
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
                var responseBody = await response.Content.ReadAsStringAsync();

                results.Add(new
                {
                    phone.ModelName,
                    Status = "Success",
                    Response = responseBody
                });
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
}