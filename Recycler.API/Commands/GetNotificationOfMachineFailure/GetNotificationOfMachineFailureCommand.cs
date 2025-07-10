using MediatR;

namespace RecyclerApi.Commands
{
    public class GetNotificationOfMachineFailureCommand : IRequest
    {
        public string? MachineName { get; set; }
        public int FailureQuantity { get; set; } 
        public DateOnly SimulationDate { get; set; } 
        public TimeOnly SimulationTime { get; set; } 
    }
}