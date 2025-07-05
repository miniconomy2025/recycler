using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Models;
using Recycler.API.Services;

namespace Recycler.API.Queries.EstimateRecyclingYield
{
    public class EstimateRecyclingYieldHandler : IRequestHandler<EstimateRecyclingYieldQuery, PhoneRecyclingEstimate>
    {
        private readonly IRecyclingService _recyclingService;

        public EstimateRecyclingYieldHandler(IRecyclingService recyclingService)
        {
            _recyclingService = recyclingService;
        }

        public async Task<PhoneRecyclingEstimate> Handle(EstimateRecyclingYieldQuery request, CancellationToken cancellationToken)
        {
            return await _recyclingService.EstimateRecyclingYieldAsync(request.PhoneId, request.Quantity);
        }
    }
}