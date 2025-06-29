namespace Recycler.API;

public class Order
{
    public Guid OrderNumber { get; set; }

    public int OrderStatusId { get; set; }
    
    public OrderStatus OrderStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public int SupplierId { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
}

