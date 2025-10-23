using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Recycler.API.Services;

namespace Recycler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankNotificationController : ControllerBase
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<OrderStatus> _orderStatusRepository;
    private readonly ILogger<BankNotificationController> _logger;
    private readonly ILogService _logService;

    public BankNotificationController(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<OrderStatus> orderStatusRepository,
        ILogger<BankNotificationController> logger,
        ILogService logService)
    {
        _orderRepository = orderRepository;
        _orderStatusRepository = orderStatusRepository;
        _logger = logger;
        _logService = logService;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveNotification([FromBody] BankNotificationDto notification)
    {
        _logger.LogInformation("💸 Received payment notification:\n{json}",
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true }));

        try
        {
            if (!string.Equals(notification.status, "success", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Ignoring failed payment for transaction {tx}", notification.transaction_number);
                return Ok();
            }

            Guid orderNumber;

            if (Guid.TryParse(notification.description, out orderNumber))
            {
                // already valid GUID
            }
            else if (int.TryParse(notification.description, out int orderInt))
            {
                var dbOrder = (await _orderRepository.GetByColumnValueAsync("id", orderInt)).FirstOrDefault();
                if (dbOrder == null)
                {
                    _logger.LogError("Order with id {id} not found for numeric description.", orderInt);
                    return BadRequest("Invalid order number format");
                }

                orderNumber = dbOrder.OrderNumber;
            }
            else
            {
                _logger.LogError("Invalid order number format in payment description: {desc}", notification.description);
                return BadRequest("Invalid order number format");
            }

            var order = (await _orderRepository.GetByColumnValueAsync("order_number", orderNumber)).FirstOrDefault();
            if (order == null)
            {
                _logger.LogError("No order found for order number: {orderNumber}", orderNumber);
                return NotFound("Order not found");
            }

            var approvedStatus = (await _orderStatusRepository.GetByColumnValueAsync("name", "Approved")).FirstOrDefault();
            if (approvedStatus == null)
            {
                _logger.LogError("'Approved' status not configured in OrderStatus table.");
                return StatusCode(500, "Order status configuration missing");
            }
            
            try
            {
                order.OrderStatusId = approvedStatus.Id;
                await _orderRepository.UpdateAsync(order, new List<string> { "OrderStatusId" });

                _logger.LogInformation("Order {orderId} marked as Approved due to payment", order.Id);
                await _logService.CreateLog(HttpContext, notification, Ok());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {id} after payment confirmation", order.Id);
                await _logService.CreateLog(HttpContext, notification, StatusCode(500, "Internal error processing payment"));
                return StatusCode(500, "Internal error processing payment");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment notification");
            await _logService.CreateLog(HttpContext, notification, StatusCode(500, "Internal error processing payment"));
            return StatusCode(500, "Internal error processing payment");
        }
    }
}
