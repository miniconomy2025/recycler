using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using System.Threading.Tasks;

namespace RecyclerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachinesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MachinesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("receive")] 
        [ProducesResponseType(typeof(ReceivedMachineDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReceiveMachine([FromBody] ReceiveMachineCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}