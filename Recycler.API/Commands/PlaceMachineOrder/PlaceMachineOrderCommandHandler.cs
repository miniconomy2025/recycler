using MediatR;
using Recycler.API.Commands;
using Recycler.API.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Recycler.API.Commands
{
    public class PlaceMachineOrderCommandHandler : IRequestHandler<PlaceMachineOrderCommand, MachineOrderResponseDto>
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PlaceMachineOrderCommandHandler> _logger;

        public PlaceMachineOrderCommandHandler(IHttpClientFactory httpFactory, IConfiguration configuration, ILogger<PlaceMachineOrderCommandHandler> logger)
        {
            _httpClient = httpFactory.CreateClient("test");
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<MachineOrderResponseDto> Handle(PlaceMachineOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Placing machine order - Machine: {MachineName}, Quantity: {Quantity}", 
                request.machineName, request.quantity);

            var thoHApiBaseUrl = _configuration["thoHApiUrl"] ?? "http://localhost:5001";
            _httpClient.BaseAddress = new Uri(thoHApiBaseUrl);
            _logger.LogInformation("THoH API base URL: {ThoHUrl}", thoHApiBaseUrl);

            var machineOrderRequest = new MachineOrderRequestDto
            {
                machineName = request.machineName,
                quantity = request.quantity ?? 1
            };

            var jsonContent = JsonSerializer.Serialize(machineOrderRequest);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            _logger.LogInformation("Machine order request payload: {JsonContent}", jsonContent);

            try
            {
                _logger.LogInformation("Sending machine order to THoH API endpoint: /api/machines");
                var response = await _httpClient.PostAsync("api/machines", httpContent, cancellationToken);
                _logger.LogInformation("THoH API response status: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Machine order failed - Status: {StatusCode}, Content: {ErrorContent}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to place machine order. Status: {response.StatusCode}, Content: {errorContent}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("THoH API response body: {ResponseBody}", responseBody);

                MachineOrderResponseDto? thoHResponse = null;
                try
                {
                    thoHResponse = JsonSerializer.Deserialize<MachineOrderResponseDto>(
                        responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse THoH response JSON. Falling back to default response.");
                }

                if (thoHResponse == null)
                {
                    _logger.LogWarning("THoH API returned null or invalid response. Using default fallback values.");
                    thoHResponse = new MachineOrderResponseDto
                    {
                        Message = "Machine order placed successfully.",
                        OrderId = 0,
                        BankAccount = "000000000000"
                    };
                }

                _logger.LogInformation(
                    "Machine order placed successfully: Message={Message}, OrderId={OrderId}, BankAccount={BankAccount}",
                    thoHResponse.Message, thoHResponse.OrderId, thoHResponse.BankAccount);

                return thoHResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error communicating with ThoH API: {ErrorMessage}", ex.Message);
                throw new ApplicationException($"Error communicating with ThoH API: {ex.Message}", ex);
            }
        }

    }
}