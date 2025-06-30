using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API;

[Table("Orders")]
public class Order
{
    public int Id { get; set; }

    public Guid OrderNumber { get; set; }

    public int OrderStatusId { get; set; }
    
    public OrderStatus OrderStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public int SupplierId { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
}

