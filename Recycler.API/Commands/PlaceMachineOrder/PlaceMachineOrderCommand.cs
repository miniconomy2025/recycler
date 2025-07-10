using MediatR;
using RecyclerApi.Models;

namespace RecyclerApi.Commands
{
    public class PlaceMachineOrderCommand : IRequest<MachineOrderResponseDto>
    {
        public string? machineName { get; set; }
    }
}