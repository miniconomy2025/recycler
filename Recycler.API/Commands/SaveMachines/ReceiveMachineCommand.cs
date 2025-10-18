using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Commands
{
    public class ReceiveMachineCommand : IRequest<ReceivedMachineDto>
    {
        public String ModelName { get; set; } = "recycling_machine";
        public int Quantity { get; set; }
    }
}