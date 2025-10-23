using System.Text.Json;
using Recycler.API.Models.ExternalApiRequests;
using Recycler.API.Models.Thoh;

namespace Recycler.API.Services;

public class AvailablePhonesNotificationService
{
    private readonly ThohService _thohService;
    private readonly ConsumerLogisticsService _consumerLogisticsService;
    private readonly MakePaymentService _paymentService;
    private readonly ILogService _logService;
    private readonly ILogger<AvailablePhonesNotificationService> _logger;

    public AvailablePhonesNotificationService(
        ThohService thohService,
        ConsumerLogisticsService consumerLogisticsService,
        MakePaymentService paymentService,
        ILogService logService,
        ILogger<AvailablePhonesNotificationService> logger)
    {
        _thohService = thohService;
        _consumerLogisticsService = consumerLogisticsService;
        _paymentService = paymentService;
        _logService = logService;
        _logger = logger;
    }

    public record NotifyResult(bool Success, string Message, List<object> Items);

    public async Task<NotifyResult> NotifyAsync(HttpContext? httpContext = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notify flow started: fetching broken phones from THoH");

        var phones = await _thohService.GetAvailableRecycledPhonesAsync();

        _logger.LogInformation("THoH returned {Count} phone model(s)", phones?.Count ?? 0);

        if (phones is null || phones.Count == 0)
        {
            _logger.LogInformation("No broken phones available from THoH. Ending notify flow.");
            return new NotifyResult(false, "No broken phones received from THoH.", new List<object>());
        }

        var results = new List<object>();

        foreach (var phone in phones)
        {
            _logger.LogInformation("Creating delivery order: model={Model} quantity={Qty}", phone.ModelName, phone.Quantity);
            var deliveryOrder = new DeliveryOrderRequestDto
            {
                modelName = phone.ModelName,
                quantity = phone.Quantity
            };

            try
            {
                var response = await _consumerLogisticsService.SendDeliveryOrderAsync(deliveryOrder);
                _logger.LogInformation("Sent delivery order to Consumer Logistics. StatusCode={StatusCode}", (int)response.StatusCode);
                var responseBodyText = await response.Content.ReadAsStringAsync(cancellationToken);
                var deliveryResponse = JsonSerializer.Deserialize<DeliveryOrderResponseDto>(responseBodyText);

                if (deliveryResponse == null)
                {
                    throw new Exception("Failed to parse delivery order response.");
                }

                _logger.LogInformation("Proceeding to payment for model={Model} reference={Reference}", phone.ModelName, deliveryResponse.referenceNo);

                await _paymentService.SendPaymentAsync(
                    toAccountNumber: deliveryResponse.accountNumber,
                    amount: decimal.Parse(deliveryResponse.amount),
                    description: deliveryResponse.referenceNo
                );

                _logger.LogInformation("Payment attempt completed for reference={Reference}", deliveryResponse.referenceNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notify flow failed for model={Model}", phone.ModelName);
                results.Add(new
                {
                    phone.ModelName,
                    Status = "Failed",
                    Error = ex.Message
                });
            }
        }

        results.Add(new { Status = "Success" });

        await _logService.CreateLog(httpContext, "AvailablePhonesNotificationService",
            new { message = "Completed delivery orders for all phones.", results });

        _logger.LogInformation("Notify flow finished. ItemsProcessed={Count}", results.Count);

        return new NotifyResult(true, "Completed delivery orders for all phones.", results);
    }
}


