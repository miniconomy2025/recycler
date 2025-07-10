using MediatR;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Commands;
using Recycler.API.Models;
using Recycler.API.Queries;
using Recycler.API.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using Recycler.API.Services;
using System; 

namespace Recycler.API.Controllers
{
    [ApiController]
    
    [Route("api/[controller]")]
    public class LogisticsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogService _logService;

        public LogisticsController(IMediator mediator, ILogService logService)
        {
            _mediator = mediator;
            _logService = logService;
        }

        [HttpPost("/logistics")] 
        [ProducesResponseType(typeof(LogisticsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessGeneralLogisticsEvent([FromBody] LogisticsRequestDto requestDto)
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

       
        [HttpPost("consumer-deliveries")] 
        [ProducesResponseType(typeof(ConsumerLogisticsDeliveryResponseDto), StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessConsumerDeliveryNotification([FromBody] ConsumerLogisticsDeliveryNotificationRequestDto requestDto) 
        {
            try
            {
                if (string.IsNullOrEmpty(requestDto.Status))
                {
                    return BadRequest(new { Message = "Status field is required." });
                }
                if (string.IsNullOrEmpty(requestDto.ModelName) || requestDto.Quantity <= 0)
                {
                    return BadRequest(new { Message = "ModelName and Quantity (must be > 0) are required for delivered items." });
                }

                var command = new ProcessConsumerDeliveryNotificationCommand 
                {
                    Status = requestDto.Status,
                    ModelName = requestDto.ModelName,
                    Quantity = requestDto.Quantity
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An internal server error occurred while processing the consumer delivery notification.", Details = ex.Message });
            }
        }
    }
}
