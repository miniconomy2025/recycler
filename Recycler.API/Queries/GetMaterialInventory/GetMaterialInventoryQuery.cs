using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Recycler.API.Queries.GetMaterialInventory
{
    public class GetMaterialInventoryQuery : IRequest<Dictionary<string, int>>
    {
        // Returns material name -> quantity in kg
    }
}