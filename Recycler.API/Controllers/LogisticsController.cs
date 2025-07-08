using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using System.Threading.Tasks;

namespace RecyclerApi.Controllers
{
    [ApiController]
    [Route("logistics")] 
    public class LogisticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogisticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost] 
        [ProducesResponseType(typeof(LogisticsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessLogisticsEvent([FromBody] LogisticsRequestDto requestDto)
        {
            try
            {
                if (requestDto.Type != "PICKUP" && requestDto.Type != "DELIVERY")
                {
                    return BadRequest(new { Message = "Invalid 'type' specified. Must be 'PICKUP' or 'DELIVERY'." });
                }

                var command = new ProcessLogisticsCommand
                {
                    Id = requestDto.Id,
                    Type = requestDto.Type,
                    Items = requestDto.Items
                };

                var result = await _mediator.Send(command);
                return Ok(result); 
            }
            catch (Exception ex)
            {
              
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An internal server error occurred while processing the logistics event.", Details = ex.Message });
            }
        }
    }
}