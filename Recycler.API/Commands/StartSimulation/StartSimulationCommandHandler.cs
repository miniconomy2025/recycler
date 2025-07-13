using MediatR;
using Recycler.API.Services;

namespace Recycler.API.Commands.StartSimulation;

public class StartSimulationCommandHandler : IRequestHandler<StartSimulationCommand, StartSimulationResponse>
{
    private readonly HttpClient _http;
    private readonly ISimulationClock _clock;
    private readonly IDatabaseResetService _resetService;
    private readonly SimulationBootstrapService _bootstrap;

    public StartSimulationCommandHandler(
        IHttpClientFactory httpFactory,
        ISimulationClock clock,
        IDatabaseResetService resetService,
        SimulationBootstrapService bootstrap)
    {
        _http = httpFactory.CreateClient("test");
        _clock = clock;
        _resetService = resetService;
        _bootstrap = bootstrap;
    }
    public async Task<StartSimulationResponse> Handle(StartSimulationCommand request, CancellationToken cancellationToken)
    {
        await _resetService.ResetAsync(cancellationToken);
        DateTime? realStart = request.EpochStartTime.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(request.EpochStartTime.Value).UtcDateTime
            : null;

        _clock.Start(realStart);

        _ = Task.Run(() => _bootstrap.RunAsync(CancellationToken.None));

        var simTime = _clock.GetCurrentSimulationTime();

        return new StartSimulationResponse
        {
            Status = "started",
            Message = $"Simulation clock started at {simTime:yyyy-MM-dd HH:mm:ss}."
        };
    }
}
