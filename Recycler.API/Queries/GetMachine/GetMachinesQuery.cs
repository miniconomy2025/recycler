using MediatR;
using RecyclerApi.Models;
using System.Collections.Generic;

namespace RecyclerApi.Queries
{
    public class GetMachinesQuery : IRequest<List<ReceivedMachineDto>>
    {
        
    }
}