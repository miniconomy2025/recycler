using MediatR;
using Recycler.API.Commands;
using Recycler.API.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Recycler.API
{
    public class PlaceMachineOrderCommandHandler : IRequestHandler<PlaceMachineOrderCommand, MachineOrderResponseDto>
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PlaceMachineOrderCommandHandler(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<MachineOrderResponseDto> Handle(PlaceMachineOrderCommand request, CancellationToken cancellationToken)
        {
            var thoHApiBaseUrl = _configuration["ThoHApi:BaseUrl"] ?? "http://localhost:5001";
            _httpClient.BaseAddress = new Uri(thoHApiBaseUrl);

            var machineOrderRequest = new MachineOrderRequestDto
            {
                machineName = request.machineName
            };

            var jsonContent = JsonSerializer.Serialize(machineOrderRequest);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/machines/orders", httpContent, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseMessage = "Machine order placed successfully.";
                    if (response.Content != null)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseBody))
                        {
                            try
                            {
                                var thoHResponse = JsonSerializer.Deserialize<MachineOrderResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (thoHResponse != null && !string.IsNullOrEmpty(thoHResponse.Message))
                                {
                                    responseMessage = thoHResponse.Message;
                                }
                            }
                            catch (JsonException)
                            {
                                
                            }
                        }
                    }
                    return new MachineOrderResponseDto { Message = responseMessage };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to place machine order. Status: {response.StatusCode}, Content: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException($"Error communicating with ThoH API: {ex.Message}", ex);
            }
        }
    }
}