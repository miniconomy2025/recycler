using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API;

[Table("OrderItems")]
public class OrderItem
{
    public int Id { get; set; }
    
    public int OrderId { get; set; }

    public int MaterialId { get; set; }

    public int Quantity { get; set; }
    
    public decimal Price { get; set; }
    
    public OrderItem MapDbObjects(OrderItemDto orderItemDto)
    {
        MaterialId = orderItemDto.RawMaterial.Id;
        Quantity = orderItemDto.Quantity;
        Price = orderItemDto.Price;

        return this;
    }
}

