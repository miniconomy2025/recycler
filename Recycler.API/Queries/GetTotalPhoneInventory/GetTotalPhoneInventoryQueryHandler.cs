using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Services;

namespace Recycler.API.Queries.GetTotalPhoneInventory
{
    public class GetTotalPhoneInventoryQueryHandler : IRequestHandler<GetTotalPhoneInventoryQuery, int>
    {
        private readonly IRecyclingService _recyclingService;

        public GetTotalPhoneInventoryQueryHandler(IRecyclingService recyclingService)
        {
            _recyclingService = recyclingService;
        }

        public async Task<int> Handle(GetTotalPhoneInventoryQuery request, CancellationToken cancellationToken)
        {
            return await _recyclingService.GetTotalPhoneInventoryAsync();
        }
    }
}