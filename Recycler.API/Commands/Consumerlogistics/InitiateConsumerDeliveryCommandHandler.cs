using MediatR;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RecyclerApi.Handlers
{
    public class InitiateConsumerDeliveryCommandHandler : IRequestHandler<InitiateConsumerDeliveryCommand, ConsumerLogisticsDeliveryResponseDto>
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public InitiateConsumerDeliveryCommandHandler(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ConsumerLogisticsDeliveryResponseDto> Handle(InitiateConsumerDeliveryCommand request, CancellationToken cancellationToken)
        {
            var consumerLogisticsApiBaseUrl = _configuration["ConsumerLogisticsApi:BaseUrl"] ?? "http://localhost:5002";
            _httpClient.BaseAddress = new Uri(consumerLogisticsApiBaseUrl);

            var deliveryRequest = new ConsumerLogisticsDeliveryRequestDto
            {
                CompanyName = request.CompanyName,
                Quantity = request.Quantity,
                Recipient = request.Recipient,
                ModelName = request.ModelName
            };

            var jsonContent = JsonSerializer.Serialize(deliveryRequest);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/delivery-orders", httpContent, cancellationToken);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var deliveryResponse = JsonSerializer.Deserialize<ConsumerLogisticsDeliveryResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return deliveryResponse;
            }
            catch (HttpRequestException ex)
            {
                var errorDetails = response?.Content != null ? await response.Content.ReadAsStringAsync() : "No content";
                throw new ApplicationException($"Failed to initiate consumer delivery. Status: {response?.StatusCode}, Details: {errorDetails}. Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An unexpected error occurred while initiating consumer delivery: {ex.Message}", ex);
            }
        }
    }
}

