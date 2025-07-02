using System.Net.Http;
using System.Net.Http.Json;
using MediatR;

namespace Recycler.API.Commands.StartSimulation;

public class StartSimulationCommandHandler : IRequestHandler<StartSimulationCommand, StartSimulationResponse>
{
    private readonly HttpClient _http;
    private readonly ISimulationClock _clock;

    public StartSimulationCommandHandler(IHttpClientFactory httpFactory, ISimulationClock clock)
    {
        _http = httpFactory.CreateClient();
        //TODO use real bank url
        _http.BaseAddress = new Uri("http://localhost:8085");
        _clock = clock;
    }

    public async Task<StartSimulationResponse> Handle(StartSimulationCommand request, CancellationToken cancellationToken)
    {
        _clock.Start();

        var accountResponse = await _http.PostAsJsonAsync("/account", new { }, cancellationToken);
        if (!accountResponse.IsSuccessStatusCode)
            return new StartSimulationResponse { Status = "error", Message = "Failed to create bank account" };

        var accountData = await accountResponse.Content.ReadFromJsonAsync<AccountResponse>(cancellationToken: cancellationToken);
        if (accountData is null)
            return new StartSimulationResponse { Status = "error", Message = "Invalid bank account response" };

        _http.DefaultRequestHeaders.Add("X-API-Key", accountData.api_key);

        var loanRequest = new { amount = 1000000 };
        var loanResponse = await _http.PostAsJsonAsync("/loan", loanRequest, cancellationToken);
        if (!loanResponse.IsSuccessStatusCode)
            return new StartSimulationResponse { Status = "error", Message = "Loan request failed" };

        var loanData = await loanResponse.Content.ReadFromJsonAsync<LoanResponse>(cancellationToken: cancellationToken);

        var currentSimTime = _clock.GetCurrentSimulationTime();

        return new StartSimulationResponse
        {
            Status = "started",
            Message = $"Simulation initialized with loan #{loanData?.loan_number ?? "unknown"} at sim time {currentSimTime:yyyy-MM-dd HH:mm:ss}"
        };
    }


    private class AccountResponse
    {
        public string account_number { get; set; } = default!;
        public string api_key { get; set; } = default!;
    }

    private class LoanResponse
    {
        public string loan_number { get; set; } = default!;
        public bool success { get; set; }
    }
}
