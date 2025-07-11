using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Models.ExternalApiResponses;
using Recycler.API.Utils;

namespace Recycler.API.Commands.CreatePickupRequest;

public class CreatePickupRequestCommandHandler : IRequestHandler<CreatePickupRequestCommand, CreatePickupRequestResponse>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CreatePickupRequestCommandHandler(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<CreatePickupRequestResponse> Handle(CreatePickupRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var logisticsUrl = _configuration["bulkLogisticsUrl"] ?? "";

            var pickupRequest = new
            {
                originalExternalOrder = request.originalExternalOrder,
                originCompany = request.originCompany,
                destinationCompany = request.destinationCompany,
                items = request.items.Select(item => new
                {
                    itemName = item.itemName,
                    quantity = item.quantity,
                }).ToArray()
            };

            var httpClient = _httpClientFactory.CreateClient("test");

            var response = await RetryHelper.RetryAsync(
                () => httpClient.PostAsJsonAsync($"{logisticsUrl}/api/pickup-request", pickupRequest, cancellationToken),
                operationName: "Create logistics pickup request");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new CreatePickupRequestResponse
                {
                    Success = false,
                    Message = $"Failed to create pickup request: {response.StatusCode} - {errorContent}"
                };
            }

            var logisticsResult = await response.Content.ReadFromJsonAsync<BulkLogisticsResponse>(cancellationToken: cancellationToken);

            if (logisticsResult == null)
            {
                return new CreatePickupRequestResponse
                {
                    Success = false,
                    Message = "Invalid response from logistics API"
                };
            }

            return new CreatePickupRequestResponse
            {
                Success = true,
                Message = $"Pickup request created successfully. ID: {logisticsResult.pickupRequestId}",
                PickupRequestId = logisticsResult.pickupRequestId,
                Cost = (decimal)logisticsResult.cost,
                PaymentReferenceId = logisticsResult.paymentReferenceId,
                BulkLogisticsBankAccount = logisticsResult.bulkLogisticsBankAccountNumber,
                Status = logisticsResult.status,
                StatusCheckUrl = logisticsResult.statusCheckUrl
            };
        }
        catch (Exception ex)
        {
            return new CreatePickupRequestResponse
            {
                Success = false,
                Message = $"Error creating pickup request: {ex.Message}"
            };
        }
    }

    private class BulkLogisticsResponse
    {
        public int pickupRequestId { get; set; }
        public double cost { get; set; }
        public string paymentReferenceId { get; set; } = default!;
        public string bulkLogisticsBankAccountNumber { get; set; } = default!;
        public string status { get; set; } = default!;
        public string statusCheckUrl { get; set; } = default!;
    }

}
