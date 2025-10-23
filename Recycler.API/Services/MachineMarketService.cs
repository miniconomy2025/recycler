using Recycler.API.Utils;

namespace Recycler.API.Services;

public class MachineMarketService
{
    private readonly HttpClient _http;
    private readonly ILogger<MachineMarketService> _logger;

    public MachineMarketService(IHttpClientFactory factory, IConfiguration config, ILogger<MachineMarketService> logger)
    {
        _http = factory.CreateClient("test");
        var thoHUrl = config["thoHApiUrl"] ?? "http://localhost:8084";
        _http.BaseAddress = new Uri(thoHUrl);
        _logger = logger;
    }

    public async Task<MachineDto?> GetRecyclingMachineAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching available machines from THoH market");
        
        var response = await RetryHelper.RetryAsync(
            () => _http.GetAsync("/machines", cancellationToken),
            operationName: "Fetch machines");

        _logger.LogInformation("Machines API response status: {StatusCode}", response.StatusCode);

        var market = await response.Content.ReadFromJsonAsync<MachineMarketResponse>(cancellationToken: cancellationToken);

        if (market?.machines == null || !market.machines.Any())
        {
            _logger.LogWarning("No machines found in market response");
            return null;
        }

        _logger.LogInformation("Found {MachineCount} machines in market", market.machines.Count);
        
        var recyclingMachine = market.machines.FirstOrDefault(m => m.machineName == "recycling_machine");
        
        if (recyclingMachine != null)
        {
            _logger.LogInformation("Found recycling machine: {MachineName}, Price: {Price}, Quantity: {Quantity}, Production Rate: {ProductionRate}", 
                recyclingMachine.machineName, recyclingMachine.price, recyclingMachine.quantity, recyclingMachine.productionRate);
        }
        else
        {
            _logger.LogWarning("No recycling machine found in market. Available machines: {MachineNames}", 
                string.Join(", ", market.machines.Select(m => m.machineName)));
        }

        return recyclingMachine;
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
