using MediatR;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using System.Text;
using System.Text.Json;
using Recycler.API.Utils;

namespace RecyclerApi.Handlers
{
    public class PlaceMachineOrderCommandHandler : IRequestHandler<PlaceMachineOrderCommand, MachineOrderResponseDto>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PlaceMachineOrderCommandHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<MachineOrderResponseDto> Handle(PlaceMachineOrderCommand request, CancellationToken cancellationToken)
        {
            var thoHApiBaseUrl = _configuration["thoHApiUrl"] ?? "";
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(thoHApiBaseUrl);

            var machineOrderRequest = new MachineOrderRequestDto
            {
                machineName = request.machineName,
                quantity = request.quantity,
            };

            var jsonContent = JsonSerializer.Serialize(machineOrderRequest);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await RetryHelper.RetryAsync(
                    () => httpClient.PostAsync("/simulation/purchase-machine", httpContent, cancellationToken),
                    operationName: "Place machine order");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var thoHResponse = JsonSerializer.Deserialize<MachineOrderResponseDto>(
                        responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (thoHResponse != null)
                    {
                        if (string.IsNullOrEmpty(thoHResponse.Message))
                        {
                            thoHResponse.Message = "Machine order placed successfully.";
                        }

                        return thoHResponse;
                    }

                    throw new ApplicationException("Machine order succeeded but response was empty or invalid.");
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