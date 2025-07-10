using MediatR;

namespace RecyclerApi.Commands
{
    public class GetNotificationOfMachineFailureCommand : IRequest
    {
        public string? MachineName { get; set; }
        public int FailureQuantity { get; set; } 
        public DateTime SimulationDate { get; set; } 
        public string SimulationTime { get; set; } = string.Empty;
    }
}