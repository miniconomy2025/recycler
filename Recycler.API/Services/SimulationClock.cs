public interface ISimulationClock
{
    DateTime GetCurrentSimulationTime();
    void Start(DateTime? customStartTime = null);

    DateTime GetSimulationTime(DateTime realTime);
}


public class SimulationClock : ISimulationClock
{
    private readonly DateTime _simStart = new(2050, 1, 1, 0, 0, 0);
    private DateTime? _realStart;
    

    public void Start(DateTime? customStartTime = null)
    {
        _realStart = customStartTime ?? DateTime.UtcNow;
    }

    public DateTime GetCurrentSimulationTime()
    {
        if (_realStart == null)
            throw new InvalidOperationException("Simulation clock not started.");

        var elapsedRealSeconds = (DateTime.UtcNow - _realStart.Value).TotalSeconds;
        var elapsedSimulationMinutes = elapsedRealSeconds * 12;
        Console.WriteLine($"{_realStart}");
        Console.WriteLine($"\nElapsed simulation minutes: {elapsedSimulationMinutes}\n");
        Console.WriteLine($"\nElapsed real seconds: {elapsedRealSeconds}\n");
        return _simStart.AddMinutes(elapsedSimulationMinutes);
    }
    
    
    public DateTime GetSimulationTime(DateTime realTime)
    {
        if (_realStart == null)
            throw new InvalidOperationException("Simulation clock not started.");

        var elapsedRealSeconds = (realTime - _realStart.Value).TotalSeconds;
        var elapsedSimulationMinutes = elapsedRealSeconds * 12;
        return _simStart.AddMinutes(elapsedSimulationMinutes);
    }
}