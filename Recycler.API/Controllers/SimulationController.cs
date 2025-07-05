using MediatR;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Commands.StartSimulation;

namespace Recycler.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SimulationController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(StartSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Start([FromBody] StartSimulationCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }
}
