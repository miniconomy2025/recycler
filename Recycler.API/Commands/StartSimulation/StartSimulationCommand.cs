using MediatR;

namespace Recycler.API.Commands.StartSimulation;

public class StartSimulationCommand : IRequest<StartSimulationResponse>
{
    public long? StartTime { get; set; } 
}
