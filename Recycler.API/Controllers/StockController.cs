using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Models;
using Recycler.API.Models.ExternalApiRequests;
using Recycler.API.Queries.GetRevenueReport;
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
        var _http = httpClientFactory.CreateClient("test");

        //         var content = new StringContent(
        //     JsonSerializer.Serialize(order),
        //     Encoding.UTF8,
        //     mediaType: "application/json"
        // );

        var temp = await _http.PostAsJsonAsync("https://consumer-logistics-api.projects.bbdgrad.com/api/pickups",

            new DeliveryOrderRequestDto
            {
                companyName = "Recycler",
                quantity = 1,
                recipient = "Recycler",
                modelName = "Cosmos_Z25_ultra"
            }
        );
        var result = temp.Content.ReadAsStringAsync();
        var query = new GetStockQuery();
        // var result = await mediator.Send(query);

        await logService.CreateLog(HttpContext, "", Ok(result));

        return Ok(result);
    }
}
