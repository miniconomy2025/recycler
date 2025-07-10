using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Commands
{
    public class PlaceMachineOrderCommand : IRequest<MachineOrderResponseDto>
    {
        public string? machineName { get; set; }
        public int? quantity { get; set; }
    }
}