using MediatR;
using RecyclerApi.Models;

namespace RecyclerApi.Commands
{
    public class PlaceMachineOrderCommand : IRequest<MachineOrderResponseDto>
    {
        public int MachineId { get; set; }
        public int Quantity { get; set; }
    }
}