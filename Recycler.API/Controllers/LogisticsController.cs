using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using System.Threading.Tasks;
using Recycler.API.Services;

namespace RecyclerApi.Controllers
{
    [ApiController]
    [Route("logistics")] 
    public class LogisticsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogService _logService;

        public LogisticsController(IMediator mediator, ILogService logService)
        {
            _mediator = mediator;
            _logService = logService;
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
                
                await _logService.CreateLog(HttpContext, requestDto, Ok(result));
                
                return Ok(result); 
            }
            catch (Exception ex)
            {
                await _logService.CreateLog(HttpContext, requestDto, StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An internal server error occurred while processing the logistics event.", Details = ex.Message }));
              
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An internal server error occurred while processing the logistics event.", Details = ex.Message });
            }
        }
    }
}
