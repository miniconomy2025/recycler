using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using RecyclerApi.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; 

namespace RecyclerApi.Controllers
{
    [ApiController]
    
    [Route("api/[controller]")]
    public class LogisticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogisticsController(IMediator mediator)
        {
            _mediator = mediator;
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
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An internal server error occurred while processing the general logistics event.", Details = ex.Message });
            }
        }

        
        [HttpGet("/logistics")] 
        [ProducesResponseType(typeof(List<LogisticsRecordDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGeneralLogisticsRecords()
        {
            var query = new GetLogisticsRecordsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        
        [HttpPost("deliveries")] 
        [ProducesResponseType(typeof(ConsumerLogisticsDeliveryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitiateConsumerDelivery([FromBody] InitiateConsumerDeliveryCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while initiating consumer delivery.", Details = ex.Message });
            }
        }

        
        [HttpPost("drop-offs")]
        [ProducesResponseType(typeof(ConsumerLogisticsDropOffResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessConsumerDropOff([FromBody] ConsumerLogisticsDropOffRequestDto requestDto)
        {
            try
            {
                if (string.IsNullOrEmpty(requestDto.Status))
                {
                    return BadRequest(new { Message = "Status field is required." });
                }
                if (string.IsNullOrEmpty(requestDto.ModelName) || requestDto.Quantity <= 0)
                {
                    return BadRequest(new { Message = "ModelName and Quantity (must be > 0) are required for drop-off items." });
                }

                var command = new ProcessConsumerDropOffCommand
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An internal server error occurred while processing the drop-off.", Details = ex.Message });
            }
        }
    }
}

