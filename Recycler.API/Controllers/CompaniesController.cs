using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecyclerApi.Commands;
using RecyclerApi.Models;

namespace RecyclerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompaniesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateCompanyResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(CreateCompany), new { id = result.CompanyId }, result);
        }
    }
}