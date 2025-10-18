namespace Recycler.API.Models;

public class GenericResponse<T> (ISimulationClock simulationClock)
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public DateTime TimeStamp { get; set; } = simulationClock.GetCurrentSimulationTime();
}