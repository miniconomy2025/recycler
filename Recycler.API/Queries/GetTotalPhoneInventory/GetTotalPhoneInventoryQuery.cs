using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Recycler.API.Queries.GetTotalPhoneInventory
{
    public class GetTotalPhoneInventoryQuery : IRequest<int>
    {
        // Returns total number of phones available for recycling
    }
}