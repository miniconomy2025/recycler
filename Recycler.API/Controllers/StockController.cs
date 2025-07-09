using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Models;
using Recycler.API.Queries.GetRevenueReport;

namespace Recycler.API.Controllers;

[ApiController]
[Route("internal/stock")]
[EnableCors("InternalApiCors")]
public class StockController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(StockSet), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStock()
    {
        var query = new GetStockQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }
}
