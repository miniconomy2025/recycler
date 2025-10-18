using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API;

[Table("OrderItems")]
public class OrderItem
{
    public int Id { get; set; }
    
    public int OrderId { get; set; }

    public int MaterialId { get; set; }

    public int QuantityInKg { get; set; }
    
    public decimal PricePerKg { get; set; }
    
    public OrderItem MapDbObjects(OrderItemDto orderItemDto)
    {
        MaterialId = orderItemDto.RawMaterial.Id;
        QuantityInKg = orderItemDto.QuantityInKg;
        PricePerKg = orderItemDto.PricePerKg;

        return this;
    }
}

