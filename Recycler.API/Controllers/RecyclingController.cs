using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Recycler.API.Commands.StartRecycling;
using Recycler.API.Dto;
using Recycler.API.Models;
using Recycler.API.Queries.EstimateRecyclingYield;
using Recycler.API.Queries.GetAvailablePhones;
using Recycler.API.Queries.GetMaterialInventory;
using Recycler.API.Queries.GetRecyclingEligibility;
using Recycler.API.Queries.GetTotalPhoneInventory;

namespace Recycler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecyclingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RecyclingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("start-recycling")]
        [ProducesResponseType(typeof(RecyclingResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecyclingResult>> StartRecycling()
        {
            var command = new StartRecyclingCommand();
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("eligibility")]
        [ProducesResponseType(typeof(RecyclingEligibilityResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecyclingEligibilityResult>> CheckEligibility([FromQuery] CheckRecyclingEligibilityQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("estimate-yield")]
        [ProducesResponseType(typeof(PhoneRecyclingEstimate), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PhoneRecyclingEstimate>> EstimateYield([FromQuery] EstimateRecyclingYieldQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("inventory/materials")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Dictionary<string, int>>> GetMaterialInventory()
        {
            var result = await _mediator.Send(new GetMaterialInventoryQuery());
            return Ok(result);
        }

        [HttpGet("inventory/phones")]
        [ProducesResponseType(typeof(List<PhoneInventoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<PhoneInventoryDto>>> GetAvailablePhones([FromQuery] GetAvailablePhonesQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

    }
}