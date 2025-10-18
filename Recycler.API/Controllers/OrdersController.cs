using System.Runtime.InteropServices.JavaScript;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Models;
using Recycler.API.Services;

namespace Recycler.API;

[ApiController]
[Route("[controller]")]
public class OrdersController(IMediator mediator, ILogService logService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GenericResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById([FromQuery] int id)
    {
        try
        {
            var query = new GetOrderByIdQuery(id);
            
            var response =  Ok(await mediator.Send(query));
            
            await logService.CreateLog(HttpContext, id, response);

            return response;
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(GenericResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    [Route("{orderNumber}")]
    public async Task<IActionResult> GetOrderByOrderNumber(Guid orderNumber)
    {
        try
        {
            var query = new GetOrderByOrderNumberQuery(orderNumber);
        
            var response =  Ok(await mediator.Send(query));
            
            await logService.CreateLog(HttpContext, orderNumber, response);
            
            return response;
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    
    [HttpPost]
    [ProducesResponseType(typeof(GenericResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand request)
    {
        try
        {
            var response = Ok(await mediator.Send(request));

            await logService.CreateLog(HttpContext, request, response);

            return response;
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}