using MediatR;
using Recycler.API.Models;
using System.Collections.Generic;

namespace Recycler.API.Queries
{
    public class GetMachinesQuery : IRequest<List<ReceivedMachineDto>>
    {
        
    }
}