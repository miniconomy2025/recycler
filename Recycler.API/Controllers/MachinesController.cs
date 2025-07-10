using MediatR;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Commands;
using Recycler.API.Models;
using Recycler.API.Queries;
using Recycler.API.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Recycler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachinesController : ControllerBase
    {
        private readonly IMediator _mediator;
        ILogService _logService;

        public MachinesController(IMediator mediator, ILogService logService)
        {
            _mediator = mediator;
            _logService = logService;
        }
        [HttpPost("receive")]
        [ProducesResponseType(typeof(ReceivedMachineDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReceiveMachine([FromBody] ReceiveMachineCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                
                await _logService.CreateLog(HttpContext, command, StatusCode(StatusCodes.Status201Created, result));
                
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception ex)
            {
                await _logService.CreateLog(HttpContext, command, BadRequest(new { Message = ex.Message }));

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