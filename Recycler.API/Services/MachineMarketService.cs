using Recycler.API.Utils;

namespace Recycler.API.Services;

public class MachineMarketService
{
    private readonly HttpClient _http;

    public MachineMarketService(IHttpClientFactory factory, IConfiguration config)
    {
        _http = factory.CreateClient("test");
        var thoHUrl = config["thoHApiUrl"] ?? "http://localhost:8084";
        _http.BaseAddress = new Uri(thoHUrl);
    }

    public async Task<MachineDto?> GetRecyclingMachineAsync(CancellationToken cancellationToken)
    {
        var response = await RetryHelper.RetryAsync(
            () => _http.GetAsync("/machines", cancellationToken),
            operationName: "Fetch machines");

        var market = await response.Content.ReadFromJsonAsync<MachineMarketResponse>(cancellationToken: cancellationToken);

        return market?.machines.FirstOrDefault(m => m.machineName == "recycling_machine");
    }

    public class MachineMarketResponse
    {
        public List<MachineDto> machines { get; set; } = new();
    }

    public class MachineDto
    {
        public string machineName { get; set; } = default!;
        public int quantity { get; set; }
        public string materialRatio { get; set; } = default!;
        public int productionRate { get; set; }
        public decimal price { get; set; }
    }
}
