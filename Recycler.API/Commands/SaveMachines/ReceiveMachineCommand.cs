using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Commands
{
    public class ReceiveMachineCommand : IRequest<ReceivedMachineDto>
    {
        public int MachineId { get; set; } 
    }
}