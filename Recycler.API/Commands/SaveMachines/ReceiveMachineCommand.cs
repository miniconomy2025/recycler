using MediatR;
using RecyclerApi.Models;

namespace RecyclerApi.Commands
{
    public class ReceiveMachineCommand : IRequest<ReceivedMachineDto>
    {
        public int MachineId { get; set; } 
    }
}