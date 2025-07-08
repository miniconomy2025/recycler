using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Recycler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankNotificationController : ControllerBase
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<OrderStatus> _orderStatusRepository;
    private readonly ILogger<BankNotificationController> _logger;

    public BankNotificationController(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<OrderStatus> orderStatusRepository,
        ILogger<BankNotificationController> logger)
    {
        _orderRepository = orderRepository;
        _orderStatusRepository = orderStatusRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveNotification([FromBody] BankNotificationDto notification)
    {
        _logger.LogInformation("💸 Received payment notification:\n{json}",
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true }));

        try
        {
            if (notification.status.ToLower() != "success")
            {
                _logger.LogWarning("Ignoring failed payment for transaction {tx}", notification.transaction_number);
                return Ok();
            }

            if (!Guid.TryParse(notification.description, out Guid orderNumber))
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

            order.OrderStatusId = approvedStatus.Id;
            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Order {orderId} marked as Approved due to payment", order.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment notification");
            return StatusCode(500, "Internal error processing payment");
        }
    }
}
