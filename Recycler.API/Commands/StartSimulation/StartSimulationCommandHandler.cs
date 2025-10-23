using MediatR;
using Recycler.API.Services;

namespace Recycler.API.Commands.StartSimulation;
using Microsoft.Extensions.DependencyInjection;

public class StartSimulationCommandHandler : IRequestHandler<StartSimulationCommand, StartSimulationResponse>
{
    private readonly HttpClient _http;
    private readonly ISimulationClock _clock;
    private readonly IDatabaseResetService _resetService;
    private readonly IServiceScopeFactory _scopeFactory;

    public StartSimulationCommandHandler(
        IHttpClientFactory httpFactory,
        ISimulationClock clock,
        IDatabaseResetService resetService,
        IServiceScopeFactory scopeFactory)
    {
        _http = httpFactory.CreateClient("test");
        _clock = clock;
        _resetService = resetService;
        _scopeFactory = scopeFactory;
    }
    public async Task<StartSimulationResponse> Handle(StartSimulationCommand request, CancellationToken cancellationToken)
    {
        await _resetService.ResetAsync(cancellationToken);
        DateTime? realStart = request.EpochStartTime.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(request.EpochStartTime.Value).UtcDateTime
            : null;

        _clock.Start(realStart);

        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var bootstrapService = scope.ServiceProvider.GetRequiredService<ISimulationBootstrapService>();
            await bootstrapService.RunAsync(CancellationToken.None);
        });

        var simTime = _clock.GetCurrentSimulationTime();

        return new StartSimulationResponse
        {
            Status = "started",
            Message = $"Simulation clock started at {simTime:yyyy-MM-dd HH:mm:ss}."
        };
    }
}
