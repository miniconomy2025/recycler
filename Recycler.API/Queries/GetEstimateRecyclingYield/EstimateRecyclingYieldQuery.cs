using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Queries.EstimateRecyclingYield
{
    public class EstimateRecyclingYieldQuery : IRequest<PhoneRecyclingEstimate>
    {
        public int PhoneId { get; set; }
        public int Quantity { get; set; }
    }

}