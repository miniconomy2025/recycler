using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Queries.GetRevenueReport;

namespace Recycler.API.Controllers;

[ApiController]
[Route("internal/revenue")]
[EnableCors("InternalApiCors")]
public class RevenueController(IMediator mediator) : ControllerBase
{
    [HttpGet("company-orders")]
    [ProducesResponseType(typeof(List<RevenueReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyOrders()
    {
        var query = new GetRevenueReportQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }
}
