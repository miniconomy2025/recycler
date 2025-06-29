using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Recycler.API;

[ApiController]
[Route("[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            return Ok(new List<Order>());
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    [Route("{orderNumber}")]
    public async Task<IActionResult> GetOrderByOrderNumber(string orderNumber)
    {
        // get Supplier's Details compare against order number
        var query = new GetOrderByOrderNumberQuery(orderNumber);
        return Ok(await mediator.Send(query));
    }
    
    
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveQuote([FromBody] CreateOrderCommand request)
    {
        return Ok(await mediator.Send(request));
    }

}