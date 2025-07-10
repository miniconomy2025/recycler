using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using Recycler.API.Services;

namespace RecyclerApi.Controllers
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
    }
}