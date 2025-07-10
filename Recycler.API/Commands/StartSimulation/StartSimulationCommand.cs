using System.Text.Json.Serialization;
using MediatR;

namespace Recycler.API.Commands.StartSimulation;

public class StartSimulationCommand : IRequest<StartSimulationResponse>
{
    [JsonPropertyName("epochStartTime")]
    public long? EpochStartTime { get; set; } 
}
