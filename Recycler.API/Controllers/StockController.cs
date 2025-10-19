using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Models;
using Recycler.API.Models.ExternalApiRequests;
using Recycler.API.Queries;
using Recycler.API.Services;


namespace Recycler.API.Controllers;

[ApiController]
[Route("internal/stock")]
[EnableCors("InternalApiCors")]
public class StockController(IHttpClientFactory httpClientFactory, IMediator mediator, ILogService logService) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(StockSet), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStock()
    {
        var query = new GetStockQuery();
        var result = await mediator.Send(query);

        await logService.CreateLog(HttpContext, "", Ok(result));

        return Ok(result);
    }
}
