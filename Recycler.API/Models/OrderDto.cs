namespace Recycler.API;

public class OrderDto
{
    public Guid OrderNumber { get; set; }

    public int OrderStatusId { get; set; }
    
    public OrderStatus? OrderStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public int SupplierId { get; set; }

    public IEnumerable<OrderItem>? OrderItems { get; set; }

    public OrderDto MapDbObjects(Order order, OrderStatus orderStatus, IEnumerable<OrderItem> orderItems)
    {
        OrderNumber = order.OrderNumber;
        OrderStatusId = order.OrderStatusId;
        OrderStatus = orderStatus;
        CreatedAt = order.CreatedAt;
        SupplierId = order.SupplierId;
        OrderItems = orderItems;

        return this;
    }
}

