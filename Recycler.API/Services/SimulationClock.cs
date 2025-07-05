public interface ISimulationClock
{
    DateTime GetCurrentSimulationTime();
    void Start(DateTime? customStartTime = null);
}


public class SimulationClock : ISimulationClock
{
    private readonly DateTime _simStart = new(2050, 1, 1);
    private DateTime? _realStart;

    public void Start(DateTime? customStartTime = null)
    {
        _realStart = customStartTime ?? DateTime.UtcNow;
    }

    public DateTime GetCurrentSimulationTime()
    {
        if (_realStart == null)
            throw new InvalidOperationException("Simulation clock not started.");

        var elapsed = DateTime.UtcNow - _realStart.Value;
        var simDays = elapsed.TotalMinutes * 0.5;
        return _simStart.AddDays(simDays);
    }
}

