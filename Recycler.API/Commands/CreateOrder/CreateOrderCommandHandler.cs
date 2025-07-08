using MediatR;

namespace Recycler.API;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order>
{
    public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        return new Order
        {
            OrderNumber = Guid.NewGuid(),
            OrderStatusId = 1,
            OrderStatus = new OrderStatus
            {
                Id = 1,
                Name = "Pending"
            },
            CreatedAt = DateTime.UtcNow,
            SupplierId = 1,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = 1,
                    OrderId = 1,
                    MaterialId = 1,
                    Quantity = 10,
                    Price = 25.50m,
                    RawMaterial = new RawMaterial
                    {
                        Id = 1,
                        Name = "Aluminum",
                        PricePerKg = 25.50m
                    }
                },
                new OrderItem
                {
                    Id = 2,
                    OrderId = 1,
                    MaterialId = 2,
                    Quantity = 5,
                    Price = 40.00m,
                    RawMaterial = new RawMaterial
                    {
                        Id = 2,
                        Name = "Copper",
                        PricePerKg = 40.00m
                    }
                }
            }
        };
    }
}