using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using RecyclerApi.Queries;
using System.Collections.Generic;
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

        [HttpGet]
        [ProducesResponseType(typeof(List<ReceivedMachineDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMachines()
        {
            var query = new GetMachinesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}