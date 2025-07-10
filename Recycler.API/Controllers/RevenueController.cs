using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Queries.GetRevenueReport;
using Recycler.API.Services;

namespace Recycler.API.Controllers;

[ApiController]
[Route("internal/revenue")]
[EnableCors("InternalApiCors")]
public class RevenueController(IMediator mediator, ILogService logService) : ControllerBase
{
    [HttpGet("company-orders")]
    [ProducesResponseType(typeof(List<RevenueReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyOrders()
    {
        var query = new GetRevenueReportQuery();
        var result = await mediator.Send(query);
        
        await logService.CreateLog(HttpContext, "", Ok(result));
        
        return Ok(result);
    }
}
