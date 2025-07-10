using MediatR;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Queries.GetMaterials;
using Recycler.API.Services;

namespace Recycler.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MaterialsController(IMediator mediator, ILogService logService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RawMaterialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaterials()
    {
        var query = new GetMaterialsQuery();
        var result = await mediator.Send(query);
        
        await logService.CreateLog(HttpContext, "", result);
        
        return Ok(result);
    }
}
