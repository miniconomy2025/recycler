using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Models;
using Recycler.API.Services;

namespace Recycler.API.Queries.GetRecyclingEligibility
{
     public class CheckRecyclingEligibilityQueryHandler : IRequestHandler<CheckRecyclingEligibilityQuery, RecyclingEligibilityResult>
    {
        private readonly IRecyclingService _recyclingService;

        public CheckRecyclingEligibilityQueryHandler(IRecyclingService recyclingService)
        {
            _recyclingService = recyclingService;
        }

        public async Task<RecyclingEligibilityResult> Handle(CheckRecyclingEligibilityQuery request, CancellationToken cancellationToken)
        {
            return await _recyclingService.CheckRecyclingEligibilityAsync();
        }
    }
}