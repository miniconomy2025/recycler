using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Recycler.API;

[ApiController]
[Route("[controller]")]
public class OrdersController(IMediator mediator, IGenericRepository<Order> orderRepository) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllOrders()
    {
        // restrict with supplier
        return Ok(new List<Order>());
    }

    [HttpGet]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    [Route("{orderNumber}")]
    public async Task<IActionResult> GetOrderByOrderNumber(Guid orderNumber)
    {
        // get Supplier's Details compare against order number
        var query = new GetOrderByOrderNumberQuery(orderNumber);
        return Ok(await mediator.Send(query));
    }
    
    
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand request)
    {
        return Ok(await mediator.Send(request));
    }

}