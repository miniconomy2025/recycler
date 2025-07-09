namespace Recycler.API;

public class OrderItemDto
{
    public RawMaterial RawMaterial { get; set; }

    public int QuantityInKg { get; set; }
    
    public decimal PricePerKg { get; set; }
    
    public OrderItemDto MapDbObjects(OrderItem orderItem, RawMaterial rawMaterial)
    {
        RawMaterial = rawMaterial;
        QuantityInKg = orderItem.QuantityInKg;
        PricePerKg = orderItem.PricePerKg;

        return this;
    }
}

