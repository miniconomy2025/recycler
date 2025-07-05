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

        /// <summary>
        /// Place a machine order with the ThoH system.
        /// </summary>
        /// <param name="command">The command containing only the Machine ID.</param>
        /// <returns>A confirmation message for the machine order.</returns>
        [HttpPost("orders")] // Endpoint for placing machine orders
        [ProducesResponseType(typeof(MachineOrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PlaceMachineOrder([FromBody] PlaceMachineOrderCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (HttpRequestException ex)
            {
                // Catch HTTP client specific errors (e.g., ThoH API returns non-success status)
                return BadRequest(new { Message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                // Catch errors related to communication setup or other internal issues
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Registers a received machine into the Recycler's inventory.
        /// </summary>
        /// <param name="command">The command containing the ID of the received machine.</param>
        /// <returns>Details of the newly registered received machine.</returns>
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