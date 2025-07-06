using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Queries.GetRecyclingEligibility
{
    public class CheckRecyclingEligibilityQuery : IRequest<RecyclingEligibilityResult>
    {
        // Checks if we have 1000+ phones and shows what we'll get
    }
}