using MediatR;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Services;
using RecyclerApi.Commands;
using RecyclerApi.Models;

namespace RecyclerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogService _logService;

        public CompaniesController(IMediator mediator, ILogService logService)
        {
            _mediator = mediator;
            _logService = logService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateCompanyResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyCommand command)
        {
            var result = await _mediator.Send(command);
            var response = CreatedAtAction(nameof(CreateCompany), new { id = result.CompanyId }, result);
           
            await _logService.CreateLog(HttpContext, command, response);
            
            return response;
        }
    }
}