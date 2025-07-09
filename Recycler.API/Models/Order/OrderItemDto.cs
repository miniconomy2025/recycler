namespace Recycler.API;

public class OrderItemDto
{
    public RawMaterial RawMaterial { get; set; }

    public int Quantity { get; set; }
    
    public decimal Price { get; set; }
    
    public OrderItemDto MapDbObjects(OrderItem orderItem, RawMaterial rawMaterial)
    {
        RawMaterial = rawMaterial;
        Quantity = orderItem.Quantity;
        Price = orderItem.Price;

        return this;
    }
}

