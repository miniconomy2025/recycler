using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Recycler.API.Services;
using RecyclerApi.Commands;

namespace Recycler.API.Controllers;

[ApiController]
public class NotificationsController(
    AvailablePhonesNotificationService notifier,
    IMediator mediator,
    ILogService logService
) : ControllerBase
{
    [HttpPost]
    [Route("recycler/notify-me")]
    public async Task<IActionResult> NotifyAvailablePhones()
    {
        Console.WriteLine("Manual notify-me trigger received. Starting notify flow.");
        var result = await notifier.NotifyAsync(HttpContext);
        if (!result.Success)
        {
            Console.WriteLine("Notify flow completed: no phones available.");
            return BadRequest(new { message = result.Message });
        }
        Console.WriteLine("Notify flow completed successfully.");
        return Ok(new { message = result.Message, results = result.Items });
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