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
                _logger.LogInformation("Sending machine order to THoH API endpoint: /machines");
                var response = await _httpClient.PostAsync("/machines", httpContent, cancellationToken);
                _logger.LogInformation("THoH API response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var responseMessage = "Machine order placed successfully.";
                    if (response.Content != null)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("THoH API response body: {ResponseBody}", responseBody);
                        
                        if (!string.IsNullOrEmpty(responseBody))
                        {
                            try
                            {
                                var thoHResponse = JsonSerializer.Deserialize<MachineOrderResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (thoHResponse != null && !string.IsNullOrEmpty(thoHResponse.Message))
                                {
                                    responseMessage = thoHResponse.Message;
                                    _logger.LogInformation("Parsed THoH response message: {Message}", thoHResponse.Message);
                                }
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogWarning(ex, "Failed to parse THoH response JSON");
                            }
                        }
                    }
                    
                    _logger.LogInformation("Machine order placed successfully: {ResponseMessage}", responseMessage);
                    return new MachineOrderResponseDto { Message = responseMessage };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Machine order failed - Status: {StatusCode}, Content: {ErrorContent}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to place machine order. Status: {response.StatusCode}, Content: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error communicating with ThoH API: {ErrorMessage}", ex.Message);
                throw new ApplicationException($"Error communicating with ThoH API: {ex.Message}", ex);
            }
        }
    }
}