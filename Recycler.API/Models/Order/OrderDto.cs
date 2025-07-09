namespace Recycler.API;

public class OrderDto(ISimulationClock simulationClock)
{
    public int OrderId { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CompanyId { get; set; }
    
    public DateTime OrderExpiresAt { get; set; }

    public IEnumerable<OrderItemDto> OrderItems { get; set; }

    public OrderDto MapDbObjects(Order? order, OrderStatus? orderStatus, IEnumerable<OrderItemDto> orderItems)
    {
        if (order != null)
        {
            OrderId = order.Id;
            CompanyId = order.CompanyId;
            CreatedAt = simulationClock.GetSimulationTime(order.CreatedAt);
            OrderExpiresAt = simulationClock.GetSimulationTime(order.OrderExpiresAt);
        }

        if (orderStatus != null)
        {
            OrderStatus = orderStatus;
        }
        
        OrderItems = orderItems;

        return this;
    }
}

