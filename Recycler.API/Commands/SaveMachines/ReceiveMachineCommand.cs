using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Commands
{
    public class ReceiveMachineCommand : IRequest<ReceivedMachineDto>
    {
        public string ModelName { get; set; }
        public int Quantity { get; set; }
    }
}